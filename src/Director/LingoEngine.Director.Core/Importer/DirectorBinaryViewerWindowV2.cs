using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Windowing;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Scores;
using ProjectorRays.Director;
using System;
using System.IO;

namespace LingoEngine.Director.Core.Importer;

/// <summary>
/// Experimental binary viewer window that parses test data and exposes stream annotations.
/// </summary>
public class DirectorBinaryViewerWindowV2 : DirectorWindow<IDirFrameworkBinaryViewerWindowV2>
{
    private readonly ILogger<DirectorBinaryViewerWindowV2> _logger;

    /// <summary>Annotations gathered from the parsed score chunk.</summary>
    public RayStreamAnnotatorDecorator? InfoToShow { get; private set; }

    /// <summary>The raw bytes read from the test file.</summary>
    public byte[]? Data { get; private set; }

    public DirectorBinaryViewerWindowV2(ILogger<DirectorBinaryViewerWindowV2> logger)
    {
        _logger = logger;
        //LoadTestData();
    }

    private void LoadTestData()
    {
        try
        {
            string path = Path.Combine(
                "..", "..", "Libraries", "LingoEngine", "WillMoveToOwnRepo", "ProjectorRays","Test", "TestData", 
                "KeyFramesTest.dir"
                );
            Data = File.ReadAllBytes(path);
            var stream = new ReadStream(Data, Data.Length, Endianness.BigEndian);
            var dir = new RaysDirectorFile(_logger);
            if (dir.Read(stream))
                InfoToShow = RaysScoreChunk.Annotator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load test data for BinaryViewerWindowV2");
        }
    }
}
