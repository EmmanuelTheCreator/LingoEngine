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

        var paragraphTexts = document.Paragraphs
            .Select(paragraph => GetParagraphText(document, paragraph))
            .ToArray();

        paragraphTexts[0].Should().Be("My first paragraph centered with all 0");
        paragraphTexts[1].Should().Be("Paragraph with align Left, Margin Left 4, Margin Right 5, First Indent 0.4inch Spacing Before 9, spacing after 7");
        paragraphTexts[2].Should().Be("Paragraph with align Left, Margin Left 1, Margin Right 2, First Indent 0.3inch Spacing Before 4, spacing after 5");

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

    [Fact]
    public void Text_Hallo_text_transform_all_on_file_should_merge_styles_into_run_text()
    {
        var document = ReadDocument("Text_Hallo_text_transform_all_on_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
        document.Styles.Should().Contain(style => style.Italic && style.Underline);
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

    private static string GetParagraphText(XmedDocument document, XmedParagraphDescriptor paragraph)
    {
        if (string.IsNullOrEmpty(document.Text) || paragraph.Length <= 0 || paragraph.Start < 0)
        {
            return string.Empty;
        }

        if (paragraph.Start >= document.Text.Length)
        {
            return string.Empty;
        }

        int length = Math.Min(paragraph.Length, document.Text.Length - paragraph.Start);
        return length > 0 ? document.Text.Substring(paragraph.Start, length) : string.Empty;
    }
}
