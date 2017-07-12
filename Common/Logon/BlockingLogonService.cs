using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Common.Logon
{
    /// <summary>
    /// Blocking implementation of <see cref="ILogonService"/>
    /// </summary>
    /// <remarks>
    /// Adding new IP to dictionary requires exclusive lock.
    /// Only one thred can write to one of <see cref="LogonAttempts"/> block,
    /// but other threads can read from another blocks at this time.
    /// Access to the same <see cref="LogonAttempts"/> instance is blocking.
    /// </remarks>
    public class BlockingLogonService : ILogonService
    {
        private readonly int _maxAttempts;
        private readonly TimeSpan _watchingInterval;
        private readonly Timer _clearTimer;
        private readonly Dictionary<string, BlockingLogonAttempts> _logonDict;
        private readonly ReaderWriterLockSlim _rw = new ReaderWriterLockSlim();

        public BlockingLogonService(int maxAttempts, TimeSpan watchingInterval)
        {
            _maxAttempts = maxAttempts;
            _watchingInterval = watchingInterval;
            _logonDict = new Dictionary<string, BlockingLogonAttempts>();

            _clearTimer = new Timer(ClearOutdated, null, _watchingInterval, _watchingInterval);
        }

        public void HandleSuccess(string identifier)
        {
            _rw.EnterWriteLock();
            try
            {
                _logonDict.Remove(identifier);
            }
            finally { _rw.ExitWriteLock(); }
        }

        public void HandleFailure(string identifier)
        {
            BlockingLogonAttempts attempts;

            _rw.EnterUpgradeableReadLock();
            try
            {
                if (_logonDict.TryGetValue(identifier, out attempts))
                {
                    attempts.PutNext();
                }
                else
                {
                    _rw.EnterWriteLock();
                    try
                    {
                        attempts = new BlockingLogonAttempts(_maxAttempts);
                        _logonDict.Add(identifier, attempts);
                    }
                    finally { _rw.ExitWriteLock(); }
                }
            }
            finally { _rw.ExitUpgradeableReadLock(); }
        }

        public bool IsRejected(string identifier)
        {
            _rw.EnterReadLock();
            try
            {
                BlockingLogonAttempts attempts;
                return _logonDict.TryGetValue(identifier, out attempts) &&
                    attempts.IsRejected(DateTime.Now - _watchingInterval);
            }
            finally { _rw.ExitReadLock(); }
        }

        private void ClearOutdated(object o = null)
        {
            DateTime watchingStart = DateTime.Now - _watchingInterval;

            _rw.EnterUpgradeableReadLock();
            try
            {
                string[] outdatedKeys = _logonDict
                    .Where(p => p.Value.IsOutdated(watchingStart))
                    .Select(p => p.Key).ToArray();

                _rw.EnterWriteLock();
                try
                {
                    foreach (var key in outdatedKeys)
                    {
                        _logonDict.Remove(key);
                    }
                }
                finally { _rw.ExitWriteLock(); }
            }
            finally { _rw.ExitUpgradeableReadLock(); }
        }
    }
}