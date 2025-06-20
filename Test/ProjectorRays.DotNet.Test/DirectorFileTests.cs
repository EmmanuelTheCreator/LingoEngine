using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.director.Chunks;
using ProjectorRays.Director;
using ProjectorRays.IO;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test;

public class DirectorFileTests
{
    private readonly ILogger<DirectorFileTests> _logger;

    public DirectorFileTests(ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
        });

        _logger = factory.CreateLogger<DirectorFileTests>();
    }

    [Theory]
    [InlineData("Dir_With_One_Img_Sprite_Hallo.dir")]
    [InlineData("Dir_With_One_Tex_Sprite_Hallo.dir")]
    public void CanReadDirectorFile(string fileName)
    {
        var path = GetPath(fileName);
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new DirectorFile(_logger);
        Assert.True(dir.Read(stream));
    }


    [Fact]
    public void ImgCastContainsBitmapMember()
    {
        var path = GetPath("ImgCast.cst");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new DirectorFile(_logger);
        Assert.True(dir.Read(stream));
        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        bool found = false;
        if (dir.Casts.Count > 0)
        {
            foreach (var id in dir.Casts[0].MemberIDs)
            {
                var chunk = (CastMemberChunk)dir.GetChunk(CASt, id);
                if (chunk.Type == MemberType.BitmapMember)
                {
                    found = true;
                    break;
                }
            }
        }
        Assert.True(found);
    }

    [Fact]
    public void TextCastTextContainsHallo()
    {
        var path = GetPath("Text_Hallo_fontsize14.cst");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new DirectorFile(_logger);
        Assert.True(dir.Read(stream));
        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        string text = string.Empty;
        if (dir.Casts.Count > 0)
        {
            foreach (var id in dir.Casts[0].MemberIDs)
            {
                var chunk = (CastMemberChunk)dir.GetChunk(CASt, id);
                dir.Logger.LogInformation($"CastMember Type={chunk.Type}, Name='{chunk.GetName()}', ScriptText='{chunk.GetScriptText()}'");
                dir.Logger.LogInformation("Raw SpecificData: " + BitConverter.ToString(chunk.SpecificData.Data, chunk.SpecificData.Offset, chunk.SpecificData.Size));
                if (chunk.Type == MemberType.FieldMember)
                {
                    var field = (CastMemberChunk)dir.GetChunk(CASt, id);
                    if (field.DecodedText is CastMemberTextRead styled)
                    {
                        //   var reader = CastMemberTextRead.FromSpecificData(chunk.SpecificData);
                        //Console.WriteLine(reader.Text); // e.g. "Hallo"
                        text = styled.Text;

                    }
                    break;
                }
            }
        }
        Assert.Contains("Hallo", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DirFileTextContainsHallo()
    {
        var path = GetPath("Dir_With_One_Tex_Sprite_Hallo.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new DirectorFile(_logger);
        Assert.True(dir.Read(stream));
        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        string text = string.Empty;
        foreach (var cast in dir.Casts)
        {
            foreach (var id in cast.MemberIDs)
            {
                var chunk = (CastMemberChunk)dir.GetChunk(CASt, id);
                if (chunk.Type == MemberType.FieldMember)
                {
                    text = chunk.GetScriptText();
                    break;
                }
            }
            if (text.Length > 0) break;
        }
        Assert.Contains("Hallo", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ScoreTimelineHasSpriteFrames()
    {
        var path = GetPath("Dir_With_One_Img_Sprite_Hallo.dir");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new DirectorFile(_logger);
        Assert.True(dir.Read(stream));

        Assert.NotNull(dir.Score);
        Assert.NotEmpty(dir.Score!.Frames);
        var first = dir.Score.Frames[0];
        Assert.True(first.EndFrame >= first.StartFrame);
    }

    [Fact]
    public void CanDumpScriptText()
    {
        var path = GetPath("TextCast.cst");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new DirectorFile(_logger);
        Assert.True(dir.Read(stream));

        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        string? text = null;
        foreach (var cast in dir.Casts)
        {
            foreach (var id in cast.MemberIDs)
            {
                var chunk = (CastMemberChunk)dir.GetChunk(CASt, id);
                var scriptId = chunk.GetScriptID();
                if (scriptId != 0)
                {
                    var script = dir.GetScript((int)scriptId);
                    script?.Parse();
                    text = script?.ScriptText(FileIO.PlatformLineEnding, false);
                    break;
                }
            }
            if (text != null) break;
        }

        Assert.NotNull(text);
    }

    [Fact]
    public void RestoresScriptTextIntoMembers()
    {
        var path = GetPath("TextCast.cst");
        var data = File.ReadAllBytes(path);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new DirectorFile(_logger);
        Assert.True(dir.Read(stream));

        dir.ParseScripts();
        dir.RestoreScriptText();

        const uint CASt = ((uint)'C' << 24) | ((uint)'A' << 16) | ((uint)'S' << 8) | (uint)'t';
        bool found = false;
        foreach (var cast in dir.Casts)
        {
            foreach (var id in cast.MemberIDs)
            {
                var chunk = (CastMemberChunk)dir.GetChunk(CASt, id);
                if (chunk.GetScriptID() != 0 && !string.IsNullOrEmpty(chunk.GetScriptText()))
                {
                    found = true;
                    break;
                }
            }
            if (found) break;
        }

        Assert.True(found);
    }

    private static string GetPath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "../../../../TestData", fileName);
    }
}
