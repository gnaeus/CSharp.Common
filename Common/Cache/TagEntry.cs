using System.Collections.Concurrent;
using System.Threading;

namespace Common.Cache
{
    internal class TagEntry
    {
        private bool _isRemoved;

        public readonly ConcurrentDictionary<object, CacheEntry> CacheEntries;

        public TagEntry()
        {
            _isRemoved = false;

            CacheEntries = new ConcurrentDictionary<object, CacheEntry>();
        }
        
        public bool IsRemoved => Volatile.Read(ref _isRemoved);

        public void MarkAsRemoved()
        {
            Volatile.Write(ref _isRemoved, true);
        }
    }
}
