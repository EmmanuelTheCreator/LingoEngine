using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using ProjectorRays.CastMembers;
using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.director.Scores;
using ProjectorRays.DotNet.Test.XMED;
using System.Linq;
using System.Text;

namespace ProjectorRays.DotNet.Test;

public static class TestFileReader
{
    public static byte[] ReadHexFile(string path)
    {
        var lines = File.ReadAllLines(path);
        var bytes = new List<byte>();
        foreach (var line in lines)
        {
            foreach (var part in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                bytes.Add(Convert.ToByte(part, 16));
            }
        }

        return bytes.ToArray();
    }
    public static bool ScoreExists(string xmedFile)
    {
        var dump = Path.ChangeExtension(xmedFile, ".score.txt");
        return File.Exists(dump);
    }
    public static byte[] ReadScore(string dirFile, bool dumpToFile = true)
    {
        var dump = Path.ChangeExtension(dirFile, ".score.txt");
        if (!File.Exists(dump))
        {
            var previous = RaysScoreChunk.FrameParserFactory;
            try
            {
                RaysScoreChunk.FrameParserFactory = (logger, annotator) => dumpToFile
                ?new RaysScoreFrameParserV2ToFile(logger, annotator, dirFile)
                :new RaysScoreFrameParserV2(logger, annotator);
                var data = File.ReadAllBytes(dirFile);
                var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
                using var factory = LoggerFactory.Create(builder => { });
                var dir = new RaysDirectorFile(factory.CreateLogger("TestFileReader"), dirFile);
                dir.Read(stream);
            }
            finally
            {
                RaysScoreChunk.FrameParserFactory = previous;
            }
        }

        return ReadHexFile(dump);
    }

    public static bool XmedExists(string xmedFile)
    {
        var dump = Path.ChangeExtension(xmedFile, ".xmed.txt");
        return File.Exists(dump);
    }
    public static byte[] ReadXmed(string xmedFile)
    {
        var dump = Path.ChangeExtension(xmedFile, ".xmed.txt");
        if (!File.Exists(dump))
        {
            var previous = RaysCastMemberTextRead.XmedReaderFactory;
            try
            {
                RaysCastMemberTextRead.XmedReaderFactory = () => new XmedReaderToFile(xmedFile);
                var data = File.ReadAllBytes(xmedFile);
                var view = new BufferView(data, data.Length);
                RaysCastMemberTextRead.XmedReaderFactory().Read(view);
            }
            finally
            {
                RaysCastMemberTextRead.XmedReaderFactory = previous;
            }
        }

        return ReadHexFile(dump);
    }

    public static void ReadHeader(string file)
    {
        var dump = Path.ChangeExtension(file, ".header.txt");

        var data = File.ReadAllBytes(file);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        uint magic = stream.ReadUint32();
        if (magic == RaysDirectorFile.FOURCC('X', 'F', 'I', 'R'))
            stream.Endianness = Endianness.LittleEndian;
        uint fileLen = stream.ReadUint32();
        uint codec = stream.ReadUint32();
        stream.Seek(0);

        using var factory = LoggerFactory.Create(builder => { });
        var dir = new RaysDirectorFile(factory.CreateLogger("Header"), file);
        dir.Read(stream, parseChunks: false);
        var infos = dir.ChunkInfos.Values.OrderBy(c => c.Offset);
        using var writer = new StreamWriter(dump, false, Encoding.UTF8);
        writer.WriteLine("[HEADER]");
        writer.WriteLine($"{0:X8} {RaysUtil.FourCCToString(magic, escape: false)}");
        writer.WriteLine($"{4:X8} {fileLen}");
        writer.WriteLine($"{8:X8} {RaysUtil.FourCCToString(codec, escape: false)}");
        writer.WriteLine();
        writer.WriteLine("[CHUNKS]");
        writer.WriteLine("# Offset FourCC Len UncompressedLen Id");
        foreach (var info in infos)
        {
            string fourCC = RaysUtil.FourCCToString(info.FourCC, escape: false);
            writer.WriteLine($"{info.Offset:X8} {fourCC} {info.Len} {info.UncompressedLen} {info.Id}");
        }
    }
}
