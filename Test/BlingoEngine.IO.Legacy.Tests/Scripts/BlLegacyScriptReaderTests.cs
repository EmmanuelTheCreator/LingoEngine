using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Scripts;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Tools;
using FluentAssertions;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Scripts;

public class BlLegacyScriptReaderTests
{
    [Fact]
    public void ReadBehaviorDir_ExtractsBehaviorScript()
    {
        var scripts = TestContextHarness.LoadScripts("Behaviors/5spritesTest_With_Behavior.dir");

        scripts.Should().NotBeNull();
        scripts.Should().NotBeEmpty();

        var script = scripts.Should().ContainSingle(s => s.ResourceId == 5).Subject;
        script.Format.Should().Be(BlLegacyScriptFormatKind.Behavior);
        script.Bytes.Should().NotBeNull();
        script.Bytes.Should().HaveCount(148);
    }

    [Fact]
    public void ReadBehaviorDir_ExtractsBehaviorScriptContent()
    {
        var singleScripts = TestContextHarness.LoadScripts("Behaviors/5spritesTest_With_Behavior.dir");

        var singleScript = singleScripts.Should().ContainSingle(s => s.ResourceId == 5).Subject;
        singleScript.Text.ShouldMatchNormalized("on beginsprite\r  put 12\rend");
        singleScript.Name.Should().Be("MyTestBe");

        var multiScripts = TestContextHarness.LoadScripts("Behaviors/Two_Behaviors.dir");

        multiScripts.Should().HaveCount(2);
        multiScripts.Should().OnlyContain(s => s.Text != null && s.Name != null);

        var expectedScripts = new Dictionary<string, string>
        {
            ["on exitFrame me\r  put 20\r    put \"I'm in cast slot 1\"\rend"] = "MyFirstBehaviorOnFrame10",
            ["on exitFrame me\r  put \"hallo\"\r  put \"I'm in cast slot 3\"\rend"] = "MeSecondBehaviorOnFrame20"
        };

        var normalizedExpectations = expectedScripts.ToDictionary(
            pair => ResultTestHelper.NormalizeLineEndings(pair.Key),
            pair => pair);

        foreach (var script in multiScripts)
        {
            var normalizedText = ResultTestHelper.NormalizeLineEndings(script.Text);
            normalizedExpectations.Should().ContainKey(normalizedText);

            var expected = normalizedExpectations[normalizedText];
            script.Text.ShouldMatchNormalized(expected.Key);
            script.Name.Should().Be(expected.Value);

            normalizedExpectations.Remove(normalizedText);
        }

        normalizedExpectations.Should().BeEmpty();
    }

    [Fact]
    public void ReadSyntheticScripts_IdentifiesBehaviorMovieAndParentCategories()
    {
        var expectedOrder = new[]
        {
            BlLegacyScriptFormatKind.Behavior,
            BlLegacyScriptFormatKind.Movie,
            BlLegacyScriptFormatKind.Parent
        };

        using var stream = new MemoryStream();
        using var context = CreateSyntheticContext(stream, archiveVersion: 0x000004C7);
        var writer = new BlLegacyScriptWriter(stream);

        var expectedScripts = new List<(int ScriptId, BlLegacyScriptFormatKind Format)>();
        var nextResourceId = 1;

        foreach (var format in expectedOrder)
        {
            var scriptId = nextResourceId++;
            var castId = nextResourceId++;

            var castEntry = writer.WriteCastScript(
                castId,
                scriptNumber: scriptId,
                scriptResourceId: scriptId,
                format,
                scriptText: null,
                scriptName: null);

            var scriptPayload = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var scriptEntry = writer.WriteScriptResource(scriptId, scriptPayload);

            context.AddResource(castEntry);
            context.AddResource(scriptEntry);

            expectedScripts.Add((scriptId, format));
        }

        ResetStream(context, stream);

        var reader = new BlLegacyScriptReader(context);
        var scripts = reader.Read();

        scripts.Should().HaveCount(expectedScripts.Count);

        for (var i = 0; i < expectedScripts.Count; i++)
        {
            var expected = expectedScripts[i];
            var script = scripts[i];

            script.ResourceId.Should().Be(expected.ScriptId);
            script.Format.Should().Be(expected.Format);
        }
    }

    [Fact]
    public void ReadSyntheticPointerScript_ExtractsTextAndName()
    {
        using var stream = new MemoryStream();
        using var context = CreateSyntheticContext(stream, archiveVersion: 0x000004C7);
        var writer = new BlLegacyScriptWriter(stream);

        const int scriptId = 101;
        const int castId = 201;
        const string scriptText = "on beginSprite\r  put 42\rend";
        const string scriptName = "PointerLayout";

        var castEntry = writer.WriteCastScript(
            castId,
            scriptNumber: scriptId,
            scriptResourceId: scriptId,
            BlLegacyScriptFormatKind.Behavior,
            scriptText,
            scriptName,
            BlLegacyScriptInfoLayout.PointerTable);

        var scriptEntry = writer.WriteScriptResource(scriptId, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });

        context.AddResource(castEntry);
        context.AddResource(scriptEntry);

        ResetStream(context, stream);

        var scripts = new BlLegacyScriptReader(context).Read();

        var script = scripts.Should().ContainSingle(s => s.ResourceId == scriptId).Subject;
        script.Format.Should().Be(BlLegacyScriptFormatKind.Behavior);
        script.Text.ShouldMatchNormalized(scriptText);
        script.Name.Should().Be(scriptName);
    }

    [Fact]
    public void ReadSyntheticLegacyScript_UsesLengthAdjacentLayout()
    {
        using var stream = new MemoryStream();
        using var context = CreateSyntheticContext(stream, archiveVersion: 0xDEADBEEF);
        var writer = new BlLegacyScriptWriter(stream);

        const int scriptId = 305;
        const int castId = 405;
        const string scriptText = "on oldExport\r  put 3\rend";
        const string scriptName = "LegacyLayout";

        var castEntry = writer.WriteCastScript(
            castId,
            scriptNumber: scriptId,
            scriptResourceId: scriptId,
            BlLegacyScriptFormatKind.Movie,
            scriptText,
            scriptName,
            BlLegacyScriptInfoLayout.LegacyTextAfterLength);

        var scriptEntry = writer.WriteScriptResource(scriptId, new byte[] { 0xCA, 0xFE });

        context.AddResource(castEntry);
        context.AddResource(scriptEntry);

        ResetStream(context, stream);

        var scripts = new BlLegacyScriptReader(context).Read();

        var script = scripts.Should().ContainSingle(s => s.ResourceId == scriptId).Subject;
        script.Format.Should().Be(BlLegacyScriptFormatKind.Movie);
        script.Text.ShouldMatchNormalized(scriptText);
        script.Name.Should().Be(scriptName);
    }

    [Fact(Skip ="to test")]
    public void WriteBinTxt()
    {
        var item = "Behaviors/5spritesTest_With_Behavior.dir";
        var texts = TestContextHarness.LoadScripts(item);
        var textItem = texts[0];
        var pathBin = TestContextHarness.GetAssetPath("Behaviors/" + Path.GetFileNameWithoutExtension(item) + "_" + texts[0].ResourceId + ".script.bin");
        var pathBin2 = TestContextHarness.GetAssetPath("Behaviors/" + Path.GetFileNameWithoutExtension(item) + "_" + texts[0].ResourceId + ".script.txt");
        File.WriteAllText(pathBin2, textItem.Bytes.ToHexString());
        File.WriteAllBytes(pathBin, textItem.Bytes);
        texts.Should().HaveCount(1);
        var text = texts[0];
        //var test = new BlXmedReader().Read(text.Bytes);
        //var textstring = texts[0].Bytes.ToHexString();
        //System.IO.File.WriteAllBytes(@"c:\temp\director\Text_Hallo.xmed", texts[0].Bytes);
    }

    private static ReaderContext CreateSyntheticContext(Stream stream, uint archiveVersion)
    {
        var context = new ReaderContext(stream, "synthetic.dir", leaveOpen: true);
        context.RegisterRifxOffset(0);

        var dataBlock = new BlDataBlock();
        dataBlock.Format.ArchiveVersion = archiveVersion;
        context.RegisterDataBlock(dataBlock);

        return context;
    }

    private static void ResetStream(ReaderContext context, Stream stream)
    {
        stream.Position = 0;
        context.Reader.Position = 0;
    }
}
