using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    private readonly ITestOutputHelper _output;

    public XmedFileTest(ITestOutputHelper output)
    {
        _output = output;
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
        document.Runs.Should().NotBeEmpty();
        document.Styles.Select(style => style.FontName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Should().Contain(name => name.Equals("Vivaldi", StringComparison.OrdinalIgnoreCase));
    }
    [Fact]
    public void Text_Hallo()
    {
        var document = ReadDocument("Text_Hallo_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
        document.Runs.Should().HaveCount(1);
        document.Styles.Select(style => style.FontName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Should().Contain(name => name.Equals("Arcade *", StringComparison.OrdinalIgnoreCase));

        document.Paragraphs.Should().ContainSingle();
        var paragraph = document.Paragraphs.Single();
        paragraph.FormatRecord.Should().NotBeNull();
        var format = paragraph.FormatRecord!;
        format.EndOffset.Should().Be(paragraph.End);
        format.LeadingMargin.Should().Be(0);
        format.Span.Should().Be(0);
        format.Flags.Should().Be(0);
        format.FirstLineIndent.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Text_3_Paragraps_13()
    {
        var document = ReadDocument("Paragraphs/Text_3_Paragraps_13.xmed.bin");

        document.Text.Should().Be(string.Concat(document.Runs.Select(run => run.Text)));

        document.Paragraphs.Should().HaveCount(3);

        document.Paragraphs[0].Text.TrimEnd().Should().Be("My first paragraph centered with all 0");
        document.Paragraphs[1].Text.TrimEnd().Should().Be("Paragraph with align Left, Margin Left 4, Margin Right 5, First Indent 0.4inch Spacing Before 9, spacing after 7");
        document.Paragraphs[2].Text.TrimEnd().Should().Be("Paragraph with align Left, Margin Left 1, Margin Right 2, First Indent 0.3inch Spacing Before 4, spacing after 5");

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

        int searchIndex = 0;
        foreach (var paragraph in document.Paragraphs)
        {
            int foundIndex = document.Text.IndexOf(paragraph.Text, searchIndex, StringComparison.Ordinal);
            foundIndex.Should().Be(paragraph.Start);
            searchIndex = paragraph.Start + paragraph.Text.Length;
        }

        document.Paragraphs[1].FormatRecord.Should().NotBeNull();
        var second = document.Paragraphs[1].FormatRecord!;
        second.EndOffset.Should().Be(document.Paragraphs[1].End);
        second.LeadingMargin.Should().BeGreaterThan(0);
        second.Span.Should().Be(0);
        second.Flags.Should().Be(8);

        document.Paragraphs[2].FormatRecord.Should().NotBeNull();
        var third = document.Paragraphs[2].FormatRecord!;
        third.EndOffset.Should().Be(document.Paragraphs[2].End);
        third.Span.Should().Be(894);
        third.Flags.Should().Be(10);
        third.TrailingValue.Should().Be(377);
        third.FirstLineIndent.Should().Be(28);
    }

    [Fact]
    public void Text_Hallo_textAlignRight_formats_track_span()
    {
        var document = ReadDocument("Paragraphs/Text_Hallo_textAlignRight_13.xmed.bin");

        document.Paragraphs.Should().ContainSingle();
        var paragraph = document.Paragraphs.Single();
        paragraph.FormatRecord.Should().NotBeNull();
        var format = paragraph.FormatRecord!;
        format.EndOffset.Should().Be(paragraph.End);
        format.LeadingMargin.Should().Be(14);
        format.Span.Should().Be(0x77);
        format.Flags.Should().Be(0);
        format.TrailingValue.Should().Be(0x3B);
        format.FirstLineIndent.Should().Be(45);
    }

    [Theory]
    [InlineData("Text_Hallo_fontsize14_13.xmed.bin", 14, "Arial")]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_size50_13.xmed.bin", 50, "Tahoma")]
    public void Text_Hallo_fontsize14_file_should_track_font_size(string fileName, int expectedSize, string expectedFont)
    {
        var document = ReadDocument(fileName);

        document.Styles.Select(style => style.FontName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Should().Contain(name => name.Equals(expectedFont, StringComparison.OrdinalIgnoreCase));
        document.Runs.Select(run => run.FontSize).Should().Contain(expectedSize);
    }

    [Fact]
    public void Text_Hallo_text_transform_all_on_file_should_enable_styles()
    {
        var document = ReadDocument("Text_Hallo_text_transform_all_on_13.xmed.bin");

        document.Runs.Should().ContainSingle();
        var run = document.Runs.Single();
        run.Bold.Should().BeTrue();
        run.Italic.Should().BeTrue();
        run.Underline.Should().BeTrue();
        run.Strikeout.Should().BeFalse();
        run.Subscript.Should().BeFalse();
        run.Superscript.Should().BeFalse();

        document.Styles.Should().Contain(style =>
            style.Flags.HasFlag(XmedStyleDescriptor.XmedStyleFlags.Bold)
            && style.Flags.HasFlag(XmedStyleDescriptor.XmedStyleFlags.Italic)
            && style.Flags.HasFlag(XmedStyleDescriptor.XmedStyleFlags.Underline));
    }

    [Fact]
    public void Text_Hallo_multifont_file_should_list_multiple_fonts()
    {
        var document = ReadDocument("Text_Hallo_multifont_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
        document.Runs.Should().HaveCountGreaterThan(1);
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
    public void Font_table_should_decode_font_metadata()
    {
        var document = ReadDocument("Text_Multi_Line_Multi_Style_13.xmed.bin");

        document.Fonts.Should().HaveCountGreaterThan(1);
        document.Fonts.Select(f => f.FamilyName)
            .Should().Contain(new[] { "Arial", "Arcade *", "arial", "Tahoma", "Terminal" });

        var terminal = document.Fonts.First(f => f.FamilyName.Equals("Terminal", StringComparison.OrdinalIgnoreCase));
        terminal.FontId.Should().Be(0x60FF);
        terminal.CodePage.Should().Be(0x4E4);
        terminal.Weight.Should().Be(0x400);
        terminal.CellHeight.Should().Be(0xFF);
        terminal.PitchAndFamily.Should().Be(0x40008);
        terminal.PitchAndFamilyByte.Should().Be(0x08);
        terminal.PitchFlags.Should().Be(XmedFontPitchFlags.Default);
        terminal.FamilyClass.Should().Be(XmedFontFamilyClass.DontCare);
        terminal.PitchDecorations.Should().Be(0x40000);
        terminal.ScriptId.Should().Be(0x101);
        terminal.Encoding.Should().NotBeNull();
        terminal.Encoding!.CodePage.Should().Be(terminal.CodePage);
    }


    [Theory]
    [InlineData("Text_Hallo_col_blue_13.xmed.bin", 0x00, 0x00, 0xFF)]
    [InlineData("Text_Hallo_col_blue1_13.xmed.bin", 0x00, 0xFF, 0xFF)]
    [InlineData("Text_Hallo_col_bordeau_13.xmed.bin", 0x88, 0x00, 0x00)]
    [InlineData("Text_Hallo_col_green_13.xmed.bin", 0x00, 0xFF, 0x00)]
    [InlineData("Text_Hallo_col_lightgreen_13.xmed.bin", 0xCC, 0xFF, 0x99)]
    [InlineData("Text_Hallo_col_orange_13.xmed.bin", 0xFF, 0xCC, 0x66)]
    [InlineData("Text_Hallo_col_pink_13.xmed.bin", 0xFF, 0x00, 0xFF)]
    [InlineData("Text_Hallo_col_yellow_13.xmed.bin", 0xFF, 0xFF, 0x00)]
    public void Text_color_samples_should_BeRead(string fileName, byte expectedR, byte expectedG, byte expectedB)
    {
        var doc = ReadDocument(fileName);
        doc.Runs.Should().ContainSingle();
        var run = doc.Runs.Single();
        run.ForeColor.R.Should().Be(expectedR);
        run.ForeColor.G.Should().Be(expectedG);
        run.ForeColor.B.Should().Be(expectedB);
        doc.Styles.Should().Contain(style =>
            style.ForegroundColor.R == expectedR
            && style.ForegroundColor.G == expectedG
            && style.ForegroundColor.B == expectedB);
    }

    [Theory]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_lh13_13.xmed.bin", 130, 178)]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_lh20_13.xmed.bin", 200, 178)]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_lh29_13.xmed.bin", 290, 178)]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_lh39_13.xmed.bin", 390, 178)]
    [InlineData("FontSize/D01_LineHeight_18_13.xmed.bin", 18, 255)]
    [InlineData("FontSize/D02_LineHeight_36_13.xmed.bin", 36, 255)]
    public void Line_height_samples_should_capture_paragraph_bounds(string fileName, int expectedBaseline, int expectedWidth)
    {
        var document = ReadDocument(fileName);

        document.Paragraphs.Should().NotBeEmpty();
        var paragraph = document.Paragraphs[0];
        paragraph.BaselineOffset.Should().Be(expectedBaseline);
        paragraph.ParagraphWidth.Should().Be(expectedWidth);
    }

    [Theory]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_lh13_13.xmed.bin", 64, 0)]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_lh20_13.xmed.bin", 64, 0)]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_lh29_13.xmed.bin", 64, 0)]
    [InlineData("FontSize/Text_Single_Line_Multi_Style3_lh39_13.xmed.bin", 64, 0)]
    [InlineData("FontSize/D01_LineHeight_18_13.xmed.bin", 64, 0)]
    [InlineData("FontSize/D02_LineHeight_36_13.xmed.bin", 64, 0)]
    public void Line_height_samples_should_capture_paragraph_spacing_records(string fileName, int expectedTop, int expectedBottom)
    {
        var document = ReadDocument(fileName);

        document.Paragraphs.Should().NotBeEmpty();
        var paragraph = document.Paragraphs[0];
        _output.WriteLine($"Spacing offsets: top={paragraph.SpacingTopOffset} bottom={paragraph.SpacingBottomOffset}");
        paragraph.SpacingTopOffset.Should().Be(expectedTop);
        paragraph.SpacingBottomOffset.Should().Be(expectedBottom);
    }

    [Fact]
    public void Multi_Text_color_samples_should_BeRead()
    {
        var doc = ReadDocument("MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin");

        doc.Runs.Should().NotBeEmpty();

        _logger.LogInformation("Document has {StyleCount} styles", doc.Styles.Count);
        foreach (var style in doc.Styles.OrderBy(s => s.StyleId))
        {
            string colorIndex = style.ColorIndex.HasValue ? $"0x{style.ColorIndex.Value:X2}" : "<null>";
            string inlineColor = style.ForegroundColor.ToHex();
            _logger.LogInformation(
                "Style {StyleId}: font '{Font}' size {Size} colorIndex {ColorIndex} inline {InlineColor} flags {Flags}",
                style.StyleId,
                style.FontName,
                style.FontSize,
                colorIndex,
                inlineColor,
                style.Flags);
            _output.WriteLine(
                $"Style {style.StyleId}: font '{style.FontName}' size {style.FontSize} colorIndex {colorIndex} inline {inlineColor} flags {style.Flags}");
        }
    }

   


    private XmedDocument ReadDocument(string fileName)
    {
        var path = TestContextHarness.GetTextAssetPath(fileName);
        var bytes = File.ReadAllBytes(path);
        var reader = new BlXmedTextReader(_logger);
        return reader.Read(bytes);
    }

    private void DumpTokenWindows(string relativePath)
    {
        var assetPath = TestContextHarness.GetTextAssetPath(relativePath);
        var bytes = File.ReadAllBytes(assetPath);
        var tokenizer = new BlXmedTokenizer();
        var (tokens, _) = tokenizer.Tokenize(bytes);

        foreach (var index in FindTokenIndices(tokens, 0x03, "0004"))
            DumpTokens(tokens, index, 32, $"03:0004@{index}");

        foreach (var index in FindTokenIndices(tokens, 0x03, "0006"))
            DumpTokens(tokens, index, 160, $"03:0006@{index}");
    }

    private static IReadOnlyList<int> FindTokenIndices(IReadOnlyList<BlXmedToken> tokens, int typeValue, string ascii)
    {
        var matches = new List<int>();
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Type != BlXmedToken.TokenType.PrefixedHex)
                continue;
            if (token.TypeValue != typeValue)
                continue;
            if (string.IsNullOrEmpty(token.Ascii))
                continue;
            if (!token.Ascii.StartsWith(ascii, StringComparison.OrdinalIgnoreCase))
                continue;
            matches.Add(i);
        }
        return matches;
    }

    private void DumpTokens(IReadOnlyList<BlXmedToken> tokens, int startIndex, int count, string label)
    {
        _logger.LogInformation("Token window {Label} starting at {StartIndex}", label, startIndex);
        _output.WriteLine($"Token window {label} starting at index {startIndex}");

        var windowTokens = tokens.Skip(startIndex).Take(count).ToList();

        for (int offset = 0; offset < windowTokens.Count; offset++)
        {
            var token = windowTokens[offset];
            string ascii = token.Ascii ?? "<null>";
            string value = token.Value.HasValue ? token.Value.Value.ToString(CultureInfo.InvariantCulture) : "<null>";
            string type = token.TypeValue.HasValue ? $"0x{token.TypeValue.Value:X2}" : "<null>";
            string line = $"  [{startIndex + offset:D4}] {token.Type,-12} ascii={ascii} value={value} type={type}";
            _logger.LogInformation("{TokenLine}", line);
            _output.WriteLine(line);
            if (offset > 0 && token.Type == BlXmedToken.TokenType.PrefixedHex && token.TypeValue == 0x03)
                break;
            if (token.Type == BlXmedToken.TokenType.Block00)
                break;
        }

        string compact = BlXmedTokenizer.DumpTokensCompact(windowTokens);
        string ultra = BlXmedTokenizer.DumpTokensUltraCompact(windowTokens);

        _logger.LogInformation("Token window {Label} compact dump:{NewLine}{Dump}", label, Environment.NewLine, compact);
        _output.WriteLine($"Token window {label} compact dump:\n{compact}");

        _logger.LogInformation("Token window {Label} ultra-compact dump:{NewLine}{Dump}", label, Environment.NewLine, ultra);
        _output.WriteLine($"Token window {label} ultra-compact dump:\n{ultra}");
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
