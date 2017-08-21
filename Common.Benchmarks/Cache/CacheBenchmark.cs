using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common.Cache;
using Common.Cache.Blocking;
//using Common.Cache.Concurrent;

namespace Common.Benchmarks.Cache
{
    public class CacheBenchmark
    {
        readonly IMemoryCache _cache = new MemoryCache();
        
        private string[] _keys;

        private string[] _tags;

        private string[][] _keyTags;

        private int _lifetimeSec;

        private int _invalidationMsec;
        
        private const int OperationStep = 1000;

        private long _totalCount = 0;

        private long _perSecondCount = 0;
        
        public void Run(string[] args)
        {
            int keys = Int32.Parse(args[1]);
            int tags = Int32.Parse(args[2]);
            int tagsPerKey = Int32.Parse(args[3]);
            int lifetimeSec = Int32.Parse(args[4]);
            int invalidationMsec = Int32.Parse(args[5]);

            if (keys < 1) throw new ArgumentOutOfRangeException(nameof(keys));
            if (tags < 0) throw new ArgumentOutOfRangeException(nameof(tags));
            if (tagsPerKey < 0 || tagsPerKey > tags) throw new ArgumentOutOfRangeException(nameof(tagsPerKey));
            if (lifetimeSec < 0) throw new ArgumentOutOfRangeException(nameof(lifetimeSec));
            if (invalidationMsec < 0) throw new ArgumentOutOfRangeException(nameof(invalidationMsec));

            int keysPerTag = tags != 0 ? (int)(((decimal)tagsPerKey) / 2 * keys / tags) : 0;

            _keys = Enumerable.Range(0, keys).Select(i => i.ToString()).ToArray();

            _tags = Enumerable.Range(0, tags).Select(i => i.ToString()).ToArray();

            var random = new Random();

            if (tags != 0)
            {
                _keyTags = Enumerable.Range(0, keys)
                    .Select(i => Enumerable.Range(0, random.Next(tagsPerKey + 1))
                        .Select(j => _tags[j])
                        .ToArray())
                    .ToArray();
            }

            _lifetimeSec = lifetimeSec;
            _invalidationMsec = invalidationMsec;

            double prob = tags != 0 ? Math.Pow(((double)(keys - keysPerTag)) / keys, lifetimeSec * 1000 / invalidationMsec) : 1;

            int threadCount = Environment.ProcessorCount;

            Console.WriteLine($"Keys: {keys}");
            Console.WriteLine($"Tags: {tags}");
            Console.WriteLine($"Tags/Key: 0 - {tagsPerKey}");
            Console.WriteLine($"Keys/Tag: {keysPerTag}");
            Console.WriteLine($"Threads: {threadCount}");
            Console.WriteLine($"Lifetime: {lifetimeSec}s");
            Console.WriteLine($"Invalidaton: 1 per {invalidationMsec}ms");
            Console.WriteLine($"Probability of timeout eviction: {prob * 100:0.0}%");
            Console.WriteLine();

            var cachingThreads = Enumerable.Range(0, threadCount)
                .Select(_ => new Thread(CachingThread))
                .ToList();

            var invalidationThread = new Thread(InvalidationThread);

            var sw = Stopwatch.StartNew();

            invalidationThread.Start();
            cachingThreads.ForEach(t => t.Start());

            int logCounter = 0;
            long totalMem = 0;

            while (true)
            {
                long lastElapsed = sw.ElapsedMilliseconds;

                Thread.Sleep(5000);

                long opsPerSecAvg = Volatile.Read(ref _totalCount) / sw.ElapsedMilliseconds;

                long opsPerSec = Interlocked.Exchange(ref _perSecondCount, 0) / (sw.ElapsedMilliseconds - lastElapsed);

                Process process = Process.GetCurrentProcess();

                // get the physical mem usage
                long mem = process.WorkingSet64;
                long peakMem = process.PeakWorkingSet64;

                totalMem += mem;
                long avgMem = totalMem / ++logCounter;

                Console.WriteLine($"Elapsed: {sw.Elapsed} | Memory peak: {peakMem >> 20} MB avg: {avgMem >> 20} MB cur: {mem >> 20} MB | Op/s avg: {opsPerSecAvg}K, cur: {opsPerSec}K");
            }
        }

        private void CachingThread()
        {
            var random = new Random();

            object value;

            if (_tags.Length == 0)
            {
                while (true)
                {
                    for (int i = 0; i < OperationStep; i++)
                    {
                        int index = random.Next(_keys.Length);

                        string key = _keys[index];
                        
                        value = _cache.GetOrAdd(
                            key, null, false, TimeSpan.FromSeconds(_lifetimeSec), () => new object());
                    }

                    Interlocked.Add(ref _totalCount, OperationStep);
                    Interlocked.Add(ref _perSecondCount, OperationStep);
                }
            }

            while (true)
            {
                for (int i = 0; i < OperationStep; i++)
                {
                    int index = random.Next(_keys.Length);

                    string key = _keys[index];
                    
                    string[] tags = _keyTags[index];

                    value = _cache.GetOrAdd(
                        key, tags, false, TimeSpan.FromSeconds(_lifetimeSec), () => new object());
                }

                Interlocked.Add(ref _totalCount, OperationStep);
                Interlocked.Add(ref _perSecondCount, OperationStep);
            }
        }

        private void InvalidationThread()
        {
            if (_tags.Length == 0)
            {
                return;
            }

            var random = new Random();

            while (true)
            {
                string tag = _tags[random.Next(_tags.Length)];

                _cache.ClearTag(tag);

                Thread.Sleep(_invalidationMsec);
            }
        }
    }
}
