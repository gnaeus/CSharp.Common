using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache.ReaderWriterLock
{
    internal class BaseEntry
    {
        private readonly ReaderWriterLockSlim _locker;

        private HashSet<CacheEntry> _derivedEntries;
        
        protected BaseEntry()
        {
            _locker = new ReaderWriterLockSlim();
        }

        public BaseEntry(CacheEntry cacheEntry)
            : this()
        {
            _derivedEntries = new HashSet<CacheEntry> { cacheEntry };
        }

        public bool HasDerivedEntries
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();

                    return _derivedEntries != null && _derivedEntries.Count > 0;
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
        }

        public void AddDerivedEntry(CacheEntry cacheEntry)
        {
            try
            {
                _locker.EnterWriteLock();

                if (_derivedEntries == null)
                {
                    _derivedEntries = new HashSet<CacheEntry>();
                }
                _derivedEntries.Add(cacheEntry);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void RemoveDerivedEntry(CacheEntry cacheEntry)
        {
            try
            {
                _locker.EnterWriteLock();

                if (_derivedEntries != null)
                {
                    _derivedEntries.Remove(cacheEntry);
                }
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void ForEachDerivedEntry(Action<CacheEntry> action)
        {
            try
            {
                _locker.EnterReadLock();

                if (_derivedEntries == null)
                {
                    return;
                }
                foreach (CacheEntry entry in _derivedEntries)
                {
                    action.Invoke(entry);
                }
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        public void ScanForRemovedDerivedEntries()
        {
            try
            {
                _locker.EnterReadLock();

                if (_derivedEntries == null)
                {
                    return;
                }
                foreach (CacheEntry entry in _derivedEntries)
                {
                    if (entry.IsRemovedFromStorage)
                    {
                        RemoveDerivedEntry(entry);
                    }
                }
            }
            finally
            {
                _locker.ExitReadLock();
            }
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
            object key, object[] tags, bool isSliding, TimeSpan lifetime, object value)
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
