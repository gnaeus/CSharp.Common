using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Cache
{
    internal class CacheEntry
    {
        private readonly object _value;
        private readonly TimeSpan _lifetime;
        private readonly bool _isSliding;

        private bool _isExpired;
        private long _expiredTicks;
        
        public HashSet<TagEntry> TagEntries;

        public CacheEntry(bool isSliding, TimeSpan lifetime, object value)
        {
            _value = value;
            _lifetime = lifetime;
            _isSliding = isSliding;

            _isExpired = false;
            _expiredTicks = (DateTime.UtcNow + lifetime).Ticks;
        }

        public bool IsExpired => Volatile.Read(ref _isExpired);

        public void MarkAsExpired()
        {
            Volatile.Write(ref _isExpired, true);
        }

        public bool CheckIfExpired()
        {
            return CheckIfExpired(DateTime.UtcNow);
        }

        public bool CheckIfExpired(DateTime utcNow)
        {
            if (IsExpired)
            {
                return true;
            }
            if (Volatile.Read(ref _expiredTicks) < utcNow.Ticks)
            {
                MarkAsExpired();
                return true;
            }
            return false;
        }

        public T GetValue<T>()
        {
            if (_isSliding)
            {
                Volatile.Write(ref _expiredTicks, (DateTime.UtcNow + _lifetime).Ticks);
            }
            try
            {
                if (_value is ILazyTask)
                {
                    return ((LazyTask<T>)_value).Value
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                }
                if (_value is ILazyValue)
                {
                    return ((LazyValue<T>)_value).Value;
                }
            }
            catch
            {
                MarkAsExpired();
                throw;
            }
            return (T)_value;
        }

        public async Task<T> GetTask<T>()
        {
            if (_isSliding)
            {
                Volatile.Write(ref _expiredTicks, (DateTime.UtcNow + _lifetime).Ticks);
            }
            try
            {
                if (_value is ILazyTask)
                {
                    return await ((LazyTask<T>)_value).Value;
                }
                if (_value is ILazyValue)
                {
                    return ((LazyValue<T>)_value).Value;
                }
            }
            catch
            {
                MarkAsExpired();
                throw;
            }
            return (T)_value;
        }
    }
}
