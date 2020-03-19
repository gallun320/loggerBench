using Benchmark;
using BenchmarkDotNet.Running;
using System;

namespace Logger
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<LoggerBenchmark>();
            Console.WriteLine(summary);
            Console.ReadKey();
        }
    }
}
