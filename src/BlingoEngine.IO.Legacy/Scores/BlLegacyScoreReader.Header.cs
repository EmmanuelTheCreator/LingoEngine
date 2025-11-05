using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Scores
{

    internal partial class BlLegacyScoreReader
    {
        private record ScoreRawHeader
        {
            public int ActualSize { get; set; }
            public byte UnkA1 { get; set; }
            public byte UnkA2 { get; set; }
            public byte UnkA3 { get; set; }
            public byte UnkA4 { get; set; }
            public int HighestFrame { get; set; }
            public byte UnkB1 { get; set; }
            public byte UnkB2 { get; set; }
            public int SpriteSize { get; set; }
            public byte UnkC1 { get; set; }
            public byte UnkC2 { get; set; }
            public int ChannelCount { get; set; }
            //public short FirstBlockSize { get; set; }
            public int EntryCount { get; internal set; }
            public int EntrySizeSum { get; internal set; }
            public int NotationBase { get; internal set; }
            public int OffsetsOffset { get; internal set; }
            public int HeaderType { get; internal set; }
            public int TotalLength { get; internal set; }
        }

        private ScoreRawHeader ReadMainHeader(BlStreamReader stream)
        {
            return new ScoreRawHeader
            {
                TotalLength = stream.ReadInt32(),
                HeaderType = stream.ReadInt32(), // constantMinus3
                OffsetsOffset = stream.ReadInt32(), // constant12
                EntryCount = stream.ReadInt32(),
                NotationBase = stream.ReadInt32(), // entryCountPlus1
                EntrySizeSum = stream.ReadInt32(),
            };
        }

        private void ReadHeader(byte[] stream, ScoreRawHeader header)
        {
            header.ActualSize = stream.ReadInt32(0);
            header.UnkA1 = stream.ReadByteOrDefault(4);
            header.UnkA2 = stream.ReadByteOrDefault(5);
            header.UnkA3 = stream.ReadByteOrDefault(6);
            header.UnkA4 = stream.ReadByteOrDefault(7);
            header.HighestFrame = stream.ReadInt32(8);
            header.UnkB1 = stream.ReadByteOrDefault(12);
            header.UnkB2 = stream.ReadByteOrDefault(13);
            header.SpriteSize = stream.ReadInt16(14);
            header.UnkC1 = stream.ReadByteOrDefault(16);
            header.UnkC2 = stream.ReadByteOrDefault(17);
            header.ChannelCount = stream.ReadInt16(18);
            //header.FirstBlockSize = stream.ReadInt16("firstBlockSize");
        }
    }
}
