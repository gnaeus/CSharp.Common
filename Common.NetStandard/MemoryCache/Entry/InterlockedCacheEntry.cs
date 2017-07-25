using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    internal class InterlockedBaseEntry : IBaseEntry
    {
        private ImmutableHashSet<ICacheEntry> _derivedEntries;

        protected InterlockedBaseEntry()
        {
            _derivedEntries = ImmutableHashSet.Create<ICacheEntry>();
        }

        public InterlockedBaseEntry(ICacheEntry cacheEntry)
        {
            _derivedEntries = ImmutableHashSet.Create(cacheEntry);
        }

        public bool HasDerivedEntries => !Volatile.Read(ref _derivedEntries).IsEmpty;

        public void AddDerivedEntry(ICacheEntry cacheEntry)
        {
            ImmutableHashSet<ICacheEntry> derivedEntries, originalEntries;

            do
            {
                derivedEntries = Volatile.Read(ref _derivedEntries);
                originalEntries = Interlocked.CompareExchange(
                    ref _derivedEntries, derivedEntries.Add(cacheEntry), derivedEntries);
            } while (originalEntries != derivedEntries);
        }

        public void RemoveDerivedEntry(ICacheEntry cacheEntry)
        {
            ImmutableHashSet<ICacheEntry> derivedEntries, originalEntries;

            do
            {
                derivedEntries = Volatile.Read(ref _derivedEntries);
                originalEntries = Interlocked.CompareExchange(
                    ref _derivedEntries, derivedEntries.Remove(cacheEntry), derivedEntries);
            } while (originalEntries != derivedEntries);
        }

        public void ForEachDerivedEntry(Action<ICacheEntry> action)
        {
            foreach (ICacheEntry entry in Volatile.Read(ref _derivedEntries))
            {
                action.Invoke(entry);
            }
        }

        public void ScanForRemovedDerivedEntries()
        {
            foreach (ICacheEntry entry in Volatile.Read(ref _derivedEntries))
            {
                RemoveDerivedEntry(entry);
            }
        }
    }

    internal class InterlockedCacheEntry : InterlockedBaseEntry, ICacheEntry
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

        public InterlockedCacheEntry(
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
}
