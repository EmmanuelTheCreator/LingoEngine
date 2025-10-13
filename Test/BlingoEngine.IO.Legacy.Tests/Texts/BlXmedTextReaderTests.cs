using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
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

    [Fact]
    public void Read_LegacyVersion_ParsesRichTextHeader()
    {
        var buffer = BuildLegacyRichTextBuffer();
        var reader = new BlXmedTextReader(_logger);

        var document = reader.Read(buffer, directorVersion: 5);

        document.DirectorVersion.Should().Be(5);
        document.Text.Should().BeEmpty();
        document.Runs.Should().BeEmpty();
        document.Styles.Should().BeEmpty();

        document.RichText.Should().NotBeNull();
        var metadata = document.RichText!;
        metadata.AntialiasFlag.Should().Be(0x9A);
        metadata.CropFlags.Should().Be(0x55);
        metadata.ScrollPosition.Should().Be(0x1234);
        metadata.AntialiasFontSize.Should().Be(0x0015);
        metadata.DisplayHeight.Should().Be(0x0456);
        metadata.ForegroundColor.Should().BeEquivalentTo(new BlLegacyColor(0x10, 0x20, 0x30));
        metadata.BackgroundColor.Should().BeEquivalentTo(new BlLegacyColor(0x40, 0x50, 0x60));

        metadata.InitialRect.Top.Should().Be(1);
        metadata.InitialRect.Left.Should().Be(2);
        metadata.InitialRect.Bottom.Should().Be(3);
        metadata.InitialRect.Right.Should().Be(4);

        metadata.BoundingRect.Top.Should().Be(5);
        metadata.BoundingRect.Left.Should().Be(6);
        metadata.BoundingRect.Bottom.Should().Be(7);
        metadata.BoundingRect.Right.Should().Be(8);
    }

    private XmedDocument ReadDocument(string asset)
    {
        var path = TestContextHarness.GetAssetPath(asset);
        var bytes = File.ReadAllBytes(path);
        var reader = new BlXmedTextReader(_logger);
        return reader.Read(bytes);
    }

    private static byte[] BuildLegacyRichTextBuffer()
    {
        var bytes = new List<byte>();

        static void AddInt16(List<byte> target, short value)
        {
            target.Add((byte)((value >> 8) & 0xFF));
            target.Add((byte)(value & 0xFF));
        }

        static void AddUInt16(List<byte> target, ushort value)
        {
            target.Add((byte)((value >> 8) & 0xFF));
            target.Add((byte)(value & 0xFF));
        }

        AddInt16(bytes, 1);
        AddInt16(bytes, 2);
        AddInt16(bytes, 3);
        AddInt16(bytes, 4);

        AddInt16(bytes, 5);
        AddInt16(bytes, 6);
        AddInt16(bytes, 7);
        AddInt16(bytes, 8);

        bytes.Add(0x9A);
        bytes.Add(0x55);

        AddUInt16(bytes, 0x1234);
        AddUInt16(bytes, 0x0015);
        AddUInt16(bytes, 0x0456);

        bytes.Add(0x00);
        bytes.Add(0x10);
        bytes.Add(0x20);
        bytes.Add(0x30);

        AddUInt16(bytes, 0x4000);
        AddUInt16(bytes, 0x5000);
        AddUInt16(bytes, 0x6000);

        return bytes.ToArray();
    }
}
