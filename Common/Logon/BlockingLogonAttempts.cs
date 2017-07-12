using System;

namespace Common.Logon
{
    /// <summary>
    /// Simplified version of <see cref="LogonAttempts"/>
    /// </summary>
    /// <remarks> Used in <see cref="BlockingLogonService"/> </remarks>
    public class BlockingLogonAttempts
    {
        private int _head;
        private readonly DateTime[] _buffer;
        private readonly object _locker = new object();

        public BlockingLogonAttempts(int maxCount)
        {
            _buffer = new DateTime[maxCount];
            _buffer[_head] = DateTime.Now;
        }

        public void PutNext()
        {
            lock (_locker)
            {
                _head = (_head + 1) % _buffer.Length;
                _buffer[_head] = DateTime.Now;
            }
        }

        public bool IsRejected(DateTime watchingStart)
        {
            lock (_locker)
            {
                int tail = (_head + 1) % _buffer.Length;
                return _buffer[tail] > watchingStart;
            }
        }

        public bool IsOutdated(DateTime watchingStart)
        {
            lock (_locker)
            {
                return _buffer[_head] < watchingStart;
            }
        }
    }
}