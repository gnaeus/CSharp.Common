using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Common.Logon
{
    public class LogonService : ILogonService
    {
        private readonly int _maxAttempts;
        private readonly TimeSpan _watchingInterval;
        private readonly Timer _clearTimer;
        private readonly ConcurrentDictionary<string, LogonAttempts> _logonDict;

        public LogonService(int maxAttempts, TimeSpan watchingInterval)
        {
            _maxAttempts = maxAttempts;
            _watchingInterval = watchingInterval;
            _logonDict = new ConcurrentDictionary<string, LogonAttempts>();

            _clearTimer = new Timer(ClearOutdated, null, _watchingInterval, _watchingInterval);
        }

        public void HandleSuccess(string identifier)
        {
            LogonAttempts attempts;
            _logonDict.TryRemove(identifier, out attempts);
        }

        public void HandleFailure(string identifier)
        {
            LogonAttempts attempts = _logonDict.GetOrAdd(
                identifier, key => new LogonAttempts(_maxAttempts));

            attempts.PutNext();
        }

        public bool IsRejected(string identifier)
        {
            LogonAttempts attempts;
            return _logonDict.TryGetValue(identifier, out attempts) &&
                attempts.IsRejected(DateTime.Now - _watchingInterval);
        }

        private void ClearOutdated(object o = null)
        {
            DateTime watchingStart = DateTime.Now - _watchingInterval;

            foreach (var pair in _logonDict)
            {
                DateTime lastTime = pair.Value.LastTime();

                if (lastTime >= watchingStart) continue;

                LogonAttempts removed;
                if (!_logonDict.TryRemove(pair.Key, out removed)) continue;

                if (removed.LastTime() != lastTime)
                {
                    _logonDict.AddOrUpdate(pair.Key, removed,
                        (key, replaced) => removed.Merge(replaced));
                }
            }
        }
    }
}