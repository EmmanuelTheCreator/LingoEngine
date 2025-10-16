namespace BlingoEngine.IO.Legacy.Tests
{
    using System;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;


    public class XunitLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ITestOutputHelper _output;
        private readonly XunitLoggerProvider _provider;

        internal XunitLogger(string categoryName, ITestOutputHelper output, XunitLoggerProvider provider)
        {
            _categoryName = categoryName;
            _output = output;
            _provider = provider;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _provider.MinimumLevel;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            if (_provider.Detailed)
                _output.WriteLine($"[{logLevel}] {_categoryName}: {message}");
            else
                _output.WriteLine($": {message}");
            if (exception != null)
            {
                _output.WriteLine(exception.ToString());
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }

    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;
        public bool Detailed { get; set; }

        public XunitLoggerProvider(ITestOutputHelper output)
        {
            _output = output;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(categoryName, _output, this);
        }

        public void Dispose() { }
    }
}
