using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

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

        public enum BlMovieRenderer
        {
            Auto,
            OpenGl,
            DirectX5_2,
            DirectX7,
            Software
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
            if (drcfData != null)
                ParseDRCF(data, drcfData);
            return data;
        }

       

        public byte[]? ReadVWFI() => BlClassicPayloadLoader.ReadResource(_context, BlTag.VWFI);
        public byte[]? ReadDRCF() => BlClassicPayloadLoader.ReadResource(_context, BlTag.DRCF);
       

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


            reader.BaseStream.Dispose();
        }
    }
}
