using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Scores
{
    
    internal partial class BlLegacyScoreReader
    {
        private readonly ReaderContext _context;

        private ScoreRawHeader Header { get; set; }
        public List<BlLegacyScoreTokenizer.BlScoreRawFrame> Tokens { get; private set; }
        private List<SpriteRawData> Sprites { get; set; }

        public BlLegacyScoreReader(ReaderContext context)
        {
            _context = context;
        }
        public void Read()
        {
            var payload = ReadVMSW();
            if (payload == null)
                return;

            ParseVMSC(payload);
        }

        public byte[]? ReadVMSW()
        {
            var classicLoader = new BlClassicPayloadLoader(_context);
            BlAfterburnerPayloadLoader? afterburnerLoader = null;
            if (_context.AfterburnerState is not null)
                afterburnerLoader = new BlAfterburnerPayloadLoader(_context, _context.AfterburnerState);
            var vwsc = _context.Resources.Entries.FirstOrDefault(e => e.Tag == BlTag.VWSC);
            if (vwsc == null) return null;

            var payload = LoadPayload(vwsc, classicLoader, afterburnerLoader);
            return payload;
        }


        private static byte[] LoadPayload(BlLegacyResourceEntry entry, BlClassicPayloadLoader classicLoader, BlAfterburnerPayloadLoader? afterburnerLoader)
        {
            return entry.StorageKind == BlResourceStorageKind.AfterburnerSegment
                ? afterburnerLoader is null ? Array.Empty<byte>() : entry.LoadAfterburner(afterburnerLoader)
                : entry.ReadClassicPayload(classicLoader);
        }

        public void ParseVMSC(byte[] payload)
        {
            var tokenizer = new BlLegacyScoreTokenizer(_context);
            var reader = new BlStreamReader(new MemoryStream(payload));
            var header = ReadMainHeader(reader);
            var datas = ReadDataRangesFromOffsets(reader, header.EntryCount);
            ReadHeader(datas[0], header);
            Header = header;
            var spriteOffsets = ReadSpriteOffsets(datas[1]);
            Sprites = ReadSprites(datas, spriteOffsets);
            Tokens = tokenizer.Tokenize(datas[0].Skip(5 * 4).ToArray());
            reader.BaseStream.Dispose();
        }
        public string ToLog()
        { 
            var logSprites = string.Join(Environment.NewLine, Sprites.Select(s => s.ToLog()));
            var tokenizer = new BlLegacyScoreTokenizer(_context);
            var logFrames = tokenizer.ToLog(Tokens);
            var logHeader = Header.ToLog();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Header ===");
            sb.AppendLine(logHeader);
            sb.AppendLine();
            sb.AppendLine("=== Sprites ===");
            sb.AppendLine(logSprites);
            sb.AppendLine();
            sb.AppendLine("=== Frames ===");
            sb.AppendLine(logFrames);
            sb.AppendLine();
            var fullLog = sb.ToString();
            return fullLog;
        }

       
        private List<byte[]> ReadDataRangesFromOffsets(BlStreamReader reader, int entryCount)
        {
            
            var offsets = new List<int>(entryCount);
            for (int i = 0; i < entryCount; i++)
                offsets.Add(reader.ReadInt32());

            var startPos = reader.Position + 4;

            var datas = new List<byte[]>(entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                var start = offsets[i];
                var end = (i + 1 < entryCount) ? offsets[i + 1] : (int)reader.BaseStream.Length;
                var length = end - start;
                if (length == 0)
                {
                    datas.Add([]);
                    continue;
                }
                reader.Position = start + startPos;
                if (reader.Position + length > reader.Length)
                {
                    continue;
                }
                var data = reader.ReadBytes(length);
                datas.Add(data);
            }
            return datas;
        }
        private List<(int SpriteData, int MemberBehaviorData)> ReadSpriteOffsets(byte[] orderingData)
        {
            var returnData = new List<(int, int)>();
            if (orderingData.Length < 2) return returnData;
            var spriteCount = orderingData.ReadInt32(0);
            for (int i = 0; i < spriteCount; i++)
            {
                var offset = orderingData.ReadInt32(i * 4 + 4);
                returnData.Add((offset, offset +1));
            }
            return returnData;
        }

        private List<SpriteRawData> ReadSprites(List<byte[]> datas, List<(int SpriteData, int MemberBehaviorData)> spriteOffsets)
        {
            var returnData = new List<SpriteRawData>();
            var index = 0;
            foreach (var offset in spriteOffsets)
            {
                var data = datas[offset.SpriteData];
                var memberBehaviorData = datas[offset.MemberBehaviorData];
                var spriteData = new SpriteRawData(data, memberBehaviorData, index);
                returnData.Add(spriteData);
                index++;
            }
            return returnData;
        }
    }
}
