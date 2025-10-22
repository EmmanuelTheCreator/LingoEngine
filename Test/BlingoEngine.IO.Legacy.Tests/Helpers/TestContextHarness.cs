using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Fields;
using BlingoEngine.IO.Legacy.Files;
using BlingoEngine.IO.Legacy.Scripts;
using BlingoEngine.IO.Legacy.Shapes;
using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;


namespace BlingoEngine.IO.Legacy.Tests.Helpers;

internal sealed class TestContextHarness : IDisposable
{
    public static IReadOnlyList<BlLegacyCastLibrary> LoadCastLibraries(string relativePath)
    {
        using var harness = Open(relativePath);
        harness.ReadResources();
        var libraries = harness.Context.ReadCastLibraries();
        return libraries;
    }

    public static IReadOnlyList<BlLegacySound> LoadSounds(string relativePath)
    {
        using var harness = Open(relativePath);
        harness.ReadResources();
        return harness.Context.ReadSounds();
    }

    public static IReadOnlyList<BlLegacyShape> LoadShapes(string relativePath)
    {
        using var harness = Open(relativePath);
        harness.ReadResources();
        return harness.Context.ReadShapes();
    }

    public static IReadOnlyList<BlLegacyBitmap> LoadBitmaps(string relativePath)
    {
        using var harness = Open(relativePath);
        harness.ReadResources();
        return harness.Context.ReadBitmaps();
    }

    public static IReadOnlyList<BlLegacyScript> LoadScripts(string relativePath)
    {
        using var harness = Open(relativePath);
        harness.ReadResources();
        return harness.Context.ReadScripts();
    }

    public static IReadOnlyList<BlLegacyText> LoadTexts(string relativePath)
    {
        using var harness = Open(relativePath);
        harness.ReadResources();
        var libraries = harness.Context.ReadCastLibraries();
        return harness.Context.ReadTexts();
    }

    public static IReadOnlyList<BlLegacyField> LoadFields(string relativePath)
    {
        using var harness = Open(relativePath);
        harness.ReadResources();
        return harness.Context.ReadFields();
    }

    private TestContextHarness(ReaderContext context)
    {
        Context = context;
    }

    public ReaderContext Context { get; }

    public static string GetAssetPath(string relativePath) => TestFolder.AssetPath(relativePath);

    public static string GetTextAssetPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Relative path must be provided.", nameof(relativePath));

        string normalized = relativePath.Replace('\\', '/').Trim('/');
        if (normalized.Length == 0)
            throw new ArgumentException("Relative path must contain a file name.", nameof(relativePath));

        string direct = GetAssetPath($"Texts_Fields/{normalized}");
        if (File.Exists(direct))
            return direct;

        string fileName = Path.GetFileName(normalized);
        string root = GetAssetPath("Texts_Fields");
        string[] matches = Directory.GetFiles(root, fileName, SearchOption.AllDirectories);
        if (matches.Length == 0)
            throw new FileNotFoundException($"XMED sample '{relativePath}' not found.", direct);

        return matches[0];
    }
    public static TestContextHarness Open(string relativePath)
    {
        var fullPath = TestFolder.AssetPath(relativePath.TrimStart('/').TrimStart('\\'));
        var stream = File.OpenRead(fullPath);
        var context = new ReaderContext(stream, Path.GetFileName(fullPath), leaveOpen: false);
        return new TestContextHarness(context);
    }
    public static string[] GetAllFilesFromFolder(string relativePath, string filter = "*.*")
    {
        var fullPath = TestFolder.AssetPath(relativePath);
        var fullPath2 = TestFolder.AssetPath("");
        return Directory.GetFiles(fullPath, filter, SearchOption.TopDirectoryOnly).Select(x => x.Replace(fullPath2,"")).ToArray();
    }

    public void ReadResources()
    {
        Context.ReadDirFilesContainer();
    }

    public static string XmedDumpLongLog(string fileName)
    {
        var item = GetTextAssetPath(fileName);
        var bytes = File.ReadAllBytes(item);
        var tokens = BlXmedTokenizer.Tokenize(bytes).Tokens;
        var log = BlXmedTokenizer.DumpTokensCompact(tokens);
        return log;
    }
    public static string XmedDumpCompactLog(string fileName)
    {
        var item = GetTextAssetPath(fileName);
        var bytes = File.ReadAllBytes(item);
        var tokens = BlXmedTokenizer.Tokenize(bytes).Tokens;
        var log = BlXmedTokenizer.DumpTokensUltraCompact(tokens);
        return log;
    }
    public static string XmedDumpCompactLogGrouped(string fileName)
    {
        var item = GetTextAssetPath(fileName);
        var bytes = File.ReadAllBytes(item);
        var tokens = BlXmedTokenizer.Tokenize(bytes).Tokens;
        var groups = BlXmedTokenizer.CreateGroups(tokens);
        var log = XmedTokenGrouper.DumpGroupedTokens(groups);
        return log;
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
