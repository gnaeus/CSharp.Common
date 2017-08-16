using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Cache
{
    public static class Extensions
    {
        #region ConcurrentDictionary

        public static bool Remove<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            return dictionary.Remove(new KeyValuePair<TKey, TValue>(key, value));
        }

        public static bool Remove<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> pair)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(pair);
        }

        #endregion

        public static T Get<T>(this IMemoryCache cache, object key)
        {
            T value;
            cache.TryGet(key, out value);
            return value;
        }

        public static void Add<T>(
            this IMemoryCache cache, object key, object[] tags, TimeSpan absoluteLifetime, T value)
        {
            cache.Add(key, tags, false, absoluteLifetime, value);
        }

        public static void Add<T>(
            this IMemoryCache cache, object key, object[] tags, DateTime absoluteExpiration, T value)
        {
            TimeSpan lifetime = absoluteExpiration.Kind == DateTimeKind.Utc
                ? absoluteExpiration - DateTime.UtcNow
                : absoluteExpiration - DateTime.Now;

            cache.Add(key, tags, false, lifetime, value);
        }

        public static void Add<T>(
            this IMemoryCache cache, object key, bool isSliding, TimeSpan lifetime, T value)
        {
            cache.Add(key, null, isSliding, lifetime, value);
        }

        public static void Add<T>(
            this IMemoryCache cache, object key, TimeSpan absoluteLifetime, T value)
        {
            cache.Add(key, false, absoluteLifetime, value);
        }

        public static void Add<T>(
            this IMemoryCache cache, object key, DateTime absoluteExpiration, T value)
        {
            TimeSpan lifetime = absoluteExpiration.Kind == DateTimeKind.Utc
                ? absoluteExpiration - DateTime.UtcNow
                : absoluteExpiration - DateTime.Now;

            cache.Add(key, false, lifetime, value);
        }

        public static T GetOrAdd<T>(
            this IMemoryCache cache, object key, object[] tags, TimeSpan absoluteLifetime, Func<T> valueFactory)
        {
            return cache.GetOrAdd(key, tags, false, absoluteLifetime, valueFactory);
        }

        public static T GetOrAdd<T>(
            this IMemoryCache cache, object key, object[] tags, DateTime absoluteExpiration, Func<T> valueFactory)
        {
            TimeSpan lifetime = absoluteExpiration.Kind == DateTimeKind.Utc
                ? absoluteExpiration - DateTime.UtcNow
                : absoluteExpiration - DateTime.Now;

            return cache.GetOrAdd(key, tags, false, lifetime, valueFactory);
        }

        public static T GetOrAdd<T>(
            this IMemoryCache cache, object key, bool isSliding, TimeSpan lifetime, Func<T> valueFactory)
        {
            return cache.GetOrAdd(key, null, isSliding, lifetime, valueFactory);
        }

        public static T GetOrAdd<T>(
            this IMemoryCache cache, object key, TimeSpan absoluteLifetime, Func<T> valueFactory)
        {
            return cache.GetOrAdd(key, false, absoluteLifetime, valueFactory);
        }

        public static T GetOrAdd<T>(
            this IMemoryCache cache, object key, DateTime absoluteExpiration, Func<T> valueFactory)
        {
            TimeSpan lifetime = absoluteExpiration.Kind == DateTimeKind.Utc
                ? absoluteExpiration - DateTime.UtcNow
                : absoluteExpiration - DateTime.Now;

            return cache.GetOrAdd(key, false, lifetime, valueFactory);
        }

        public static Task<T> GetOrAddAsync<T>(
            this IMemoryCache cache, object key, object[] tags, TimeSpan absoluteLifetime, Func<Task<T>> taskFactory)
        {
            return cache.GetOrAddAsync(key, tags, false, absoluteLifetime, taskFactory);
        }

        public static Task<T> GetOrAddAsync<T>(
            this IMemoryCache cache, object key, object[] tags, DateTime absoluteExpiration, Func<Task<T>> taskFactory)
        {
            TimeSpan lifetime = absoluteExpiration.Kind == DateTimeKind.Utc
                ? absoluteExpiration - DateTime.UtcNow
                : absoluteExpiration - DateTime.Now;

            return cache.GetOrAddAsync(key, tags, false, lifetime, taskFactory);
        }

        public static Task<T> GetOrAddAsync<T>(
            this IMemoryCache cache, object key, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory)
        {
            return cache.GetOrAddAsync(key, null, isSliding, lifetime, taskFactory);
        }

        public static Task<T> GetOrAddAsync<T>(
            this IMemoryCache cache, object key, TimeSpan absoluteLifetime, Func<Task<T>> taskFactory)
        {
            return cache.GetOrAddAsync(key, false, absoluteLifetime, taskFactory);
        }

        public static Task<T> GetOrAddAsync<T>(
            this IMemoryCache cache, object key, DateTime absoluteExpiration, Func<Task<T>> taskFactory)
        {
            TimeSpan lifetime = absoluteExpiration.Kind == DateTimeKind.Utc
                ? absoluteExpiration - DateTime.UtcNow
                : absoluteExpiration - DateTime.Now;

            return cache.GetOrAddAsync(key, false, lifetime, taskFactory);
        }

        public static void Remove(this IMemoryCache cache, params object[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            foreach (object key in keys)
            {
                cache.Remove(key);
            }
        }

        public static void Remove(this IMemoryCache cache, IEnumerable<object> keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            foreach (object key in keys)
            {
                cache.Remove(key);
            }
        }

        public static void RemoveByTag(this IMemoryCache cache, params object[] tags)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));

            foreach (object tag in tags)
            {
                cache.RemoveByTag(tag);
            }
        }

        public static void RemoveByTag(this IMemoryCache cache, IEnumerable<object> tags)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));

            foreach (object tag in tags)
            {
                cache.RemoveByTag(tag);
            }
        }
    }
}
