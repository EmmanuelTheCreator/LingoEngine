using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class BlXmedTextReaderTests
{
    private readonly ILogger<XmedFileTest> _logger;

    public BlXmedTextReaderTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
        });

        _logger = factory.CreateLogger<XmedFileTest>();
    }
    [Fact]
    public void Read_SingleRunText_ParsesHeaderAndText()
    {
        var document = ReadDocument("Texts_Fields/Text_Hallo_13.xmed.bin");

        document.Text.Should().Be("Hallo");
        document.Runs.Should().ContainSingle();

        var run = document.Runs[0];
        run.Start.Should().Be(0);
        run.Length.Should().Be(5);
        run.Text.Should().Be("Hallo");
    }

    [Fact]
    public void Read_DecodesStyleFlags()
    {
        var italic = ReadDocument("Texts_Fields/Text_Hallo_italic_13.xmed.bin");
        italic.Styles.Should().Contain(s => s.Italic);

        var underline = ReadDocument("Texts_Fields/Text_Hallo_underline_13.xmed.bin");
        underline.Styles.Should().Contain(s => s.Underline);
    }
  
    [Fact]
    public void Read_ParsesStyleDescriptorsForMultifont()
    {
        var document = ReadDocument("Texts_Fields/Text_Multi_Line_Multi_Style_13.xmed.bin");

        document.Text.Should().Contain("This text is red");
        //document.RunMap.Should().NotBeEmpty();
        document.Styles.Should().HaveCountGreaterThan(1);
    }

  
    private XmedDocument ReadDocument(string asset)
    {
        var path = TestContextHarness.GetAssetPath(asset);
        var bytes = File.ReadAllBytes(path);
        var reader = new BlXmedTextReader(_logger);
        return reader.Read(bytes);
    }

   
}
