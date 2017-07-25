using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    internal class BlockingBaseEntry : IBaseEntry
    {
        private HashSet<ICacheEntry> _derivedEntries;

        protected BlockingBaseEntry() { }

        public BlockingBaseEntry(ICacheEntry cacheEntry)
        {
            _derivedEntries = new HashSet<ICacheEntry> { cacheEntry };
        }

        public bool HasDerivedEntries
        {
            get
            {
                lock (this)
                {
                    return _derivedEntries != null && _derivedEntries.Count > 0;
                }
            }
        }

        public void AddDerivedEntry(ICacheEntry cacheEntry)
        {
            lock (this)
            {
                if (_derivedEntries == null)
                {
                    _derivedEntries = new HashSet<ICacheEntry>();
                }
                _derivedEntries.Add(cacheEntry);
            }
        }

        public void RemoveDerivedEntry(ICacheEntry cacheEntry)
        {
            lock (this)
            {
                if (_derivedEntries != null)
                {
                    _derivedEntries.Remove(cacheEntry);
                }
            }
        }

        public void ForEachDerivedEntry(Action<ICacheEntry> action)
        {
            lock (this)
            {
                if (_derivedEntries == null)
                {
                    return;
                }
                foreach (ICacheEntry entry in _derivedEntries)
                {
                    action.Invoke(entry);
                }
            }
        }

        public void ScanForRemovedDerivedEntries()
        {
            lock (this)
            {
                if (_derivedEntries == null)
                {
                    return;
                }

                List<ICacheEntry> entriesToRemove = null;

                foreach (ICacheEntry entry in _derivedEntries)
                {
                    if (entry.IsRemovedFromStorage)
                    {
                        if (entriesToRemove == null)
                        {
                            entriesToRemove = new List<ICacheEntry>();
                        }
                        entriesToRemove.Add(entry);
                    }
                }

                if (entriesToRemove != null)
                {
                    _derivedEntries.ExceptWith(entriesToRemove);
                }
            }
        }
    }

    internal class BlockingCacheEntry : BlockingBaseEntry, ICacheEntry
    {
        private readonly object _key;
        private readonly object[] _tags;

        private readonly bool _isSliding;
        private readonly TimeSpan _lifetime;

        private readonly object _value;

        // DateTime or DateTimeOffset can not be volatile
        private long _expiredUtcTicks;
        private bool _isExpired;
        private bool _isRemovedFromStorage;

        public BlockingCacheEntry(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, object value)
        {
            _key = key;
            _tags = tags ?? Array.Empty<object>();

            _isSliding = isSliding;
            _lifetime = lifetime;

            _value = value;

            // inside the constructor we not need a Volatile.Write
            _expiredUtcTicks = ( DateTime.UtcNow + lifetime ).Ticks;
            _isExpired = false;
            _isRemovedFromStorage = false;
        }

        public object Key => _key;

        public object[] Tags => _tags;

        public bool IsExpired
        {
            get
            {
                if (Volatile.Read(ref _isExpired))
                {
                    return true;
                }
                if (Volatile.Read(ref _expiredUtcTicks) < DateTime.UtcNow.Ticks)
                {
                    Volatile.Write(ref _isExpired, true);
                    return true;
                }
                return false;
            }
        }

        public bool IsRemovedFromStorage => Volatile.Read(ref _isRemovedFromStorage);
        
        public void MarkAsRemovedFromStorage()
        {
            Volatile.Write(ref _isRemovedFromStorage, true);
        }

        public T GetValue<T>()
        {
            if (_isSliding)
            {
                Volatile.Write(ref _expiredUtcTicks, ( DateTime.UtcNow + _lifetime ).Ticks);
            }
            if (_value is T)
            {
                return (T)_value;
            }
            try
            {
                if (_value is Lazy<T>)
                {
                    return ((LazyValue<T>)_value).Value;
                }
                return ((LazyTask<T>)_value).Value
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                Volatile.Write(ref _isExpired, true);
                throw;
            }
        }

        public async Task<T> GetTask<T>()
        {
            if (_isSliding)
            {
                Volatile.Write(ref _expiredUtcTicks, ( DateTime.UtcNow + _lifetime ).Ticks);
            }
            if (_value is T)
            {
                return (T)_value;
            }
            try
            {
                if (_value is LazyValue<T>)
                {
                    return ((LazyValue<T>)_value).Value;
                }
                return await ((LazyTask<T>)_value).Value;
            }
            catch
            {
                Volatile.Write(ref _isExpired, true);
                throw;
            }
        }
    }

    public class BlockingEntryFactory : IEntryFactory
    {
        public IBaseEntry CreateBase(ICacheEntry cacheEntry)
        {
            return new BlockingBaseEntry(cacheEntry);
        }

        public ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, T value)
        {
            return new BlockingCacheEntry(key, tags, isSliding, lifetime, value);
        }

        public ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory)
        {
            return new BlockingCacheEntry(key, tags, isSliding, lifetime, new LazyValue<T>(valueFactory));
        }

        public ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory)
        {
            return new BlockingCacheEntry(key, tags, isSliding, lifetime, new LazyTask<T>(taskFactory));
        }
    }
}
