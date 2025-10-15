using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
using BlingoEngine.IO.Legacy.Tools;
using FluentAssertions;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class BlLegacyTextReaderTests
{
    [Fact]
    public void GenerateXmedText()
    {
        //var folder = "Texts_Fields";
        //var folder = "Texts_Fields/Tests2";
        var folder = "Texts_Fields/MemberTests";
        var cstFiles = TestContextHarness.GetAllFilesFromFolder(folder, "*.cst");
        foreach (var item in cstFiles)
        {
            var texts = TestContextHarness.LoadTexts(item);
            if (texts.Count > 0)
            {
                foreach (var textItem in texts)
                {

                    if (textItem.Format == BlLegacyTextFormatKind.Xmed)
                    {
                        var path = TestContextHarness.GetAssetPath(folder+"/" + Path.GetFileNameWithoutExtension(item)+"_"+ textItem.ResourceId + ".xmed.txt");
                        if (File.Exists(path))
                            continue;

                        File.WriteAllText(path, textItem.Bytes.ToHexString());
                        var pathBin = TestContextHarness.GetAssetPath(folder+"/" + Path.GetFileNameWithoutExtension(item)+"_"+ textItem.ResourceId + ".xmed.bin");
                        File.WriteAllBytes(pathBin, textItem.Bytes);
                        var pathLog = TestContextHarness.GetAssetPath(folder+"/" + Path.GetFileNameWithoutExtension(item)+"_"+ textItem.ResourceId + ".xmedlog.txt");
                        var tokens = BlXmedTokenizer.Tokenize(textItem.Bytes).Tokens;
                        var log = BlXmedTokenizer.DumpTokensUltraCompact(tokens);
                        File.WriteAllText(pathLog, log);
                    }
                }
            }
        }

    }
    [Fact]
    public void Read_ResolvesXmedPayload()
    {
        var texts = TestContextHarness.LoadTexts("Texts_Fields/Text_Hallo.cst");

        texts.Should().HaveCount(1);
        var text = texts[0];
        text.Format.Should().Be(BlLegacyTextFormatKind.Xmed);
        text.Bytes.Length.Should().Be(1431);
        //var test = new BlXmedReader().Read(text.Bytes);
        //var textstring = texts[0].Bytes.ToHexString();
        //System.IO.File.WriteAllBytes(@"c:\temp\director\Text_Hallo.xmed", texts[0].Bytes);
    }

    [Fact]
    public void Read_HandlesMultipleTextMembers()
    {
        var texts = TestContextHarness.LoadTexts("Texts_Fields/Dir_With_2_Members.dir");

        texts.Should().HaveCount(2);
        texts.Select(t => t.ResourceId)
            .Should().BeEquivalentTo(new[] { 11, 13 });
        texts.Should().OnlyContain(t => t.Format == BlLegacyTextFormatKind.Xmed);
    }
    [Fact]
    public void ReadFieldWidthWidth()
    {
        var libraries = TestContextHarness.LoadTexts("Texts_Fields/Text_Single_Line_Multi_Style.cst");

        
    }
    [Fact]
    public void ReadDirector2StandaloneText_LoadsStxtPayload()
    {
        using var stream = new MemoryStream();
        var writer = new BlLegacyTextWriter(stream);
        var payload = Encoding.ASCII.GetBytes("Director 2 text");
        var entry = writer.WriteStxt(200, payload);

        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());
        context.AddResource(entry);

        var texts = context.ReadTexts();

        texts.Should().HaveCount(1);
        var text = texts[0];
        text.ResourceId.Should().Be(entry.Id);
        text.Format.Should().Be(BlLegacyTextFormatKind.Stxt);
        text.Bytes.Should().Equal(payload);
    }

    [Fact]
    public void ReadDirector4LinkedText_LoadsStxtPayload()
    {
        using var stream = new MemoryStream();
        var writer = new BlLegacyTextWriter(stream);
        var payload = Encoding.ASCII.GetBytes("Linked text");
        var entry = writer.WriteStxt(310, payload);

        stream.Position = 0;

        using var context = new ReaderContext(stream, "synthetic", leaveOpen: true);
        context.RegisterRifxOffset(0);
        context.RegisterDataBlock(new BlDataBlock());
        context.AddResource(entry);
        context.AddResourceRelationship(new BlResourceKeyLink(entry.Id, 75, entry.Tag));

        var texts = context.ReadTexts();

        texts.Should().HaveCount(1);
        var text = texts[0];
        text.ResourceId.Should().Be(entry.Id);
        text.Format.Should().Be(BlLegacyTextFormatKind.Stxt);
        text.Bytes.Should().Equal(payload);
    }
}
