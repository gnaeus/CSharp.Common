using System.Collections.Concurrent;
using System.Threading;

namespace Common.Cache
{
    internal class TagEntry
    {
        private bool _isRemoved;

        public readonly ConcurrentDictionary<object, CacheEntry> CacheEntries;

        /// <summary>
        /// Create already not empty <see cref="TagEntry"/>.
        /// </summary>
        public TagEntry(object key, CacheEntry cacheEntry)
        {
            _isRemoved = false;

            CacheEntries = new ConcurrentDictionary<object, CacheEntry>();

            CacheEntries.TryAdd(key, cacheEntry);
        }
        
        public bool IsRemoved => Volatile.Read(ref _isRemoved);

        public void MarkAsRemoved()
        {
            Volatile.Write(ref _isRemoved, true);
        }
    }
}
