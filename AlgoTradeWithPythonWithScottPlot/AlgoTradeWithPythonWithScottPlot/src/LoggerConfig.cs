using Serilog;
using System;
using System.IO;

namespace AlgoTradeWithPythonWithScottPlot
{
    public static class LoggerConfig
    {
        private static string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static string _logFileName = "application.log";
        private static MemoryLogSink _memoryLogSink = new MemoryLogSink();

        public static MemoryLogSink MemoryLogSink => _memoryLogSink;

        public static void SetLogDirectory(string directory)
        {
            _logDirectory = directory;
        }

        public static void SetLogFileName(string fileName)
        {
            _logFileName = fileName;
        }

        public static void Initialize()
        {
            Directory.CreateDirectory(_logDirectory);

            string logFilePath = Path.Combine(_logDirectory, _logFileName);
            
            // Log dosyasını sıfırla (eğer mevcutsa)
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    logFilePath,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .WriteTo.Sink(_memoryLogSink)
                .Enrich.WithThreadId()
                .CreateLogger();

            Log.Information("Logger initialized successfully");
            Log.Information($"Log file path: {logFilePath}");
        }

        public static void Shutdown()
        {
            Log.Information("Shutting down logger");
            Log.CloseAndFlush();
        }
    }
}