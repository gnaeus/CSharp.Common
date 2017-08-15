using System.Collections.Concurrent;
using System.Threading;

namespace Common.Cache
{
    internal class TagEntry
    {
        private bool _isExpired;

        public readonly ConcurrentDictionary<object, CacheEntry> CacheEntries;

        public TagEntry()
        {
            _isExpired = false;

            CacheEntries = new ConcurrentDictionary<object, CacheEntry>();
        }
        
        public bool IsExpired => Volatile.Read(ref _isExpired);

        public void MarkAsExpired()
        {
            Volatile.Write(ref _isExpired, true);
        }

        public bool CheckIfExpired()
        {
            if (IsExpired)
            {
                return true;
            }
            if (CacheEntries.IsEmpty)
            {
                MarkAsExpired();
                return true;
            }
            return false;
        }
    }
}
