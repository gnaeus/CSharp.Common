using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class MemoryCacheExtensions
    {
        private class LazyTask<T> : Lazy<Task<T>>
        {
            public LazyTask(Func<Task<T>> taskFactory)
                : base(() => Task.Factory.StartNew(taskFactory).Unwrap(),
                       LazyThreadSafetyMode.ExecutionAndPublication)
            {
            }
        }

        public static Task<T> GetOrAddAsync<T>(
            this ObjectCache cache, string key, DateTimeOffset absoluteExpiration, Func<Task<T>> valueFactory)
        {
            return GetOrAddAsync(
                cache, key, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, valueFactory);
        }

        public static Task<T> GetOrAddAsync<T>(
            this ObjectCache cache, string key, TimeSpan slidingExpiration, Func<Task<T>> valueFactory)
        {
            return GetOrAddAsync(
                cache, key, new CacheItemPolicy { SlidingExpiration = slidingExpiration }, valueFactory);
        }

        private static async Task<T> GetOrAddAsync<T>(
            ObjectCache cache, string key, CacheItemPolicy policy, Func<Task<T>> valueFactory)
        {
            var newEntry = new LazyTask<T>(valueFactory);

            if (cache.AddOrGetExisting(key, newEntry, policy) is LazyTask<T> existingEntry)
            {
                return await existingEntry.Value;
            }

            try
            {
                Task<T> result = newEntry.Value;

                if (result.IsCanceled || result.IsFaulted)
                {
                    cache.Remove(key);
                }

                return await result;
            }
            catch
            {
                cache.Remove(key);
                throw;
            }
        }
    }
}
