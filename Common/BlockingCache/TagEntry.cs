using System.Collections.Generic;
using System.Threading;

namespace Common.BlockingCache
{
    internal class TagEntry : HashSet<CacheEntry>
    {
        private bool _isRemoved;
        private bool _isEvicted;
        
        /// <summary>
        /// Create already not empty <see cref="TagEntry"/>.
        /// </summary>
        public TagEntry(CacheEntry cacheEntry)
        {
            _isRemoved = false;
            _isEvicted = false;
            
            Add(cacheEntry);
        }

        public bool IsRemoved => Volatile.Read(ref _isRemoved);

        public void MarkAsRemoved()
        {
            Volatile.Write(ref _isRemoved, true);
        }

        public bool IsEvicted => Volatile.Read(ref _isEvicted);

        public void MarkAsEvicted()
        {
            Volatile.Write(ref _isEvicted, true);
        }

        public bool IsActive => !IsRemoved && !IsEvicted;
    }
}
