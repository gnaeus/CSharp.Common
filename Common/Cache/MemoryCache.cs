using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Cache
{
    public class MemoryCache : IMemoryCache
    {        
        readonly TimeSpan _expirationScanFrequency;

        readonly ConcurrentDictionary<object, CacheEntry> _cacheEntries;

        readonly ConcurrentDictionary<object, TagEntry> _tagEntries;

        public MemoryCache(TimeSpan expirationScanFrequency)
        {
            if (expirationScanFrequency <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(expirationScanFrequency));
            }

            _expirationScanFrequency = expirationScanFrequency;

            _cacheEntries = new ConcurrentDictionary<object, CacheEntry>();

            _tagEntries = new ConcurrentDictionary<object, TagEntry>();
        }

        public MemoryCache()
            : this(TimeSpan.FromMinutes(1))
        {
        }

        public bool TryGet<T>(object key, out T value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            ScheduleScanForExpiredEntries();

            CacheEntry cacheEntry;
            if (!_cacheEntries.TryGetValue(key, out cacheEntry))
            {
                value = default(T);
                return false;
            }

            if (cacheEntry.CheckIfExpired())
            {
                if (_cacheEntries.Remove(key, cacheEntry))
                {
                    RemoveFromAllTagEntries(cacheEntry);
                }
                
                value = default(T);
                return false;
            }

            value = cacheEntry.GetValue<T>();
            return true;
        }
        
        public void Add<T>(object key, object[] tags, bool isSliding, TimeSpan lifetime, T value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(lifetime));
            }

            ScheduleScanForExpiredEntries();

            CacheEntry createdEntry, deletedEntry = null;

            createdEntry = new CacheEntry(tags, isSliding, lifetime, value);
            
            _cacheEntries.AddOrUpdate(key, createdEntry, (_, updatedEntry) =>
            {
                updatedEntry.MarkAsExpired();

                deletedEntry = updatedEntry;

                return createdEntry;
            });

            if (deletedEntry != null)
            {
                RemoveFromAllTagEntries(deletedEntry);
            }

            AddToTagEntries(key, createdEntry);
        }

        public T GetOrAdd<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(lifetime));
            }
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            ScheduleScanForExpiredEntries();

            CacheEntry actualEntry, createdEntry;

            if (!_cacheEntries.TryGetValue(key, out actualEntry) || actualEntry.CheckIfExpired())
            {
                createdEntry = new CacheEntry(tags, isSliding, lifetime, new LazyValue<T>(valueFactory));

                actualEntry = GetOrAddCacheEntry(key, createdEntry);
            }

            return actualEntry.GetValue<T>();
        }

        public Task<T> GetOrAddAsync<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(lifetime));
            }
            if (taskFactory == null) throw new ArgumentNullException(nameof(taskFactory));

            ScheduleScanForExpiredEntries();

            CacheEntry actualEntry, createdEntry;

            if (!_cacheEntries.TryGetValue(key, out actualEntry) || actualEntry.CheckIfExpired())
            {
                createdEntry = new CacheEntry(tags, isSliding, lifetime, new LazyTask<T>(taskFactory));

                actualEntry = GetOrAddCacheEntry(key, createdEntry);
            }

            return actualEntry.GetTask<T>();
        }

        private CacheEntry GetOrAddCacheEntry(object key, CacheEntry createdEntry)
        {
            CacheEntry actualEntry, deletedEntry = null;

            actualEntry = _cacheEntries.AddOrUpdate(key, createdEntry, (_, updatedEntry) =>
            {
                if (updatedEntry.CheckIfExpired())
                {
                    deletedEntry = updatedEntry;
                    return createdEntry;
                }
                else
                {
                    deletedEntry = null;
                    return updatedEntry;
                }
            });

            if (deletedEntry != null)
            {
                RemoveFromAllTagEntries(deletedEntry);
            }

            if (actualEntry == createdEntry)
            {
                AddToTagEntries(key, createdEntry);
            }
            
            return actualEntry;
        }

        private void AddToTagEntries(object key, CacheEntry cacheEntry)
        {
            if (cacheEntry.Tags != null)
            {
                var tagEntries = new List<TagEntry>(cacheEntry.Tags.Length);

                foreach (object tag in cacheEntry.Tags)
                {
                    TagEntry tagEntry = _tagEntries.GetOrAdd(tag, _ => new TagEntry(cacheEntry, key));

                    tagEntry.TryAdd(cacheEntry, key);

                    tagEntries.Add(tagEntry);
                }
                
                if (cacheEntry.IsExpired || tagEntries.Any(tagEntry => tagEntry.IsRemoved))
                {
                    if (_cacheEntries.Remove(key, cacheEntry))
                    {
                        cacheEntry.MarkAsExpired();
                    }
                    
                    RemoveFromTagEntries(tagEntries, cacheEntry);
                }
            }
        }

        private void RemoveFromTagEntries(List<TagEntry> tagEntries, CacheEntry cacheEntry)
        {
            foreach (TagEntry tagEntry in tagEntries)
            {
                if (tagEntry.IsActive)
                {
                    object key;
                    tagEntry.TryRemove(cacheEntry, out key);
                }
            }
        }

        private void RemoveFromAllTagEntries(CacheEntry cacheEntry)
        {
            if (cacheEntry.Tags != null)
            {
                foreach (object tag in cacheEntry.Tags)
                {
                    TagEntry tagEntry;
                    if (_tagEntries.TryGetValue(tag, out tagEntry) && tagEntry.IsActive)
                    {
                        object key;
                        tagEntry.TryRemove(cacheEntry, out key);
                    }
                }
            }
        }
        
        public void Remove(object key)
        {
            CacheEntry cacheEntry;
            if (_cacheEntries.TryRemove(key, out cacheEntry))
            {
                cacheEntry.MarkAsExpired();

                RemoveFromAllTagEntries(cacheEntry);
            }
        }
        
        public void RemoveByTag(object tag)
        {
            TagEntry tagEntry;
            if (_tagEntries.TryRemove(tag, out tagEntry))
            {
                tagEntry.MarkAsRemoved();

                RemoveLinkedCacheEntries(tagEntry);
            }
        }

        private void RemoveLinkedCacheEntries(TagEntry tagEntry)
        {
            foreach (var cachePair in tagEntry)
            {
                object key = cachePair.Value;
                CacheEntry cacheEntry = cachePair.Key;
                
                if (_cacheEntries.Remove(key, cacheEntry))
                {
                    cacheEntry.MarkAsExpired();

                    RemoveFromAllTagEntries(cacheEntry);
                }
            }
        }

        private long _lastExpirationScanTicks = 0;

        private int _cleanupIsRunning = 0;

        private void ScheduleScanForExpiredEntries()
        {
            long nextExpirationScanTicks = (DateTime.UtcNow - _expirationScanFrequency).Ticks;

            if (nextExpirationScanTicks > Volatile.Read(ref _lastExpirationScanTicks))
            {
                if (Interlocked.CompareExchange(ref _cleanupIsRunning, 1, 0) == 0)
                {
                    Volatile.Write(ref _lastExpirationScanTicks, DateTime.UtcNow.Ticks);

                    ThreadPool.QueueUserWorkItem(state => ScanForExpiredEntries((MemoryCache)state), this);
                }
            }
        }

        private static void ScanForExpiredEntries(MemoryCache cache)
        {
            DateTime utcNow = DateTime.UtcNow;

            foreach (var cachePair in cache._cacheEntries)
            {
                object key = cachePair.Key;
                CacheEntry cacheEntry = cachePair.Value;

                if (cacheEntry.CheckIfExpired(utcNow))
                {
                    if (cache._cacheEntries.Remove(key, cacheEntry))
                    {
                        cache.RemoveFromAllTagEntries(cacheEntry);
                    }
                }
            }

            foreach (var tagPair in cache._tagEntries)
            {
                object tag = tagPair.Key;
                TagEntry tagEntry = tagPair.Value;

                if (tagEntry.IsEmpty)
                {
                    if (cache._tagEntries.Remove(tag, tagEntry))
                    {
                        tagEntry.MarkAsRemoved();

                        cache.RemoveLinkedCacheEntries(tagEntry);
                    }
                }
            }

            Volatile.Write(ref cache._cleanupIsRunning, 0);
        }
    }
}
