namespace BlingoEngine.IO.Legacy.Tests
{
    using System;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;


    public class XunitLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ITestOutputHelper _output;
        private readonly bool _detailed;

        public XunitLogger(string categoryName, ITestOutputHelper output, bool detailed = false)
        {
            _categoryName = categoryName;
            _output = output;
            _detailed = detailed;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);
            if (_detailed)
                _output.WriteLine($"[{logLevel}] {_categoryName}: {message}");
            else
                _output.WriteLine($": {message}");
            if (exception != null)
            {
                _output.WriteLine(exception.ToString());
            }
        }
    }

    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        public XunitLoggerProvider(ITestOutputHelper output)
        {
            _output = output;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(categoryName, _output);
        }

        public void Dispose() { }
    }
}
