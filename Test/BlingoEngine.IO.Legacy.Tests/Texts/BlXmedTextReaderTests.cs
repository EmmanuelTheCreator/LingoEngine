using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data.Txc;
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
        var document = ReadDocument("Text_Hallo_13.xmed.bin");

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
        var italic = ReadDocument("Styles/Text_Hallo_italic_13.xmed.bin");
        italic.Styles.Should().Contain(s => s.Italic);

        var underline = ReadDocument("Styles/Text_Hallo_underline_13.xmed.bin");
        underline.Styles.Should().Contain(s => s.Underline);
    }
  
    [Fact]
    public void Read_ParsesStyleDescriptorsForMultifont()
    {
        var document = ReadDocument("Text_Multi_Line_Multi_Style_13.xmed.bin");

        document.Text.Should().Contain("This text is red");
        //document.RunMap.Should().NotBeEmpty();
        document.Styles.Should().HaveCountGreaterThan(1);
    }

    [Theory]
    [InlineData("MemberTests/Text_PreRender_CopyInk_SaveBitmap_13.xmed.bin", 'p', false)]
    [InlineData("MemberTests/Text_PreRender_OtherInk_SaveBitmap_13.xmed.bin", 'l', true)]
    public void Read_WithPreRenderedBitmap_LoadsTxcImage(string asset, char variant, bool expectDecodable)
    {
        var document = ReadDocument(asset);

        document.PreRenderedImage.Should().NotBeNull();

        var image = document.PreRenderedImage!;
        image.Variant.Should().Be(variant);
        image.Width.Should().Be((ushort)500);
        image.Height.Should().Be((ushort)154);

        if (expectDecodable)
        {
            image.Compression.Should().Be(BlLegacyTxcCompressionKind.RlePairs);
            image.EncodedPixels.Should().NotBeEmpty();
            image.Pixels.Should().HaveCount(image.Width * image.Height);
        }
        else
        {
            image.Compression.Should().Be(BlLegacyTxcCompressionKind.Unknown);
            image.EncodedPixels.Should().BeEmpty();
            image.RemainingData.Should().NotBeEmpty();
        }
    }

  
    private XmedDocument ReadDocument(string asset)
    {
        var path = TestContextHarness.GetTextAssetPath(asset);
        var bytes = File.ReadAllBytes(path);
        var reader = new BlXmedTextReader(_logger);
        return reader.Read(bytes);
    }

   
}
