using Director.Primitives;
using Director.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Director.IO
{  public class CstChunk
        {
            public string Tag { get; set; } = string.Empty;
            public uint Length { get; set; }
            public byte[] Data { get; set; } = Array.Empty<byte>();
        }

    public class CstFileReader
    {
        private readonly string _filePath;

        public CstFileReader(string filePath)
        {
            _filePath = filePath;
        }

        public void ReadChunks()
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fs);

            while (fs.Position < fs.Length)
            {
                if (fs.Length - fs.Position < 8)
                    break; // Not enough for a full chunk header

                // Read tag (4 chars)
                var tagBytes = reader.ReadBytes(4);
                string tag = Encoding.ASCII.GetString(tagBytes);

                // Read length (4 bytes)
                uint length = reader.ReadUInt32();

                // Safety check
                if (length > fs.Length - fs.Position)
                    break;

                byte[] data = reader.ReadBytes((int)length);

                Console.WriteLine($"Chunk: {tag}, Size: {length}");
                HandleChunk(tag, data);
                // Optional: Dump partial hexdump or full data
            }
        }
        private void HandleChunk(string tag, byte[] data)
        {
            //switch (tag)
            //{
            //    case var t when t == ResourceTags.MKTAG('X', 'F', 'I', 'R'):
            //        HandleXFIR(reader, chunkLength);
            //        break;
            //    case var t when t == ResourceTags.MKTAG('f', 'c', 'R', 'D'):
            //        HandleFCRD(reader, chunkLength);
            //        break;
            //    case var t when t == ResourceTags.MKTAG('e', 'e', 'r', 'f'):
            //        HandleEERF(reader, chunkLength);
            //        break;
            //    case var t when t == ResourceTags.MKTAG('k', 'n', 'u', 'j'):
            //        HandleKNUJ(reader, chunkLength);
            //        break;
            //    case var t when t == ResourceTags.MKTAG('p', 'a', 'm', 'm'):
            //        HandlePAMM(reader, chunkLength);
            //        break;
            //    default:
            //        LogHelper.DebugLog(1, DebugChannel.Loading, $"Unknown CST chunk: {ResourceTags.FromTag(tag)} ({tag:X8})");
            //        reader.BaseStream.Seek(chunkLength, SeekOrigin.Current);
            //        break;
            //}
        }
    }


}
