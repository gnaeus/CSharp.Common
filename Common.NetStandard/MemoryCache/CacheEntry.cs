using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    internal class TagEntry
    {
        public ImmutableHashSet<CacheEntry> DerivedEntries;

        protected TagEntry()
        {
            DerivedEntries = ImmutableHashSet.Create<CacheEntry>();
        }

        public TagEntry(CacheEntry cacheEntry)
        {
            DerivedEntries = ImmutableHashSet.Create(cacheEntry);
        }
    }

    internal class CacheEntry : TagEntry
    {
        public readonly object Key;
        public readonly object[] Tags;

        private readonly bool _isSliding;
        private readonly TimeSpan _lifetime;

        // writes to Int64 are atomic
        // writes to DateTimeOffset are not
        private long _expiredUtc;
        private bool _isExpired;

        private readonly object _value;
        
        private CacheEntry(
            object key, object[] tags,
            bool isSliding, TimeSpan lifetime,
            object value)
        {
            Key = key;
            Tags = tags ?? Array.Empty<object>();

            _isSliding = isSliding;
            _lifetime = lifetime;
            _expiredUtc = (DateTimeOffset.UtcNow + lifetime).Ticks;
            _isExpired = false;

            _value = value;
        }
        
        public static CacheEntry Create<T>(
            object key, object[] tags,
            bool isSliding, TimeSpan lifetime,
            T value)
        {
            return new CacheEntry(key, tags, isSliding, lifetime, value);
        }

        public static CacheEntry Create<T>(
            object key, object[] tags,
            bool isSliding, TimeSpan lifetime,
            Func<T> valueFactory)
        {
            return new CacheEntry(key, tags, isSliding, lifetime, new LazyValue<T>(valueFactory));
        }

        public static CacheEntry Create<T>(
            object key, object[] tags,
            bool isSliding, TimeSpan lifetime,
            Func<Task<T>> taskFactory)
        {
            return new CacheEntry(key, tags, isSliding, lifetime, new LazyTask<T>(taskFactory));
        }

        public bool IsExpired => _isExpired || (_isExpired = _expiredUtc < DateTimeOffset.UtcNow.Ticks);

        public T GetValue<T>()
        {
            if (_isSliding)
            {
                _expiredUtc = (DateTimeOffset.UtcNow + _lifetime).Ticks;
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
                _isExpired = true;
                throw;
            }
        }

        public async Task<T> GetTask<T>()
        {
            if (_isSliding)
            {
                _expiredUtc = (DateTimeOffset.UtcNow + _lifetime).Ticks;
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
                _isExpired = true;
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
