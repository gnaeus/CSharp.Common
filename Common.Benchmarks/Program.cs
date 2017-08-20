using System;
using System.Linq;
using BenchmarkDotNet.Running;
using Common.Benchmarks.Cache;

namespace Common.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                switch (args[0])
                {
                    case nameof(CacheBenchmark):
                        new CacheBenchmark().Run(args);
                        return;

                    case "help":
                    case "Help":
                    default:
                        Console.WriteLine($"Common.Benchmarks.exe {nameof(CacheBenchmark)} [keys:int] [tags:int] [tagsPerKey:int] [lifetimeSec:sec] [invalidationMsec:msec]");
                        return;
                }
            }

            // new BenchmarkSwitcher(new[] {
            //     typeof(SomeBenchmark),
            // }).Run(args);
        }
    }
}
