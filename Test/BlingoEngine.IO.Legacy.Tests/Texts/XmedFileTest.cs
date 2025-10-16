using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

// Each test targets a specific XMED sample under Texts_Fields.
// The assertions reconstruct the member text exclusively from the parsed runs.

public class XmedFileTest
{
    private readonly ILogger<XmedFileTest> _logger;

    public XmedFileTest(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
        });

        _logger = factory.CreateLogger<XmedFileTest>();
    }

    [Fact]
    public void Text_Hallo_tab_true_file_should_report_tabs()
    {
        var document = ReadDocument("Text_Hallo_tab_true_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
        document.Runs.Should().HaveCount(1);
        document.Styles.Select(style => style.FontName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Should().Contain(name => name.Equals("Vivaldi", StringComparison.OrdinalIgnoreCase));
    }
    [Fact]
    public void Text_Hallo()
    {
        var document = ReadDocument("Text_Hallo_13.xmed.bin");

        //string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        //textFromRuns.ShouldMatchNormalized("Hallo");
        //document.Runs.Should().HaveCount(1);
        //document.Styles.Select(style => style.FontName)
        //    .Where(name => !string.IsNullOrEmpty(name))
        //    .Should().Contain(name => name.Equals("Arcade *", StringComparison.OrdinalIgnoreCase));
    }
   
    [Fact]
    public void Text_3_Paragraps_13()
    {
        var document = ReadDocument("Text_3_Paragraps_13.xmed.bin");

        document.Text.Should().Be(string.Concat(document.Runs.Select(run => run.Text)));

        document.Paragraphs.Should().HaveCount(3);

        document.Paragraphs[0].Text.Should().Be("My first paragraph centered with all 0");
        document.Paragraphs[1].Text.Should().Be("Paragraph with align Left, Margin Left 4, Margin Right 5, First Indent 0.4inch Spacing Before 9, spacing after 7");
        document.Paragraphs[2].Text.Should().Be("Paragraph with align Left, Margin Left 1, Margin Right 2, First Indent 0.3inch Spacing Before 4, spacing after 5");

        document.Paragraphs[0].LeftMargin.Should().Be(0);
        document.Paragraphs[0].RightMargin.Should().Be(0);
        document.Paragraphs[0].FirstLineIndent.Should().Be(0);
        document.Paragraphs[0].SpacingBefore.Should().Be(0);
        document.Paragraphs[0].SpacingAfter.Should().Be(0);

        document.Paragraphs[1].LeftMargin.Should().Be(288);
        document.Paragraphs[1].RightMargin.Should().Be(360);
        document.Paragraphs[1].FirstLineIndent.Should().Be(28);
        document.Paragraphs[1].SpacingBefore.Should().Be(9);
        document.Paragraphs[1].SpacingAfter.Should().Be(7);

        document.Paragraphs[2].LeftMargin.Should().Be(72);
        document.Paragraphs[2].RightMargin.Should().Be(144);
        document.Paragraphs[2].FirstLineIndent.Should().Be(21);
        document.Paragraphs[2].SpacingBefore.Should().Be(4);
        document.Paragraphs[2].SpacingAfter.Should().Be(5);
    }

    [Fact]
    public void Text_Hallo_multifont_file_should_list_multiple_fonts()
    {
        var document = ReadDocument("Text_Hallo_multifont_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
        document.Runs.Should().HaveCount(1);
        document.Styles.Select(style => style.FontName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Should().Contain(name => name.Equals("Trajan Pro", StringComparison.OrdinalIgnoreCase));
        document.Styles.Select(style => style.FontName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Should().Contain(name => name.Equals("Arcade *", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Text_Hallo_multiLine_file_should_preserve_carriage_returns()
    {
        var document = ReadDocument("Text_Hallo_multiLine_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo\rmulti line\ris longer\rYES!");
        document.Text.Should().Contain("\r");
        document.Text.Split('\r', StringSplitOptions.RemoveEmptyEntries).Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Text_Single_Line_Multi_Style_file_should_read_long_text_and_runs()
    {
        var document = ReadDocument("Text_Single_Line_Multi_Style_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("This text is red, Arial,12px,  The text is yellow, Tahoma, 9px, , bold, italic, underline The text is green, font Terminal, 18px, with spacing of 39 The text is orange, Tahoma, 9px, bold, italic, underline This text is red, Arial,12px, again");
        GetNormalizedFonts(document).Should().Contain(new[] { "arial", "tahoma", "terminal" });
    }
    [Fact]
    public void Text_Multi_Line_Multi_Style_file_should_read_long_text_and_runs()
    {
        var document = ReadDocument("Text_Multi_Line_Multi_Style_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        //textFromRuns.ShouldMatchNormalized("This text is red, Arial,12px,  The text is yellow, Tahoma, 9px, , bold, italic, underline The text is green, font Terminal, 18px, with spacing of 39 The text is orange, Tahoma, 9px, bold, italic, underline This text is red, Arial,12px, again");
        GetNormalizedFonts(document).Should().Contain(new[] { "arial", "tahoma", "terminal" });
        document.Text.Should().Contain("\r");
    }


    [Theory]
    [InlineData("Text_Hallo_col_blue_13.xmed.bin", 0x00, 0x00, 0xFF)]
    [InlineData("Text_Hallo_col_blue1_13.xmed.bin", 0x00, 0x00, 0xFF)]
    [InlineData("Text_Hallo_col_bordeau_13.xmed.bin", 0x88, 0x00, 0x00)]
    [InlineData("Text_Hallo_col_green_13.xmed.bin", 0xFF, 0x00, 0x00)]
    [InlineData("Text_Hallo_col_lightgreen_13.xmed.bin", 0xCC, 0xFF, 0x99)]
    [InlineData("Text_Hallo_col_orange_13.xmed.bin", 0xFF, 0xCC, 0x66)]
    [InlineData("Text_Hallo_col_pink_13.xmed.bin", 0xFF, 0x00, 0xFF)]
    [InlineData("Text_Hallo_col_yellow_13.xmed.bin", 0xFF, 0xFF, 0x00)]
    public void Text_color_samples_should_BeRead(string fileName, byte expectedR, byte expectedG, byte expectedB)
    {
        var doc = ReadDocument($"{fileName}");

        // TODO
    }
    [Fact]
    public void Multi_Text_color_samples_should_BeRead()
    {
        var doc = ReadDocument($"MemberTests/Text_Multi_Style_Size_Color_13.bin");

        // TODO
    }

   


    private XmedDocument ReadDocument(string fileName)
    {
        var path = TestContextHarness.GetAssetPath($"Texts_Fields/{fileName}");
        var bytes = File.ReadAllBytes(path);
        var reader = new BlXmedTextReader(_logger);
        return reader.Read(bytes);
    }

    private static IReadOnlyCollection<string> GetNormalizedFonts(XmedDocument document)
    {
        return document.Styles
            .Select(style => style.FontName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name.ToLowerInvariant())
            .ToArray();
    }

   

    
}
