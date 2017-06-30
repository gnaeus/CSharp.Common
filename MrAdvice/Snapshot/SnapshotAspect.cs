using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MrAdvice.Aspects.Snapshot
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class SnapshotAspect : Attribute
    {
        public static AsyncLocal<SnapshotProfile> Profile = new AsyncLocal<SnapshotProfile>();
        
        protected virtual string GetFilePath(
            MethodBase targetMethod, IList<object> arguments, string folderPath)
        {
            StringBuilder sb = SerializeArguments(targetMethod, arguments);

            int folderLength = Path.GetFullPath(folderPath).Length;

            const int maxLength = 260;
            const int sha1Length = 28;
            int threshold = maxLength - folderLength - 7;

            if (sb.Length > threshold)
            {
                using (var sha1 = new SHA1Managed())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                    byte[] hash = sha1.ComputeHash(bytes);
                    sb.Length = threshold - sha1Length;
                    sb.Append(Convert.ToBase64String(hash));
                }
            }

            sb.Append(".json");
            return Path.Combine(folderPath, sb.ToString());
        }

        #region Arguments Serialization

        private static JsonSerializerSettings ArgumentsSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        private static char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        protected virtual StringBuilder SerializeArguments(
            MethodBase targetMethod, IList<object> arguments)
        {
            ParameterInfo[] parameters = targetMethod.GetParameters();

            Dictionary<string, object> argumentsDict = arguments
                .Zip(parameters, (a, p) => new
                {
                    Name = p.Name,
                    Value = a,
                })
                .ToDictionary(o => o.Name, o => o.Value);

            string path = JsonConvert.SerializeObject(argumentsDict, ArgumentsSettings);

            StringBuilder sb = new StringBuilder();
            foreach (char c in path)
            {
                if (c == ':')
                {
                    sb.Append('=');
                }
                else if (InvalidFileNameChars.Contains(c))
                {
                    continue;
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb;
        }

        #endregion

        #region Return Value Serialization

        private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
        };

        protected virtual object ReadReturnValue(MethodBase targetMethod, string filePath)
        {
            return CacheGetOrAdd(filePath, () =>
            {
                using (StreamReader reader = File.OpenText(filePath))
                // using (FileStream stream = File.OpenRead(filePath))
                // using (BsonDataReader reader = new BsonDataReader(stream))
                {
                    MethodInfo methodInfo = (MethodInfo)targetMethod;
                    JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                    return serializer.Deserialize(reader, methodInfo.ReturnType);
                }
            });
        }

        protected virtual void WriteReturnValue(
            object returnValue, MethodBase targetMethod, string folderPath, string filePath)
        {
            CacheRemove(filePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (StreamWriter writer = File.CreateText(filePath))
            // using (FileStream stream = File.OpenWrite(filePath))
            // using (BsonDataWriter writer = new BsonDataWriter(stream))
            {
                MethodInfo methodInfo = (MethodInfo)targetMethod;
                Type type = returnValue?.GetType() ?? methodInfo.ReturnType;

                JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                serializer.Serialize(writer, returnValue, type);
            }
        }

        protected virtual object ReadReturnTaskResult(MethodBase targetMethod, string filePath)
        {
            MethodInfo methodInfo = (MethodInfo)targetMethod;
            Type resultType = methodInfo.ReturnType.GetGenericArguments()[0];

            dynamic taskResult = CacheGetOrAdd(filePath, () =>
            {
                using (StreamReader reader = File.OpenText(filePath))
                // using (FileStream stream = File.OpenRead(filePath))
                // using (BsonDataReader reader = new BsonDataReader(stream))
                {
                    JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                    return serializer.Deserialize(reader, resultType);
                }
            });

            Type tcsType = typeof(TaskCompletionSource<>).MakeGenericType(resultType);
            dynamic tcs = Activator.CreateInstance(tcsType);
            tcs.SetResult(taskResult);

            return tcs.Task;
        }

        protected virtual void WriteReturnTaskResult(
            object returnValue, MethodBase targetMethod, string folderPath, string filePath)
        {
            CacheRemove(filePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (StreamWriter writer = File.CreateText(filePath))
            // using (FileStream stream = File.OpenWrite(filePath))
            // using (BsonDataWriter writer = new BsonDataWriter(stream))
            {
                MethodInfo methodInfo = (MethodInfo)targetMethod;
                Type taskType = returnValue?.GetType() ?? methodInfo.ReturnType;
                Type resultType = taskType.GetGenericArguments()[0];
                object taskResult = ((dynamic)returnValue).Result;

                JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                serializer.Serialize(writer, taskResult, resultType);
            }
        }

        #endregion

        #region Caching

        private static object CacheGetOrAdd(string key, Func<object> addItemFactory)
        {
            Lazy<object> newLazyCacheItem = new Lazy<object>(addItemFactory);

            CacheItemPolicy policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(10),
            };

            Lazy<object> existingCacheItem = MemoryCache.Default
                .AddOrGetExisting(key, newLazyCacheItem, policy) as Lazy<object>;

            if (existingCacheItem != null)
            {
                return existingCacheItem.Value;
            }

            try
            {
                return newLazyCacheItem.Value;
            }
            catch // addItemFactory errored so do not cache the exception
            {
                MemoryCache.Default.Remove(key);
                throw;
            }
        }

        private static void CacheRemove(string key)
        {
            MemoryCache.Default.Remove(key);
        }

        #endregion
    }
}
