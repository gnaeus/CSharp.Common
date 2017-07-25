using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    internal class ReaderWriterBaseEntry : IBaseEntry
    {
        private readonly ReaderWriterLockSlim _locker;

        private HashSet<ICacheEntry> _derivedEntries;

        protected ReaderWriterBaseEntry()
        {
            _locker = new ReaderWriterLockSlim();
        }

        public ReaderWriterBaseEntry(ICacheEntry cacheEntry)
            : this()
        {
            _derivedEntries = new HashSet<ICacheEntry> { cacheEntry };
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

        public void AddDerivedEntry(ICacheEntry cacheEntry)
        {
            try
            {
                _locker.EnterWriteLock();

                if (_derivedEntries == null)
                {
                    _derivedEntries = new HashSet<ICacheEntry>();
                }
                _derivedEntries.Add(cacheEntry);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void RemoveDerivedEntry(ICacheEntry cacheEntry)
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

        public void ForEachDerivedEntry(Action<ICacheEntry> action)
        {
            try
            {
                _locker.EnterReadLock();

                if (_derivedEntries == null)
                {
                    return;
                }
                foreach (ICacheEntry entry in _derivedEntries)
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
                    try
                    {
                        _locker.EnterWriteLock();

                        _derivedEntries.ExceptWith(entriesToRemove);
                    }
                    finally
                    {
                        _locker.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }
    }

    internal class ReaderWriterCacheEntry : ReaderWriterBaseEntry, ICacheEntry
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

        public ReaderWriterCacheEntry(
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

    public class ReaderWriterEntryFactory : IEntryFactory
    {
        public IBaseEntry CreateBase(ICacheEntry cacheEntry)
        {
            return new ReaderWriterBaseEntry(cacheEntry);
        }

        public ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, T value)
        {
            return new ReaderWriterCacheEntry(key, tags, isSliding, lifetime, value);
        }

        public ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory)
        {
            return new ReaderWriterCacheEntry(key, tags, isSliding, lifetime, new LazyValue<T>(valueFactory));
        }

        public ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory)
        {
            return new ReaderWriterCacheEntry(key, tags, isSliding, lifetime, new LazyTask<T>(taskFactory));
        }
    }
}
