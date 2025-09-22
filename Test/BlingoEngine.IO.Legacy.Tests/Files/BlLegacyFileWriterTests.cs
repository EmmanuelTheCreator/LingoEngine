using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Files;
using BlingoEngine.IO.Legacy.Scripts;
using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Shapes;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Fields;

using FluentAssertions;

using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Files;

public class BlLegacyFileWriterTests
{
    [Fact]
    public void DirWriter_RoundTripsResources()
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x01, 0x02, 0x03 }
        });
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "Lscr_0001.bin",
            Bytes = new byte[] { 0x10, 0x11 }
        });

        var writer = new BlDirFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "movie.dir", leaveOpen: true);
        var roundTrip = context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        var block = context.DataBlock!;
        block.PayloadStart.Should().Be(12);
        block.Format.IsBigEndian.Should().BeFalse();
        block.Format.Codec.Should().Be(BlRifxCodec.MV93);
        block.Format.MapVersion.Should().Be(BlLegacyFormatConstants.ClassicMapVersion);
        block.Format.ArchiveVersion.Should().Be(BlLegacyFormatConstants.Director101ArchiveVersion);

        roundTrip.Files.Should().HaveCount(2);
        roundTrip.Files[0].Bytes.Should().Equal(container.Files[0].Bytes);
        roundTrip.Files[1].Bytes.Should().Equal(container.Files[1].Bytes);
    }

    [Fact]
    public void DirWriter_WritesTextCastLibrary()
    {
        const string textPayload = "Hello Director";
        var container = BlLegacyTextLibraryBuilder.BuildSingleMemberTextLibrary("text member", textPayload);

        var writer = new BlDirFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "movie.dir", leaveOpen: true);
        context.ReadDirFilesContainer();

        var texts = context.ReadTexts();
        texts.Should().ContainSingle();
        var text = texts[0];
        text.Format.Should().Be(BlLegacyTextFormatKind.Stxt);
        text.Bytes.Should().Equal(Encoding.Latin1.GetBytes(textPayload));
    }

    [Fact]
    public void DirWriter_WritesFieldCastLibrary()
    {
        const string fieldPayload = "Editable Field";
        var container = BlLegacyFieldLibraryBuilder.BuildSingleMemberFieldLibrary("field member", fieldPayload);

        var writer = new BlDirFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "movie.dir", leaveOpen: true);
        context.ReadDirFilesContainer();

        var fields = context.ReadFields();
        fields.Should().ContainSingle();
        var field = fields[0];
        field.Format.Should().Be(BlLegacyFieldFormatKind.Stxt);
        field.Bytes.Should().Equal(Encoding.Latin1.GetBytes(fieldPayload));
    }

    [Fact]
    public void DirWriter_WritesBehaviorCastLibrary()
    {
        const string scriptText = "on beginSprite me\n  put 42\nend";
        var container = BlLegacyBehaviorLibraryBuilder.BuildSingleMemberBehaviorLibrary("behavior member", scriptText);

        var writer = new BlDirFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "movie.dir", leaveOpen: true);
        context.ReadDirFilesContainer();

        var scripts = context.ReadScripts();
        scripts.Should().ContainSingle();
        var script = scripts[0];
        script.Format.Should().Be(BlLegacyScriptFormatKind.Behavior);
        script.Text.Should().Be(scriptText.Replace("\n", "\r"));
        script.Name.Should().Be("behavior member");
    }

    [Fact]
    public void CstWriter_WritesCastContainer()
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x20, 0x21, 0x22 }
        });

        var writer = new BlCstFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "library.cst", leaveOpen: true);
        var roundTrip = context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        var format = context.DataBlock!.Format;
        format.Codec.Should().Be(BlRifxCodec.MV93);
        format.MapVersion.Should().Be(BlLegacyFormatConstants.ClassicMapVersion);
        format.ArchiveVersion.Should().Be(BlLegacyFormatConstants.Director101ArchiveVersion);
        roundTrip.Files.Should().ContainSingle();
        roundTrip.Files[0].Bytes.Should().Equal(container.Files[0].Bytes);
    }

    [Fact]
    public void CstWriter_WritesSoundCastLibrary()
    {
        var mp3Bytes = new byte[]
        {
            (byte)'I', (byte)'D', (byte)'3', 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            (byte)'T', (byte)'A', (byte)'G', 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xFF, 0xFB, 0x50, 0x80
        };

        var container = BlLegacySoundLibraryBuilder.BuildSingleMemberMp3Library("mp3 member", mp3Bytes);

        var writer = new BlCstFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "library.cst", leaveOpen: true);
        context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        var format = context.DataBlock!.Format;
        format.Codec.Should().Be(BlRifxCodec.MV93);
        format.MapVersion.Should().Be(BlLegacyFormatConstants.ClassicMapVersion);
        format.ArchiveVersion.Should().Be(BlLegacyFormatConstants.Director101ArchiveVersion);

        var sounds = context.ReadSounds();
        sounds.Should().ContainSingle();
        var sound = sounds[0];
        sound.Format.Should().Be(BlLegacySoundFormatKind.Mp3);
        sound.Bytes.Should().Equal(mp3Bytes);
    }

    [Fact]
    public void CstWriter_WritesBitmapCastLibrary()
    {
        var bitdPayload = new byte[] { 0x01, 0x00, 0x81, 0x7F, 0x02 };

        var container = BlLegacyBitmapLibraryBuilder.BuildSingleMemberBitmapLibrary(
            "bitmap member",
            bitdBytes: bitdPayload);

        var writer = new BlCstFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "library.cst", leaveOpen: true);
        context.ReadDirFilesContainer();

        var bitmaps = context.ReadBitmaps();
        bitmaps.Should().ContainSingle();
        var bitmap = bitmaps[0];
        bitmap.Format.Should().Be(BlLegacyBitmapFormatKind.Bitd);
        bitmap.Bytes.Should().Equal(bitdPayload);
    }

    [Fact]
    public void CstWriter_WritesShapeCastLibrary()
    {
        var shapeRecord = new byte[]
        {
            0x00, 0x02,
            0xFF, 0xF6,
            0xFF, 0xF0,
            0x00, 0x14,
            0x00, 0x20,
            0x12, 0x34,
            0x80,
            0x7F,
            0x1C,
            0x08,
            0x02
        };

        const string memberName = "shape member";
        var container = BlLegacyShapeLibraryBuilder.BuildSingleMemberShapeLibrary(
            memberName,
            shapeRecord);

        var castResource = container.Files.Single(file => file.FileName == "CASt_0003.bin");
        var header = castResource.Bytes.AsSpan();
        var nameByteCount = Encoding.UTF8.GetByteCount(memberName);
        var expectedInfoLength = (uint)(1 + nameByteCount);
        BinaryPrimitives.ReadUInt32BigEndian(header.Slice(0, 4)).Should().Be((uint)BlLegacyCastMemberType.Shape);
        BinaryPrimitives.ReadUInt32BigEndian(header.Slice(4, 4)).Should().Be(expectedInfoLength);
        BinaryPrimitives.ReadUInt32BigEndian(header.Slice(8, 4)).Should().Be((uint)shapeRecord.Length);

        var info = header.Slice(12, (int)expectedInfoLength);
        info[0].Should().Be((byte)nameByteCount);
        Encoding.UTF8.GetString(info.Slice(1)).Should().Be(memberName);

        var encodedShape = header.Slice(12 + (int)expectedInfoLength, shapeRecord.Length);
        encodedShape.ToArray().Should().Equal(shapeRecord);

        var writer = new BlCstFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "library.cst", leaveOpen: true);
        context.ReadDirFilesContainer();

        var shapes = context.ReadShapes();
        shapes.Should().ContainSingle();
        var shape = shapes[0];
        shape.Format.Should().Be(BlLegacyShapeFormatKind.Director4To10UnsignedColors);
        shape.Bytes.Should().Equal(shapeRecord);
    }

    [Fact]
    public void DxrWriter_WritesProtectedMovie()
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x30, 0x31 }
        });

        var writer = new BlDxrFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "movie.dxr", leaveOpen: true);
        context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        context.DataBlock!.Format.Codec.Should().Be(BlRifxCodec.MC95);
    }

    [Fact]
    public void CxtWriter_WritesProtectedCastLibrary()
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x25 }
        });

        var writer = new BlCxtFileWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, container);

        stream.Position = 0;
        using var context = new ReaderContext(stream, "library.cxt", leaveOpen: true);
        context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        var format = context.DataBlock!.Format;
        format.Codec.Should().Be(BlRifxCodec.MC95);
        format.IsBigEndian.Should().BeFalse();
    }

    [Theory]
    [InlineData("library.cst", BlRifxCodec.MV93)]
    [InlineData("library.cxt", BlRifxCodec.MC95)]
    [InlineData("movie.dir", BlRifxCodec.MV93)]
    [InlineData("movie.dxr", BlRifxCodec.MC95)]
    [InlineData("movie.dcr", BlRifxCodec.MC95)]
    public void LegacyWriter_SelectsWriterFromExtension(string fileName, BlRifxCodec expectedCodec)
    {
        var container = new DirFilesContainerDTO();
        container.Files.Add(new DirFileResourceDTO
        {
            FileName = "CASt_0000.bin",
            Bytes = new byte[] { 0x40 }
        });

        var writer = new BlLegacyWriter();
        using var stream = new MemoryStream();
        writer.Write(stream, fileName, container, leaveOpen: true);

        stream.CanWrite.Should().BeTrue();

        stream.Position = 0;
        using var context = new ReaderContext(stream, fileName, leaveOpen: true);
        context.ReadDirFilesContainer();

        context.DataBlock.Should().NotBeNull();
        context.DataBlock!.Format.Codec.Should().Be(expectedCodec);
    }
}
