using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;
using System.Buffers.Binary;
using System.Data;

namespace BlingoEngine.IO.Legacy.Movies
{
    internal class BlMovieRawInfo
    {
        public string AboutText { get; set; } = "";
        public string CopyRightText { get; set; } = "";
        public BlMovieRenderer Renderer { get; set; }
        public int PaletteIndex { get; set; }
        public bool PaletteByIndex { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string MoviePath { get; internal set; } = "";
        public string CreatedBy { get; internal set; } = "";
        public string ModifiedBy { get; internal set; } = "";
        public BlingoColorDTO BackgroundColor { get; internal set; }
        public int Top { get; internal set; }
        public int Left { get; internal set; }

        public enum BlMovieRenderer
        {
            Auto = 0,
            OpenGl = 1,
            DirectX5_2 = 2,
            DirectX7 = 3,
            Software = 4,
        }
    }
    internal class BlMovieVWFIReader
    {
        private ReaderContext _context;

        public BlMovieVWFIReader(ReaderContext context)
        {
            _context = context;
        }

        internal BlMovieRawInfo Read()
        {
            var payload = ReadVWFI();
            if (payload == null)
                return new BlMovieRawInfo();

            var data = ParseVWFI(payload);

            var drcfData = ReadDRCF();
            if (drcfData != null) ParseDRCF(data, drcfData);

            var publData = ReadPUBL();
            if (publData != null) ParsePUBL(data, publData);


            return data;
        }

       

        public byte[]? ReadVWFI() => BlClassicPayloadLoader.ReadResource(_context, BlTag.VWFI);
        public byte[]? ReadDRCF() => BlClassicPayloadLoader.ReadResource(_context, BlTag.DRCF);
        public byte[]? ReadPUBL() => BlClassicPayloadLoader.ReadResource(_context, BlTag.PUBL);
       

        public BlMovieRawInfo ParseVWFI(byte[] payload)
        {
            var info = new BlMovieRawInfo();
            var reader = new BlStreamReader(new MemoryStream(payload));
            var numberOfOffsets = reader.ReadInt32();
            var something = reader.ReadInt32();
            var something1 = reader.ReadInt32();
            var something2 = reader.ReadInt16();
            var something3 = reader.ReadByte();
            var something4 = reader.ReadByte();
            var something5 = reader.ReadInt16();
            var offsets = new List<int>();  
            for (int i = 0; i < 12; i++)
                offsets.Add(reader.ReadInt32());
            var datas = new List<byte[]>();
            var last = 0;
            foreach (var offset in offsets)
            {
                if (offset == 0)
                {
                    datas.Add([]);
                    continue;
                }    
                var length = offset - last;
                var data = reader.ReadBytes(length);
                datas.Add(data);
                last += length;
            }

            info.CreatedBy = datas[0].ReadStringWithFirstByteLength();
            info.ModifiedBy = datas[4].ReadStringWithFirstByteLength();
            info.MoviePath = datas[5].ReadStringWithFirstByteLength();
            info.CopyRightText = datas[9].ReadStringWithFirstByteLength();
            info.AboutText = datas[10].ReadStringWithFirstByteLength();

            reader.BaseStream.Dispose();
            return info;
        }

        private void ParseDRCF(BlMovieRawInfo data, byte[] payload)
        {
            var reader = new BlStreamReader(new MemoryStream(payload));
            try
            {
                var values = new List<int>();
                for (int i = 0; i < 15; i++)
                    values.Add(reader.ReadInt16());

                data.Top = values[2];
                data.Left = values[3];
                var bottom = values[4];
                var right = values[5];
                data.Width = right - data.Left;
                data.Height = bottom - data.Top;

                var unknown2 = reader.ReadByte();       // FD
                var unknown3 = reader.ReadByte();       // 00
                var values2 = new List<int>();
                for (int i = 0; i < 13; i++)
                    values2.Add(reader.ReadInt16());
                data.Renderer = (BlMovieRawInfo.BlMovieRenderer)values2[5];

                var palleteIndex = reader.ReadByte();   // 0C
                var palleteFlag = reader.ReadByte();    // 3C
                var unknown4 = reader.ReadInt16();      // 00 00
                var unknown5 = reader.ReadInt16();      // 00 3C
               
                var unknownA = reader.ReadUInt16();      // 78 73   
                var unknownB = reader.ReadUInt16();      // 49 3E
                var unknown6 = reader.ReadInt32();      // 00 00 00 00
                var unknown7 = reader.ReadInt32();      // 00 00 00 00
                var colorPerhaps = reader.ReadInt32();      // FF FF FF 9B

                var flag1 = reader.ReadByte();          // 01
                var flag2 = reader.ReadByte();          // 00
                var flag3 = reader.ReadByte();          // 00
                var flag4 = reader.ReadByte();          // 01
                var unknownC1 = reader.ReadInt16();     // 00 00
                var unknownC2 = reader.ReadInt16();     // 01 7A
                var unknownC3 = reader.ReadInt32();     // 00 00 00 00
                var unknownC3B = reader.ReadInt32();    // 00 00 00 00
                var unknownC3C = reader.ReadInt32();    // 00 00 00 00
            }
            catch (Exception)
            {


            }
            finally
            {
                reader.BaseStream.Dispose();
            }
        }
        private void ParsePUBL(BlMovieRawInfo data, byte[] payload)
        {
            var reader = new BlStreamReader(new MemoryStream(payload));
            try
            {
                var startVal1 = reader.ReadInt32();      // 00 00 00 0B
                var startVal2 = reader.ReadInt32();      // 00 00 00 02 
                var startByte1 = reader.ReadByte();      // D0
                var startByte2 = reader.ReadByte();      // 00
                var startVal3 = reader.ReadInt16();      // 00 01
                var startVal4 = reader.ReadByte();      // E0 
                var bgColorR = reader.ReadByte();      // FF
                var bgColorG = reader.ReadByte();      // FF
                var bgColorB = reader.ReadByte();      // FF
                data.BackgroundColor = new BlingoColorDTO(bgColorR, bgColorG, bgColorB);
                string htmlPageName = ReadTextBytes(reader);
                string htmlCopyrightPageName = ReadTextBytes(reader);
                string dcrName = ReadTextBytes(reader);
                string jpgImagageName = ReadTextBytes(reader);
                string className = ReadTextBytes(reader);
                string publishName = ReadTextBytes(reader);
                var unknown0 = reader.ReadInt32();      // 00 00 00 00
                var unknown1 = reader.ReadInt32();      // FF FF FF 00
                //var pageColorR = reader.ReadByte();      // FF
                //var pageColorG = reader.ReadByte();      // FF
                //var pageColorB = reader.ReadByte();      // FF
                //var pageBgColor = new BlingoColorDTO(pageColorR, pageColorG, pageColorB);
                var flag1 = reader.ReadInt16();          // 00 00
                var flag2 = reader.ReadByte();           // 01
                var flag3 = reader.ReadByte();           // 50
                var flag4 = reader.ReadByte();           // 01
                var flag5 = reader.ReadByte();           // 00
                var flag6 = reader.ReadByte();           // 00
                var flag7 = reader.ReadByte();           // 01
                var flag8 = reader.ReadByte();           // 01
                var flag9 = reader.ReadByte();           // 00
                var flag10 = reader.ReadByte();          // 01
                string swContextMenu = ReadTextBytes(reader); // swContextMenu
                var valC1 = reader.ReadByte();           // 01
                var valC2 = reader.ReadInt32();          // 00 00 00 01
                var valC3 = reader.ReadInt16();          // 00 00
                var valC4 = reader.ReadByte();           // 01
                var valC5 = reader.ReadInt16();          // 00 01
                var valC7 = reader.ReadInt32();          // 00 00 00 00
                var valC8 = reader.ReadInt32();          // 00 00 00 00
                string exeFileName = reader.ReadStringWithFirstByteLength();
                reader.Skip(3);
                string ocxFileName = reader.ReadStringWithFirstByteLength();
                reader.Skip(3);
                string classicFileName5 = reader.ReadStringWithFirstByteLength();
                reader.Skip(3);
                string standard = reader.ReadStringWithFirstByteLength();

            }
            catch (Exception)
            {
            }
            finally
            {
                reader.BaseStream.Dispose();
            }
        }

        private static string ReadTextBytes(BlStreamReader reader)
        {
            var nextWordLetterCount = reader.ReadByte();      // 0C

            var word1 = new List<byte>();
            for (int i = 0; i < nextWordLetterCount; i++)
                word1.Add((byte)reader.ReadInt32());

            var text = word1.ToArray().ReadCString(0);
            return text;
        }
    }
}
