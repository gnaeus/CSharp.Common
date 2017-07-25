using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    public class ConcurrentDictionaryCache : IConcurrentCache
    {
        readonly ICollection<KeyValuePair<object, IBaseEntry>> _storageCollection;

        readonly ConcurrentDictionary<object, IBaseEntry> _storage;

        readonly TimeSpan _expirationScanFrequency;

        readonly IEntryFactory _entryFactory;
        
        public ConcurrentDictionaryCache(IEntryFactory entryFactory, TimeSpan expirationScanFrequency)
        {
            if (entryFactory == null)
            {
                throw new ArgumentNullException(nameof(entryFactory));
            }
            if (expirationScanFrequency <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(expirationScanFrequency));
            }

            _entryFactory = entryFactory;

            _expirationScanFrequency = expirationScanFrequency;

            _storageCollection = _storage = new ConcurrentDictionary<object, IBaseEntry>();
        }

        public ConcurrentDictionaryCache(IEntryFactory entryFactory)
            : this(entryFactory, TimeSpan.FromMinutes(1)) { }

        public ConcurrentDictionaryCache()
            : this(new BlockingEntryFactory()) { }

        public bool TryGetValue<T>(object key, out T value)
        {
            IBaseEntry entry;
            if (!_storage.TryGetValue(key, out entry))
            {
                value = default(T);
                ScheduleScanForExpiredEntries();
                return false;
            }

            var cacheEntry = entry as ICacheEntry;
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

            IBaseEntry oldEntry = null;

            var newICacheEntry = (ICacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return _entryFactory.Create(key, tags, isSliding, lifetime, value);
            }, (_, existingEntry) =>
            {
                oldEntry = existingEntry;

                return _entryFactory.Create(key, tags, isSliding, lifetime, value);
            });

            AddToTags(newICacheEntry);

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

            IBaseEntry oldEntry = null;
            ICacheEntry newICacheEntry = null;

            var actualICacheEntry = (ICacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return newICacheEntry = _entryFactory.Create(key, tags, isSliding, lifetime, valueFactory);
            }, (_, existingEntry) =>
            {
                var existingICacheEntry = existingEntry as ICacheEntry;

                if (existingICacheEntry == null || existingICacheEntry.IsExpired)
                {
                    oldEntry = existingEntry;

                    return newICacheEntry = _entryFactory.Create(key, tags, isSliding, lifetime, valueFactory);
                }

                return existingICacheEntry;
            });

            if (actualICacheEntry == newICacheEntry)
            {
                AddToTags(newICacheEntry);
            }

            if (oldEntry != null)
            {
                RemoveFromDependencyGraph(oldEntry);
            }

            T value = actualICacheEntry.GetValue<T>();

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

            IBaseEntry oldEntry = null;
            ICacheEntry newICacheEntry = null;

            var actualICacheEntry = (ICacheEntry)_storage.AddOrUpdate(key, _ =>
            {
                return newICacheEntry = _entryFactory.Create(key, tags, isSliding, lifetime, taskFactory);
            }, (_, existingEntry) =>
            {
                var existingICacheEntry = existingEntry as ICacheEntry;

                if (existingICacheEntry == null || existingICacheEntry.IsExpired)
                {
                    oldEntry = existingEntry;

                    return newICacheEntry = _entryFactory.Create(key, tags, isSliding, lifetime, taskFactory);
                }

                return existingICacheEntry;
            });

            if (actualICacheEntry == newICacheEntry)
            {
                AddToTags(newICacheEntry);
            }

            if (oldEntry != null)
            {
                RemoveFromDependencyGraph(oldEntry);
            }

            Task<T> task = actualICacheEntry.GetTask<T>();

            ScheduleScanForExpiredEntries();

            return task;
        }
        
        private void AddToTags(ICacheEntry cacheEntry)
        {
            foreach (object tag in cacheEntry.Tags)
            {
                _storage.AddOrUpdate(tag, _ =>
                {
                    return _entryFactory.CreateBase(cacheEntry);
                }, (_, entry) =>
                {
                    entry.AddDerivedEntry(cacheEntry);

                    return entry;
                });
            }
        }

        private void RemoveFromTags(ICacheEntry cacheEntry)
        {
            foreach (object tag in cacheEntry.Tags)
            {
                IBaseEntry entry;
                if (!_storage.TryGetValue(tag, out entry))
                {
                    continue;
                }

                entry.RemoveDerivedEntry(cacheEntry);
            }
        }

        private void RemoveFromDependencyGraph(IBaseEntry entry)
        {
            var cacheEntry = entry as ICacheEntry;

            if (cacheEntry != null)
            {
                cacheEntry.MarkAsRemovedFromStorage();
                RemoveFromTags(cacheEntry);
            }

            entry.ForEachDerivedEntry(RemoveFromStorage);
        }
        
        private void RemoveFromStorage(ICacheEntry cacheEntry)
        {
            RemoveFromStorage(new KeyValuePair<object, IBaseEntry>(cacheEntry.Key, cacheEntry));
        }

        private void RemoveFromStorage(KeyValuePair<object, IBaseEntry> pair)
        {
            if (_storageCollection.Remove(pair))
            {
                RemoveFromDependencyGraph(pair.Value);
            }
        }

        public void Remove(object key)
        {
            IBaseEntry entry;
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
                    ThreadPool.QueueUserWorkItem(s => ScanForExpiredEntries((ConcurrentDictionaryCache)s), this);
                }
            }
        }

        private static void ScanForExpiredEntries(ConcurrentDictionaryCache cache)
        {
            foreach (var pair in cache._storage)
            {
                IBaseEntry entry = pair.Value;

                var cacheEntry = entry as ICacheEntry;

                if (cacheEntry != null)
                {
                    if (cacheEntry.IsExpired)
                    {
                        cache.RemoveFromStorage(pair);
                        continue;
                    }
                }
                else
                {
                    if (!entry.HasDerivedEntries)
                    {
                        cache.RemoveFromStorage(pair);
                        continue;
                    }
                }

                entry.ScanForRemovedDerivedEntries();
            }

            Volatile.Write(ref cache._cleanupIsRunning, 0);
        }
    }
}
