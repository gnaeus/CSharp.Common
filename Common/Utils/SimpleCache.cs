using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Utils
{
    /// <summary>
    /// Simple in-memory cache based on <see cref="ConcurrentDictionary{TKey, TValue}"/>
    /// for implementing "Cache Aside" pattern.
    /// <para />
    /// https://docs.microsoft.com/en-us/azure/architecture/patterns/cache-aside
    /// </summary>
    public class SimpleCache
    {
        private class CacheEntry
        {
            public object Data;
            public DateTime ExpiredUtc;
        }

        readonly ConcurrentDictionary<object, CacheEntry> _storage
            = new ConcurrentDictionary<object, CacheEntry>();

        readonly int _capacity;

        public SimpleCache(int capacity = 65536)
        {
            _capacity = capacity;
        }
        
        public void Add<T>(object key, TimeSpan lifetime, T value)
        {
            TryCleanupStorage();

            _storage[key] = new CacheEntry
            {
                Data = value,
                ExpiredUtc = DateTime.UtcNow + lifetime,
            };
        }
        
        public T GetOrAdd<T>(
            object key, bool isSliding, TimeSpan lifetime, Func<T> valueFactory)
        {
            TryCleanupStorage();

            DateTime utcNow = DateTime.UtcNow;
            DateTime expiredUtc = utcNow + lifetime;

            CacheEntry entry = _storage.AddOrUpdate(key, _ =>
            {
                return new CacheEntry
                {
                    Data = new Lazy<T>(valueFactory),
                    ExpiredUtc = expiredUtc,
                };
            }, (_, existing) =>
            {
                if (existing.ExpiredUtc < utcNow)
                {
                    return new CacheEntry
                    {
                        Data = new Lazy<T>(valueFactory),
                        ExpiredUtc = expiredUtc,
                    };
                }
                if (isSliding)
                {
                    existing.ExpiredUtc = expiredUtc;
                }
                return existing;
            });

            return Unwrap<T>(key, entry.Data);
        }
        
        public Task<T> GetOrAddAsync<T>(
            object key, bool isSliding, TimeSpan lifetime, Func<Task<T>> valueFactory)
        {
            TryCleanupStorage();

            DateTime utcNow = DateTime.UtcNow;
            DateTime expiredUtc = utcNow + lifetime;

            CacheEntry entry = _storage.AddOrUpdate(key, _ =>
            {
                return new CacheEntry
                {
                    Data = new AsyncLazy<T>(valueFactory),
                    ExpiredUtc = expiredUtc,
                };
            }, (_, existing) =>
            {
                if (existing.ExpiredUtc < utcNow)
                {
                    return new CacheEntry
                    {
                        Data = new AsyncLazy<T>(valueFactory),
                        ExpiredUtc = expiredUtc,
                    };
                }
                if (isSliding)
                {
                    existing.ExpiredUtc = expiredUtc;
                }
                return existing;
            });

            return UnwrapAsync<T>(key, entry.Data);
        }

        public void Remove(object key)
        {
            CacheEntry _;
            _storage.TryRemove(key, out _);
        }

        private T Unwrap<T>(object key, object data)
        {
            if (data is T)
            {
                return (T)data;
            }
            try
            {
                if (data is Lazy<T>)
                {
                    return ((Lazy<T>)data).Value;
                }
                return ((AsyncLazy<T>)data).Value
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                Remove(key);
                throw;
            }
        }

        private async Task<T> UnwrapAsync<T>(object key, object data)
        {
            if (data is T)
            {
                return (T)data;
            }
            try
            {
                if (data is Lazy<T>)
                {
                    return ((Lazy<T>)data).Value;
                }
                return await (AsyncLazy<T>)data;
            }
            catch
            {
                Remove(key);
                throw;
            }
        }
        
        private int _cleanupIsRunning;

        private void TryCleanupStorage()
        {
            if (_storage.Count >= _capacity)
            {
                if (Interlocked.CompareExchange(ref _cleanupIsRunning, 1, 0) == 0)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        DateTime utcNow = DateTime.UtcNow;
                        var store = (IDictionary<object, CacheEntry>)_storage;

                        foreach (var pair in _storage)
                        {
                            if (pair.Value.ExpiredUtc < utcNow)
                            {
                                store.Remove(pair.Key);
                            }
                        }
                        Interlocked.Exchange(ref _cleanupIsRunning, 0);
                    });
                }
            }
        }
    }
}
