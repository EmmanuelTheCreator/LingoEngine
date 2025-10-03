using System;
using System.IO;
using System.Linq;

namespace BlingoEngine.Lingo.Core.Tests;

internal static class TetriGroundsTestData
{
    private static string _cachedOriginalScriptsDirectory = string.Empty;

    public static string GetOriginalScriptsDirectory()
    {
        if (!string.IsNullOrEmpty(_cachedOriginalScriptsDirectory))
            return _cachedOriginalScriptsDirectory;

        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "Demo", "TetriGrounds"));
        var candidates = new[]
        {
            "TetriGrounds.Lingo.Original",
            "TetriGrounds.Blingo.Original"
        };

        foreach (var name in candidates)
        {
            var candidate = Path.Combine(root, name);
            if (Directory.Exists(candidate) && Directory.EnumerateFiles(candidate, "*.ls", SearchOption.TopDirectoryOnly).Any())
            {
                _cachedOriginalScriptsDirectory = candidate;
                return candidate;
            }
        }

        throw new DirectoryNotFoundException($"Could not locate TetriGrounds original scripts under '{root}'.");
    }
}
