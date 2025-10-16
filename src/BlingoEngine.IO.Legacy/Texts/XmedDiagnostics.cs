using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{
    [Flags]
    internal enum XmedDiagnosticArea
    {
        None = 0,
        TokenParser = 1 << 0,
        TokenReader = 1 << 1,
        StyleParser = 1 << 2,
        RunSliceBuilder = 1 << 3,
        All = TokenParser | TokenReader | StyleParser | RunSliceBuilder
    }

    internal static class XmedDiagnostics
    {
        private static XmedDiagnosticArea _enabledAreas = XmedDiagnosticArea.All;
        private static readonly object _sync = new();

        public static XmedDiagnosticArea EnabledAreas
        {
            get
            {
                lock (_sync)
                {
                    return _enabledAreas;
                }
            }
            set
            {
                lock (_sync)
                {
                    _enabledAreas = value;
                }
            }
        }

        public static void EnableAll()
        {
            EnabledAreas = XmedDiagnosticArea.All;
        }

        public static void DisableAll()
        {
            EnabledAreas = XmedDiagnosticArea.None;
        }

        public static void Enable(XmedDiagnosticArea areas)
        {
            SetEnabled(areas, true);
        }

        public static void Disable(XmedDiagnosticArea areas)
        {
            SetEnabled(areas, false);
        }

        public static void SetEnabled(XmedDiagnosticArea areas, bool enabled)
        {
            lock (_sync)
            {
                if (enabled)
                    _enabledAreas |= areas;
                else
                    _enabledAreas &= ~areas;
            }
        }

        public static bool IsEnabled(XmedDiagnosticArea area)
        {
            lock (_sync)
            {
                return (_enabledAreas & area) != 0;
            }
        }

        public static bool IsTraceEnabled(XmedDiagnosticArea area, ILogger? logger)
        {
            if (logger == null)
                return false;

            if (!IsEnabled(area))
                return false;

            return logger.IsEnabled(LogLevel.Trace);
        }

        public static void LogTrace(XmedDiagnosticArea area, ILogger? logger, string message, params object?[] args)
        {
            if (!IsTraceEnabled(area, logger))
                return;

            logger!.LogTrace(message, args);
        }
    }
}
