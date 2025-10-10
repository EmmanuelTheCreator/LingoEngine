using System.IO;
using System.Linq;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using BlingoEngine.IO.Legacy.Texts;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

// Each test targets a specific XMED sample under Texts_Fields.
// The assertions reconstruct the member text exclusively from the parsed runs.
public class XmedFileTest
{
    [Fact]
    public void Text_Hallo_tab_true_file_should_report_tabs()
    {
        var document = ReadDocument("Text_Hallo_tab_true_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
        document.Runs.Should().HaveCount(1);
    }
    [Fact]
    public void Text_Hallo()
    {
        var document = ReadDocument("Text_Hallo_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
        document.Runs.Should().HaveCount(1);
    }
    [Fact]
    public void Text_3_Paragraps_13()
    {
        var document = ReadDocument("Text_3_Paragraps_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        //textFromRuns.ShouldMatchNormalized("My first paragraph centered");
        document.Runs.Should().HaveCount(1);
    }

    [Fact]
    public void Text_Hallo_multifont_file_should_list_multiple_fonts()
    {
        var document = ReadDocument("Text_Hallo_multifont_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
        document.Runs.Should().HaveCount(1);
    }

    [Fact]
    public void Text_Hallo_multiLine_file_should_preserve_carriage_returns()
    {
        var document = ReadDocument("Text_Hallo_multiLine_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo\rmulti line\ris longer\rYES!");
    }

    [Fact]
    public void Text_Single_Line_Multi_Style_file_should_read_long_text_and_runs()
    {
        var document = ReadDocument("Text_Single_Line_Multi_Style_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("This text is red, Arial,12px,  The text is yellow, Tahoma, 9px, , bold, italic, underline The text is green, font Terminal, 18px, with spacing of 39 The text is orange, Tahoma, 9px, bold, italic, underline This text is red, Arial,12px, again");
    }
    [Fact]
    public void Text_Multi_Line_Multi_Style_file_should_read_long_text_and_runs()
    {
        var document = ReadDocument("Text_Multi_Line_Multi_Style_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        //textFromRuns.ShouldMatchNormalized("This text is red, Arial,12px,  The text is yellow, Tahoma, 9px, , bold, italic, underline The text is green, font Terminal, 18px, with spacing of 39 The text is orange, Tahoma, 9px, bold, italic, underline This text is red, Arial,12px, again");
    }

    [Fact]
    public void Text_Hallo_text_transform_all_on_file_should_merge_styles_into_run_text()
    {
        var document = ReadDocument("Text_Hallo_text_transform_all_on_13.xmed.bin");

        string textFromRuns = string.Concat(document.Runs.Select(run => run.Text));
        textFromRuns.ShouldMatchNormalized("Hallo");
    }

    private static XmedDocument ReadDocument(string fileName)
    {
        var path = TestContextHarness.GetAssetPath($"Texts_Fields/{fileName}");
        var bytes = File.ReadAllBytes(path);
        var reader = new BlXmedTextReader();
        return reader.Read(bytes);
    }
}
