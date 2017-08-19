using System.Collections.Concurrent;
using System.Threading;

namespace Common.Cache
{
    internal class TagEntry : ConcurrentDictionary<CacheEntry, object>
    {
        private bool _isRemoved;
        
        /// <summary>
        /// Create already not empty <see cref="TagEntry"/>.
        /// </summary>
        public TagEntry(CacheEntry cacheEntry, object key)
        {
            _isRemoved = false;
            
            TryAdd(cacheEntry, key);
        }
        
        public bool IsRemoved => Volatile.Read(ref _isRemoved);

        public void MarkAsRemoved()
        {
            Volatile.Write(ref _isRemoved, true);
        }

        public bool IsActive => !IsRemoved;
    }
}
