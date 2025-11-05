using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System.Collections.Concurrent;
using System.IO;

namespace AlgoTradeWithPythonWithScottPlot
{
    public class MemoryLogSink : ILogEventSink
    {
        private readonly ConcurrentQueue<string> _logMessages;
        private readonly ITextFormatter _formatter;

        public MemoryLogSink()
        {
            _logMessages = new ConcurrentQueue<string>();
            _formatter = new MessageTemplateTextFormatter(
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
        }

        public void Emit(LogEvent logEvent)
        {
            using var writer = new StringWriter();
            _formatter.Format(logEvent, writer);
            _logMessages.Enqueue(writer.ToString());
        }

        public List<string> GetAndClearLogs()
        {
            var logs = new List<string>();
            while (_logMessages.TryDequeue(out var message))
            {
                logs.Add(message);
            }
            return logs;
        }

        public bool HasLogs => !_logMessages.IsEmpty;
    }
}