using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Common.Cache
{
    internal class TagEntry
    {
        private bool _isEvicted;
        private bool _isRemoved;

        public readonly ConcurrentDictionary<CacheEntry, object> CacheEntries;

        /// <summary>
        /// Create already not empty <see cref="TagEntry"/>.
        /// </summary>
        public TagEntry(CacheEntry cacheEntry, object key)
        {
            _isEvicted = false;
            _isRemoved = false;

            CacheEntries = new ConcurrentDictionary<CacheEntry, object>();

            CacheEntries.TryAdd(cacheEntry, key);
        }
        
        public bool IsEvicted => Volatile.Read(ref _isEvicted);

        public void MarkAsEvicted()
        {
            Debug.Assert(Volatile.Read(ref _isEvicted) == false);
            Debug.Assert(Volatile.Read(ref _isRemoved) == false);

            Volatile.Write(ref _isEvicted, true);
        }

        public bool IsRemoved => Volatile.Read(ref _isRemoved);

        public void MarkAsRemoved()
        {
            Debug.Assert(Volatile.Read(ref _isEvicted) == false);
            Debug.Assert(Volatile.Read(ref _isRemoved) == false);

            Volatile.Write(ref _isRemoved, true);
        }

        public bool IsActive => !IsRemoved && !IsEvicted;
    }
}
