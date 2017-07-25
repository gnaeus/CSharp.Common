using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    internal class BaseEntry
    {
        private ImmutableHashSet<CacheEntry> _derivedEntries;
        
        protected BaseEntry()
        {
            _derivedEntries = ImmutableHashSet.Create<CacheEntry>();
        }

        public BaseEntry(CacheEntry cacheEntry)
        {
            _derivedEntries = ImmutableHashSet.Create(cacheEntry);
        }

        public ImmutableHashSet<CacheEntry> DerivedEntries => Volatile.Read(ref _derivedEntries);

        public void AddDerivedEntry(CacheEntry cacheEntry)
        {
            ImmutableHashSet<CacheEntry> derivedEntries, originalEntries;

            do
            {
                derivedEntries = Volatile.Read(ref _derivedEntries);
                originalEntries = Interlocked.CompareExchange(
                    ref _derivedEntries, derivedEntries.Add(cacheEntry), derivedEntries);
            } while (originalEntries != derivedEntries);
        }

        public void RemoveDerivedEntry(CacheEntry cacheEntry)
        {
            ImmutableHashSet<CacheEntry> derivedEntries, originalEntries;

            do
            {
                derivedEntries = Volatile.Read(ref _derivedEntries);
                originalEntries = Interlocked.CompareExchange(
                    ref _derivedEntries, derivedEntries.Remove(cacheEntry), derivedEntries);
            } while (originalEntries != derivedEntries);
        }
    }

    internal class CacheEntry : BaseEntry
    {
        public readonly object Key;
        public readonly object[] Tags;

        private readonly bool _isSliding;
        private readonly TimeSpan _lifetime;

        private readonly object _value;

        // DateTime or DateTimeOffset can not be volatile
        private long _expiredUtcTicks;
        private bool _isExpired;
        private bool _isRemovedFromStorage;
        
        private CacheEntry(
            object key, object[] tags,
            bool isSliding, TimeSpan lifetime,
            object value)
        {
            Key = key;
            Tags = tags ?? Array.Empty<object>();

            _isSliding = isSliding;
            _lifetime = lifetime;

            _value = value;

            // inside the constructor we not need a Volatile.Write
            _expiredUtcTicks = (DateTime.UtcNow + lifetime).Ticks;
            _isExpired = false;
            _isRemovedFromStorage = false;
        }
        
        public static CacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, T value)
        {
            return new CacheEntry(key, tags, isSliding, lifetime, value);
        }

        public static CacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory)
        {
            return new CacheEntry(key, tags, isSliding, lifetime, new LazyValue<T>(valueFactory));
        }

        public static CacheEntry Create<T>(
            object key, object[] tags,bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory)
        {
            return new CacheEntry(key, tags, isSliding, lifetime, new LazyTask<T>(taskFactory));
        }
        
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
                Volatile.Write(ref _expiredUtcTicks, (DateTime.UtcNow + _lifetime).Ticks);
            }
            if (_value is T)
            {
                return (T)_value;
            }
            try
            {
                if (_value is Lazy<T>)
                {
                    return ((LazyValue<T>)_value ).Value;
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
                Volatile.Write(ref _expiredUtcTicks, (DateTime.UtcNow + _lifetime).Ticks);
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

    internal class LazyValue<T> : Lazy<T>
    {
        public LazyValue(Func<T> valueFactory)
            : base(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication) { }
    }

    internal class LazyTask<T> : Lazy<Task<T>>
    {
        public LazyTask(Func<Task<T>> taskFactory)
            : base(() => Task.Factory.StartNew(taskFactory).Unwrap(),
                   LazyThreadSafetyMode.ExecutionAndPublication) { }
    }
}
