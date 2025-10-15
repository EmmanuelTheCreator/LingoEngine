using System;
using System.Collections.Generic;
using System.IO;
using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Files;

using BlingoEngine.IO.Legacy.Scripts;
using BlingoEngine.IO.Legacy.Shapes;
using BlingoEngine.IO.Legacy.Sounds;

using BlingoEngine.IO.Legacy.Fields;
using BlingoEngine.IO.Legacy.Shapes;
using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Texts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public void Dispose()
    {
        Context.Dispose();
    }
}
