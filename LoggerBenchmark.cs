using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using NLog;
using NLog.Config;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Benchmark
{
    [SimpleJob(launchCount: 3, warmupCount: 3, targetCount: 10, invocationCount: 5, id: "QuickJob")]
    public class LoggerBenchmark
    {
        private int _iterations = 50_000;

        private const string SerilogTemplate = "[{Timestamp:HH:mm:ss.ffffff} {Level: u3}] {Message: lj}{NewLine}";

        #region Serilog Current

        private static Serilog.ILogger _serilogAsyncLogger;

        private static Serilog.ILogger GetAsyncSerilogLogger()
        {
            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Verbose))
                .WriteTo.Async(a => a.File("./serilog_1.log", fileSizeLimitBytes: 1134217728, outputTemplate: SerilogTemplate, buffered: true))
                .CreateLogger();
        }

        #endregion

        #region NLog

        private static NLog.ILogger Nlog {
            get { 
                if(_nlog == null)
                {
                    _nlog = LogManager.GetCurrentClassLogger();
                }
                return _nlog;
            } 
        }
        private static NLog.ILogger _nlog;
        private static void NLogSetup()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
        }

        #endregion

        #region Serilog without async
        
        private static Serilog.ILogger _serilogLogger;

        private static Serilog.ILogger GetSerilogLogger()
        {
            return new LoggerConfiguration()
                .WriteTo.File("./serilog_2.log", fileSizeLimitBytes: 1134217728, outputTemplate: SerilogTemplate, buffered: true, flushToDiskInterval: new TimeSpan(0, 0, 0))
                .CreateLogger();
        }

        #endregion

        #region Setup

        [GlobalCleanup]
        public void Clean()
        {
            (_serilogLogger as IDisposable)?.Dispose();
            (_serilogAsyncLogger as IDisposable)?.Dispose();

            DeleteLogs();
        }

        [GlobalSetup]
        public void Setup()
        {
            _serilogAsyncLogger = GetAsyncSerilogLogger();
            _serilogLogger = GetSerilogLogger();

            // create nlog
            NLogSetup();
        }

        private static void DeleteLogs()
        {
            var files = Directory.GetFiles("./");
            foreach (var file in files)
            {
                if (file.EndsWith(".log")) File.Delete(file);
            }
        }

        #endregion

        [Benchmark(Description = "Serilog", Baseline = true)]
        public void Serilog()
        {
            for (int i = 0; i < _iterations; i++)
            {
                var str = i.ToString();
                _serilogLogger.Debug($"This is a message with two params! {i}, {str}");
            }
        }

        [Benchmark(Description = "NLog")]
        public void NLog()
        {
            for (int i = 0; i < _iterations; i++)
            {
                 var str = i.ToString();
                Nlog.Debug($"This is a message with two params! {i}, {str}");
            }
        }
        
        [Benchmark(Description = "Serilog async")]
        public void SerilogAsync()
        {
            for (int i = 0; i < _iterations; i++)
            {
                var str = i.ToString();
                _serilogAsyncLogger.Debug($"This is a message with two params! {i}, {str}");
            }
        }
    }
}
