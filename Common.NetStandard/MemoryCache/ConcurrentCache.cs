using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    public class ConcurrentCache : IConcurrentCache
    {
        readonly ConcurrentDictionary<object, TagEntry> _storage;

        readonly TimeSpan _expirationScanFrequency;
        
        public ConcurrentCache(TimeSpan expirationScanFrequency)
        {
            if (expirationScanFrequency <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(expirationScanFrequency));
            }

            _expirationScanFrequency = expirationScanFrequency;
            _storage = new ConcurrentDictionary<object, TagEntry>();
        }

        public ConcurrentCache()
            : this(TimeSpan.FromMinutes(1)) { }

        public bool TryGetValue<T>(object key, out T value)
        {
            TagEntry tagEntry;
            if (!_storage.TryGetValue(key, out tagEntry))
            {
                value = default(T);
                ScheduleScanForExpiredEntries();
                return false;
            }

            var cacheEntry = tagEntry as CacheEntry;
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

        public void AddOrUpdate<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, T value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));
            if (value == null) throw new ArgumentNullException(nameof(value));

            TagEntry oldTagEntry = null;

            var newCacheEntry = (CacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return CacheEntry.Create(key, tags, isSliding, lifetime, value);
            }, (_, existingTagEntry) =>
            {
                oldTagEntry = existingTagEntry;
                
                return CacheEntry.Create(key, tags, isSliding, lifetime, value);
            });
            
            ProcessTags(newCacheEntry, newCacheEntry, oldTagEntry);
            
            ScheduleScanForExpiredEntries();
        }

        public T GetOrAdd<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            CacheEntry newCacheEntry = null;
            TagEntry oldTagEntry = null;

            var actualCacheEntry = (CacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return newCacheEntry = CacheEntry.Create(key, tags, isSliding, lifetime, valueFactory);
            }, (_, existingTagEntry) =>
            {
                var existingCacheEntry = existingTagEntry as CacheEntry;

                if (existingCacheEntry == null || existingCacheEntry.IsExpired)
                {
                    oldTagEntry = existingTagEntry;

                    return newCacheEntry = CacheEntry.Create(key, tags, isSliding, lifetime, valueFactory);
                }

                return existingCacheEntry;
            });
            
            ProcessTags(actualCacheEntry, newCacheEntry, oldTagEntry);

            T value = actualCacheEntry.GetValue<T>();

            ScheduleScanForExpiredEntries();

            return value;
        }

        public Task<T> GetOrAddAsync<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));
            if (taskFactory == null) throw new ArgumentNullException(nameof(taskFactory));
            CacheEntry newCacheEntry = null;
            TagEntry oldTagEntry = null;

            var actualCacheEntry = (CacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return newCacheEntry = CacheEntry.Create(key, tags, isSliding, lifetime, taskFactory);
            }, (_, existingTagEntry) =>
            {
                var existingCacheEntry = existingTagEntry as CacheEntry;

                if (existingCacheEntry == null || existingCacheEntry.IsExpired)
                {
                    oldTagEntry = existingTagEntry;

                    return newCacheEntry = CacheEntry.Create(key, tags, isSliding, lifetime, taskFactory);
                }

                return existingCacheEntry;
            });
            
            ProcessTags(actualCacheEntry, newCacheEntry, oldTagEntry);

            Task<T> task = actualCacheEntry.GetTask<T>();

            ScheduleScanForExpiredEntries();

            return task;
        }

        /// <summary>
        /// Can run out of order, e.g. add to tags entry v2 and then add to tags entry v1.
        /// So we need to cleanup entries from all of DerivedEntries that are not present in _storage.
        /// </summary>
        private void ProcessTags(
            CacheEntry actualCacheEntry, CacheEntry newCacheEntry, TagEntry oldTagEntry)
        {
            // TODO: execute in background if needed

            if (actualCacheEntry == newCacheEntry)
            {
                AddToTags(newCacheEntry);
            }
            if (oldTagEntry != null)
            {
                var oldCacheEntry = oldTagEntry as CacheEntry;

                if (oldCacheEntry != null)
                {
                    RemoveFromTags(oldCacheEntry);
                }

                foreach (CacheEntry derivedEntry in oldTagEntry.DerivedEntries)
                {
                    Remove(derivedEntry.Key);
                }
            }
        }
        
        private void AddToTags(CacheEntry entry)
        {
            foreach (object tag in entry.Tags)
            {
                _storage.AddOrUpdate(tag, _ =>
                {
                    return new TagEntry(entry);
                }, (_, tagEntry) =>
                {
                    ImmutableHashSet<CacheEntry> derivedEntries, originalEntries;

                    do
                    {
                        derivedEntries = tagEntry.DerivedEntries;
                        originalEntries = Interlocked.CompareExchange(
                            ref tagEntry.DerivedEntries, derivedEntries.Add(entry), derivedEntries);
                    } while (originalEntries != derivedEntries);

                    return tagEntry;
                });
            }
        }

        private void RemoveFromTags(CacheEntry entry)
        {
            foreach (object tag in entry.Tags)
            {
                TagEntry tagEntry;
                if (!_storage.TryGetValue(tag, out tagEntry))
                {
                    continue;
                }

                ImmutableHashSet<CacheEntry> derivedEntries, originalEntries;

                do
                {
                    derivedEntries = tagEntry.DerivedEntries;
                    originalEntries = Interlocked.CompareExchange(
                        ref tagEntry.DerivedEntries, derivedEntries.Remove(entry), derivedEntries);
                } while (originalEntries != derivedEntries);
            }
        }

        public void Remove(object key)
        {
            TagEntry tagEntry;
            if (_storage.TryRemove(key, out tagEntry))
            {
                var cacheEntry = tagEntry as CacheEntry;

                if (cacheEntry != null)
                {
                    RemoveFromTags(cacheEntry);
                }

                foreach (CacheEntry derivedEntry in cacheEntry.DerivedEntries)
                {
                    Remove(derivedEntry.Key);
                }
            }

            ScheduleScanForExpiredEntries();
        }

        private long _lastExpirationScan = 0;
        private int _cleanupIsRunning = 0;

        private void ScheduleScanForExpiredEntries()
        {
            if ((DateTimeOffset.UtcNow - _expirationScanFrequency).Ticks > Volatile.Read(ref _lastExpirationScan))
            {
                if (Interlocked.CompareExchange(ref _cleanupIsRunning, 1, 0) == 0)
                {
                    Volatile.Write(ref _lastExpirationScan, DateTimeOffset.UtcNow.Ticks);
                    ThreadPool.QueueUserWorkItem(s => ScanForExpiredEntries((ConcurrentCache)s), this);
                }
            }
        }

        /// <summary>
        /// Check all entries for expiration.
        /// Check all entries from <see cref="TagEntry.DerivedEntries"/> for existance in <see cref="_storage"/>.
        /// Such entries can not resurrect, so we can remove it from <see cref="TagEntry.DerivedEntries"/>.
        /// </summary>
        private static void ScanForExpiredEntries(ConcurrentCache cache)
        {
            foreach (var pair in cache._storage)
            {
                TagEntry tagEntry = pair.Value;

                var cacheEntry = tagEntry as CacheEntry;

                if (cacheEntry != null)
                {
                    if (cacheEntry.IsExpired)
                    {
                        cache.Remove(pair.Key);
                        continue;
                    }
                }
                else
                {
                    if (tagEntry.DerivedEntries.IsEmpty)
                    {
                        cache.Remove(pair.Key);
                        continue;
                    }
                }

                foreach (CacheEntry derivedEntry in tagEntry.DerivedEntries)
                {
                    if (!cache._storage.ContainsKey(derivedEntry.Key))
                    {
                        // TODO: maybe try remove deriveEntry only once for speedup
                        // and if CAS is failed, then derivedEntry will be removed at the next scan
                        ImmutableHashSet<CacheEntry> derivedEntries, originalEntries;
                        do
                        {
                            derivedEntries = tagEntry.DerivedEntries;
                            originalEntries = Interlocked.CompareExchange(
                                ref tagEntry.DerivedEntries, derivedEntries.Remove(derivedEntry), derivedEntries);
                        } while (originalEntries != derivedEntries);
                    }
                }
            }

            Volatile.Write(ref cache._cleanupIsRunning, 0);
        }
    }
}
