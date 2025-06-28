using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
#if GODOT_WINDOWS
using System;
using System.Runtime.InteropServices;
#endif

namespace LingoEngine.LGodot.Core
{

    public enum GodotLoggerTarget
    {
        Auto,
        Godot,
        VisualStudio
    }


    public class GodotLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly GodotLoggerTarget _target;

        public GodotLogger(string categoryName, GodotLoggerTarget target)
        {
            _categoryName = categoryName;
            _target = target == GodotLoggerTarget.Auto
                ? (IsRunningInGodot() ? GodotLoggerTarget.Godot : GodotLoggerTarget.VisualStudio)
                : target;
#if GODOT_WINDOWS && DEBUG
            if (_target == GodotLoggerTarget.VisualStudio)
                ConsoleHelper.ShowConsole(); // only attach console in Windows Godot context
#endif
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string message = formatter(state, exception);
            if (exception != null)
                message += $" Exception: {exception}";

            string logLine = $"[{_categoryName}] [{logLevel}] {message}";

            switch (_target)
            {
                case GodotLoggerTarget.Godot:
                    LogToGodot(logLevel, logLine);
                    break;
                case GodotLoggerTarget.VisualStudio:
#if GODOT_WINDOWS 
                    WriteColoredConsoleLine(logLevel, logLine);
#else
                    Debug.WriteLine(logLine);
#endif
                    break;
            }
        }

        private static void LogToGodot(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    GD.PushError(message);
                    break;
                case LogLevel.Warning:
                    GD.PushWarning(message);
                    break;
                default:
                    GD.Print(message);
                    break;
            }
        }

        private static bool IsRunningInGodot()
        {
            return Engine.IsEditorHint() || OS.HasFeature("Godot");
        }
        private static void WriteColoredConsoleLine(LogLevel level, string message)
        {
            var originalColor = Console.ForegroundColor;

            switch (level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;

            // Also send to VS debug window
            Debug.WriteLine(message);
        }

    }
    public class GodotLoggerProvider : ILoggerProvider
    {
        private readonly GodotLoggerTarget _target;

        public GodotLoggerProvider(GodotLoggerTarget target = GodotLoggerTarget.Auto)
        {
            _target = target;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new GodotLogger(categoryName, _target);
        }

        public void Dispose() { }
    }
    public static class IoCRegistration
    {
        public static IServiceCollection AddGodotLogging(this IServiceCollection services, GodotLoggerTarget target = GodotLoggerTarget.Auto)
        {
            services.AddSingleton<ILoggerProvider>(new GodotLoggerProvider(target));
            IServiceCollection serviceCollection = services.AddSingleton<ILoggerFactory>(sp =>
            {
                var factory = LoggerFactory.Create(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(sp.GetRequiredService<ILoggerProvider>());
                });
                return factory;
            });

            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            return services;
        }
    }

}
#if GODOT_WINDOWS 

public static class ConsoleHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();

    private static bool _consoleAllocated = false;

    public static void ShowConsole()
    {
        if (!_consoleAllocated)
        {
            AllocConsole();
#pragma warning disable CA1416 // Validate platform compatibility
                Console.BufferHeight = 5000;
#pragma warning restore CA1416 // Validate platform compatibility
            _consoleAllocated = true;
        }
    }

    public static void HideConsole()
    {
        if (_consoleAllocated)
        {
            FreeConsole();
            _consoleAllocated = false;
        }
    }
}

#endif