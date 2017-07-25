using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    public class ConcurrentCache : IConcurrentCache
    {
        readonly ConcurrentDictionary<object, BaseEntry> _storage;
        
        readonly TimeSpan _expirationScanFrequency;
        
        public ConcurrentCache(TimeSpan expirationScanFrequency)
        {
            if (expirationScanFrequency <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(expirationScanFrequency));
            }

            _expirationScanFrequency = expirationScanFrequency;
            
            _storage = new ConcurrentDictionary<object, BaseEntry>();
        }

        public ConcurrentCache()
            : this(TimeSpan.FromMinutes(1)) { }
        
        public bool TryGetValue<T>(object key, out T value)
        {
            BaseEntry entry;
            if (!_storage.TryGetValue(key, out entry))
            {
                value = default(T);
                ScheduleScanForExpiredEntries();
                return false;
            }

            var cacheEntry = entry as CacheEntry;
            if (cacheEntry == null)
            {
                value = default(T);
                ScheduleScanForExpiredEntries();
                return false;
            }

            value = cacheEntry.GetValue<T>();
            ScheduleScanForExpiredEntries();
            return true;
        }

        /// <summary>
        /// Value guaranteed to be removed by <paramref name="tags"/> only after AddOrUpdate will be completed.
        /// </summary>
        public void AddOrUpdate<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, T value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));
            if (value == null) throw new ArgumentNullException(nameof(value));

            BaseEntry oldEntry = null;

            var newCacheEntry = (CacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return CacheEntry.Create(key, tags, isSliding, lifetime, value);
            }, (_, existingEntry) =>
            {
                oldEntry = existingEntry;

                return CacheEntry.Create(key, tags, isSliding, lifetime, value);
            });

            AddToTags(newCacheEntry);

            if (oldEntry != null)
            {
                RemoveFromDependencyGraph(oldEntry);
            }

            ScheduleScanForExpiredEntries();
        }

        /// <summary>
        /// Value guaranteed to be removed by <paramref name="tags"/> only after GetOrAdd will be completed.
        /// </summary>
        public T GetOrAdd<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            BaseEntry oldEntry = null;
            CacheEntry newCacheEntry = null;

            var actualCacheEntry = (CacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return newCacheEntry = CacheEntry.Create(key, tags, isSliding, lifetime, valueFactory);
            }, (_, existingEntry) =>
            {
                var existingCacheEntry = existingEntry as CacheEntry;

                if (existingCacheEntry == null || existingCacheEntry.IsExpired)
                {
                    oldEntry = existingEntry;

                    return newCacheEntry = CacheEntry.Create(key, tags, isSliding, lifetime, valueFactory);
                }

                return existingCacheEntry;
            });

            if (actualCacheEntry == newCacheEntry)
            {
                AddToTags(newCacheEntry);
            }

            if (oldEntry != null)
            {
                RemoveFromDependencyGraph(oldEntry);
            }

            T value = actualCacheEntry.GetValue<T>();

            ScheduleScanForExpiredEntries();

            return value;
        }

        /// <summary>
        /// Value guaranteed to be removed by <paramref name="tags"/> only after GetOrAdd will be completed.
        /// </summary>
        public Task<T> GetOrAddAsync<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));
            if (taskFactory == null) throw new ArgumentNullException(nameof(taskFactory));

            BaseEntry oldEntry = null;
            CacheEntry newCacheEntry = null;

            var actualCacheEntry = (CacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return newCacheEntry = CacheEntry.Create(key, tags, isSliding, lifetime, taskFactory);
            }, (_, existingEntry) =>
            {
                var existingCacheEntry = existingEntry as CacheEntry;

                if (existingCacheEntry == null || existingCacheEntry.IsExpired)
                {
                    oldEntry = existingEntry;

                    return newCacheEntry = CacheEntry.Create(key, tags, isSliding, lifetime, taskFactory);
                }

                return existingCacheEntry;
            });

            if (actualCacheEntry == newCacheEntry)
            {
                AddToTags(newCacheEntry);
            }

            if (oldEntry != null)
            {
                RemoveFromDependencyGraph(oldEntry);
            }

            Task<T> task = actualCacheEntry.GetTask<T>();

            ScheduleScanForExpiredEntries();

            return task;
        }
        
        private void AddToTags(CacheEntry cacheEntry)
        {
            foreach (object tag in cacheEntry.Tags)
            {
                _storage.AddOrUpdate(tag, _ =>
                {
                    return new BaseEntry(cacheEntry);
                }, (_, entry) =>
                {
                    entry.AddDerivedEntry(cacheEntry);

                    return entry;
                });
            }
        }

        private void RemoveFromTags(CacheEntry cacheEntry)
        {
            foreach (object tag in cacheEntry.Tags)
            {
                BaseEntry entry;
                if (!_storage.TryGetValue(tag, out entry))
                {
                    continue;
                }

                entry.RemoveDerivedEntry(cacheEntry);
            }
        }

        private void RemoveFromDependencyGraph(BaseEntry entry)
        {
            var cacheEntry = entry as CacheEntry;

            if (cacheEntry != null)
            {
                cacheEntry.MarkAsRemovedFromStorage();
                RemoveFromTags(cacheEntry);
            }

            foreach (CacheEntry derivedEntry in entry.DerivedEntries)
            {
                RemoveFromStorage(derivedEntry.Key, derivedEntry);
            }
        }

        private void RemoveFromStorage(object key, BaseEntry entry)
        {
            var storage = (ICollection<KeyValuePair<object, BaseEntry>>)_storage;

            var pair = new KeyValuePair<object, BaseEntry>(key, entry);

            if (storage.Remove(pair))
            {
                RemoveFromDependencyGraph(entry);
            }
        }
        
        public void Remove(object key)
        {
            BaseEntry entry;
            if (_storage.TryRemove(key, out entry))
            {
                RemoveFromDependencyGraph(entry);
            }

            ScheduleScanForExpiredEntries();
        }

        private long _lastExpirationScan = 0;
        private int _cleanupIsRunning = 0;

        private void ScheduleScanForExpiredEntries()
        {
            if ((DateTime.UtcNow - _expirationScanFrequency).Ticks > Volatile.Read(ref _lastExpirationScan))
            {
                if (Interlocked.CompareExchange(ref _cleanupIsRunning, 1, 0) == 0)
                {
                    Volatile.Write(ref _lastExpirationScan, DateTime.UtcNow.Ticks);
                    ThreadPool.QueueUserWorkItem(s => ScanForExpiredEntries((ConcurrentCache)s), this);
                }
            }
        }

        private static void ScanForExpiredEntries(ConcurrentCache cache)
        {
            foreach (var pair in cache._storage)
            {
                BaseEntry entry = pair.Value;

                var cacheEntry = entry as CacheEntry;

                if (cacheEntry != null)
                {
                    if (cacheEntry.IsExpired)
                    {
                        cache.RemoveFromStorage(pair.Key, pair.Value);
                        continue;
                    }
                }
                else
                {
                    if (entry.DerivedEntries.IsEmpty)
                    {
                        cache.RemoveFromStorage(pair.Key, pair.Value);
                        continue;
                    }
                }

                foreach (CacheEntry derivedEntry in entry.DerivedEntries)
                {
                    if (derivedEntry.IsRemovedFromStorage)
                    {
                        entry.RemoveDerivedEntry(derivedEntry);
                    }
                }
            }

            Volatile.Write(ref cache._cleanupIsRunning, 0);
        }
    }
}
