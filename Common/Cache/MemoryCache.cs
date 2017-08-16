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
                RemoveCacheEntry(key, cacheEntry);

                value = default(T);
                return false;
            }

            value = cacheEntry.GetValue<T>();
            return true;
        }

        const int VersionsMask = 0xff;

        readonly long[] _versions = new long[255];

        private CacheEntry CreateCacheEntry(object key, bool isSliding, TimeSpan lifetime, object value)
        {
            long version = Interlocked.Increment(ref _versions[key.GetHashCode() & VersionsMask]);

            return new CacheEntry(version, isSliding, lifetime, value);
        }

        private CacheEntry AddCacheEntry(object key, object[] tags, CacheEntry cacheEntry)
        {
            CacheEntry actualEntry, deletedEntry = null;

            actualEntry = _cacheEntries.AddOrUpdate(key, cacheEntry, (_, updatedEntry) =>
            {
                if (updatedEntry.Version < cacheEntry.Version)
                {
                    deletedEntry = updatedEntry;
                    return cacheEntry;
                }
                else
                {
                    deletedEntry = null;
                    return updatedEntry;
                }
            });

            if (deletedEntry != null)
            {
                deletedEntry.MarkAsExpired();

                RemoveFromTagEntries(key, deletedEntry);
            }

            if (tags != null && actualEntry == cacheEntry)
            {
                AddToTagEntries(key, tags, cacheEntry);
            }

            return actualEntry;
        }

        private void RemoveCacheEntry(object key, CacheEntry cacheEntry)
        {
            cacheEntry.MarkAsExpired();

            _cacheEntries.Remove(key, cacheEntry);

            RemoveFromTagEntries(key, cacheEntry);
        }
        
        private void AddToTagEntries(object key, object[] tags, CacheEntry cacheEntry)
        {
            cacheEntry.TagEntries = new List<TagEntry>(tags.Length);

            foreach (object tag in tags)
            {
                TagEntry tagEntry = _tagEntries.GetOrAdd(key, _ => new TagEntry());

                CacheEntry actualEntry;
                if (!tagEntry.CacheEntries.TryGetValue(key, out actualEntry) || actualEntry != cacheEntry)
                {
                    tagEntry.CacheEntries.AddOrUpdate(key, cacheEntry, (_, updatedEntry) =>
                    {
                        return updatedEntry.Version < cacheEntry.Version ? cacheEntry : updatedEntry;
                    });
                }
                
                cacheEntry.TagEntries.Add(tagEntry);
            }

            if (cacheEntry.TagEntries.Any(tagEntry => tagEntry.IsExpired))
            {
                RemoveCacheEntry(key, cacheEntry); 
            }
        }

        private static void RemoveFromTagEntries(object key, CacheEntry cacheEntry)
        {
            if (cacheEntry.TagEntries != null)
            {
                foreach (TagEntry tagEntry in cacheEntry.TagEntries)
                {
                    if (!tagEntry.IsExpired)
                    {
                        tagEntry.CacheEntries.Remove(key, cacheEntry);
                    }
                }
            }
        }
        
        public void Add<T>(object key, object[] tags, bool isSliding, TimeSpan lifetime, T value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (lifetime <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(lifetime));
            }

            ScheduleScanForExpiredEntries();

            CacheEntry createdEntry = CreateCacheEntry(key, isSliding, lifetime, value);

            AddCacheEntry(key, tags, createdEntry);
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

            CacheEntry actualEntry;
            if (!_cacheEntries.TryGetValue(key, out actualEntry) || actualEntry.CheckIfExpired())
            {
                CacheEntry createdEntry = CreateCacheEntry(
                    key, isSliding, lifetime, new LazyValue<T>(valueFactory));

                actualEntry = AddCacheEntry(key, tags, createdEntry);
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

            CacheEntry actualEntry;
            if (!_cacheEntries.TryGetValue(key, out actualEntry) || actualEntry.CheckIfExpired())
            {
                CacheEntry createdEntry = CreateCacheEntry(
                    key, isSliding, lifetime, new LazyTask<T>(taskFactory));

                actualEntry = AddCacheEntry(key, tags, createdEntry);
            }

            return actualEntry.GetTask<T>();
        }

        public void Remove(object key)
        {
            CacheEntry cacheEntry;
            if (_cacheEntries.TryRemove(key, out cacheEntry))
            {
                cacheEntry.MarkAsExpired();

                RemoveFromTagEntries(key, cacheEntry);
            }
        }

        public void RemoveByTag(object tag)
        {
            TagEntry tagEntry;
            if (_tagEntries.TryRemove(tag, out tagEntry))
            {
                tagEntry.MarkAsExpired();

                foreach (var cachePair in tagEntry.CacheEntries)
                {
                    RemoveCacheEntry(cachePair.Key, cachePair.Value);
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
                if (cachePair.Value.CheckIfExpired(utcNow))
                {
                    cache.RemoveCacheEntry(cachePair.Key, cachePair.Value);
                }
            }

            foreach (var tagPair in cache._tagEntries)
            {
                if (tagPair.Value.CheckIfExpired())
                {
                    cache._tagEntries.Remove(tagPair);

                    foreach (var cachePair in tagPair.Value.CacheEntries)
                    {
                        cache.RemoveCacheEntry(cachePair.Key, cachePair.Value);
                    }
                }
            }

            Volatile.Write(ref cache._cleanupIsRunning, 0);
        }
    }
}
