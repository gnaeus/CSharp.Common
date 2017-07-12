using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Common.Logon
{
    /// <summary>
    /// Ring buffer, that contains ordered sequence of timestamps 
    /// for last N times, when somebody was failed to login from the same IP.
    /// </summary>
    public class LogonAttempts
    {
        private class RingQueue
        {
            public int Head;

            public readonly DateTime[] Buffer;

            public RingQueue(int size)
            {
                Head = size - 1;
                Buffer = new DateTime[size];
            }
        }

        private RingQueue _queue;
        private readonly object _locker = new object();

        public LogonAttempts(int maxCount)
        {
            _queue = new RingQueue(maxCount);
        }

        public DateTime LastTime()
        {
            lock (_locker)
            {
                return _queue.Buffer[_queue.Head];
            }
        }

        /// <remarks>
        /// If we want to store the time when request comes to controller instead
        /// of time when thread gets lock on <see cref="_locker"/> we should use
        /// some of "PriorityQueue" with fixed size (equals max attempts count)
        /// instead of <see cref="RingQueue"/>. Because <see cref="DateTime"/> 
        /// that comes from controller can be in different order with times when
        /// controller's threads gets lock on <see cref="_locker"/>.
        /// </remarks>
        public void PutNext()
        {
            lock (_locker)
            {
                DateTime[] buffer = _queue.Buffer;

                _queue.Head = (_queue.Head + 1) % buffer.Length;
                buffer[_queue.Head] = DateTime.Now;
            }
        }

        public bool IsRejected(DateTime watchingStart)
        {
            lock (_locker)
            {
                DateTime[] buffer = _queue.Buffer;
                int tail = (_queue.Head + 1) % buffer.Length;

                return buffer[tail] > watchingStart;
            }
        }

        public LogonAttempts Merge(LogonAttempts replaced)
        {
            lock (_locker)
                lock (replaced._locker)
                {
                    DateTime[] buffer = _queue.Buffer;
                    DateTime lastTime = buffer[_queue.Head];

                    var mergedTimes = replaced.GetAccessTimes()
                        .SkipWhile(t => t <= lastTime);

                    foreach (var newTime in mergedTimes)
                    {
                        buffer[_queue.Head] = newTime;
                        _queue.Head = (_queue.Head + 1) % buffer.Length;
                    }

                    Interlocked.Exchange(ref replaced._queue, _queue);
                }

            return this;
        }

        private IEnumerable<DateTime> GetAccessTimes()
        {
            DateTime[] buffer = _queue.Buffer;
            int length = buffer.Length;
            int head = _queue.Head;

            for (int i = 0; i != length; i++)
            {
                int index = (head + i) % length;
                yield return buffer[index];
            }
        }
    }
}