using System.Collections.Generic;
using Serilog.Events;
using Serilog.Extensions.Logging;
 using Serilog.Sinks.ListOfString;

 namespace maxbl4.RaceLogic.Tests
{
    public class MemoryLogger
    {
        public static (Serilog.ILogger instance, List<string> messages) Serilog()
        {
            var messages = new List<string>();
            return (new Serilog.LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.StringList(messages, outputTemplate: "{Message}")
                .CreateLogger(), messages);
        }

        public static (Serilog.ILogger instance, List<string> messages) Serilog<T>()
        {
            var messages = new List<string>();
            return (new Serilog.LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.StringList(messages, outputTemplate: "[{SourceContext}] {Message}")
                .CreateLogger().ForContext<T>(), messages);
        }

        public static (Microsoft.Extensions.Logging.ILogger instance, List<string> messages) Logger<T>()
        {
            var (logger, messages) = Serilog();
            return (new SerilogLoggerProvider(logger).CreateLogger(typeof(T).FullName), messages);
        }
    }
}