using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;
using static ProjectorRays.CastMembers.XmedChunkParser;

namespace ProjectorRays.CastMembers
{
    public class XmedChunkParser
    {
        public class TextStyleRun
        {
            public int Start { get; set; }
            public int Length { get; set; }
            public string FontName { get; set; } = "";
            public ushort FontSize { get; set; }
            public bool Bold { get; set; }
            public bool Italic { get; set; }
            public bool Underline { get; set; }
            public string Text { get; set; } = "";
            public ushort FontId { get; set; }
            public LingoColor ForeColor { get; set; }
            public LingoColor BackColor { get; set; }
        }

        public static List<TextStyleRun> Parse(BufferView view)
        {
            var stream = new ReadStream(view, Endianness.LittleEndian);
            var result = new List<TextStyleRun>();

            // Skip header "DEMX" and unknown bytes
            var signature = stream.ReadString(4);
            if (signature != "DEMX")
                throw new InvalidDataException("Invalid XMED chunk header");

            stream.Seek(128); // Skip to styled run zone (heuristic, improves with format knowledge)

            while (!stream.Eof)
            {
                var pos = stream.Pos;
                byte marker = stream.ReadUint8();
                if (marker == 0x35 && stream.PeekChar() == ',') // found "5," which starts "5,Hallo"
                {
                    stream.Seek(pos);
                    string rtfText = stream.ReadCString(); // "5,Hallo"
                    string text = rtfText.Substring(rtfText.IndexOf(',') + 1);

                    var run = new TextStyleRun
                    {
                        Text = text,
                        FontId = 0, // Unknown in this example
                        FontSize = 0,
                        Bold = false,
                        Italic = false,
                        Underline = false,
                        ForeColor = new LingoColor(0, 8, 255),
                        BackColor = new LingoColor(0, 0, 0)
                    };
                    result.Add(run);
                    break;
                }
            }

            return result;
        }
    }
    public class TextRun
    {
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Text { get; set; } = string.Empty;
        public ushort FontId { get; set; }
        public ushort FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public LingoColor ForeColor { get; set; }
        public LingoColor BackColor { get; set; }
    }
    /*
    var hex = BitConverter.ToString(view.Data, view.Offset, Math.Min(64, view.Size));
            dir.Logger.LogInformation($"XMED raw start (64 bytes): {hex}");
    */
    /// <summary>
    /// Extracted field/text style info from an XMED chunk.
    /// </summary>
    public class CastMemberTextRead
    {
        public string Text { get; set; } = string.Empty;
        public List<TextStyleRun> Styles { get; set; } = new();
        public BufferView RtfBytes { get; set; } = BufferView.Empty;
        public List<TextRun> Runs { get; set; } = new();
        public ushort FontSize { get; set; }
        public ushort FontId { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public LingoColor ForeColor { get; set; }
        public LingoColor BackColor { get; set; }
        public short Alignment { get; set; }
        public bool WordWrap { get; set; }
        public bool Editable { get; set; }
        public bool Scrollable { get; set; }
        public string RtfText { get; private set; }
        public string FontName { get; private set; }

        public static CastMemberTextRead FromXmedChunk(BufferView view, DirectorFile dir)
        {
            var result = new CastMemberTextRead();
            var hex = BitConverter.ToString(view.Data, view.Offset, view.Size);
            dir.Logger.LogInformation($"XMED all : {hex}");
            return result;
            // Read as raw ASCII string
            string rawAscii = Encoding.ASCII.GetString(view.Data, view.Offset, view.Size);
            dir.Logger.LogInformation($"XMED raw ASCII: {rawAscii.Substring(0, Math.Min(128, rawAscii.Length))}");

            // Try to find embedded RTF block
            int rtfStart = rawAscii.IndexOf(@"{\rtf", StringComparison.OrdinalIgnoreCase);
            if (rtfStart < 0)
            {
                throw new InvalidOperationException("Could not find RTF start marker in XMED chunk.");
            }

            string rtf = rawAscii.Substring(rtfStart).TrimEnd('\0');
            result.RtfText = rtf;

            // Optional: Extract preview/plain text (basic fallback)
            //result.Text = ExtractPlainText(rtf);

            // Heuristic: Try to extract font name
            var fontMatch = Regex.Match(rawAscii, @"([A-Za-z0-9 \*]+)\?{8,}");
            if (fontMatch.Success)
            {
                result.FontName = fontMatch.Groups[1].Value.Trim();
            }




            //var stream = new ReadStream(view, Endianness.BigEndian);
            //var result = new CastMemberTextRead();

            //dir.Logger.LogInformation($"XMED Begin Parse at stream.Pos={stream.Pos}, Size={stream.Size}");

            //var marker = new byte[] { 0x44, 0x45, 0x4D, 0x58 }; // 'DEMX'
            //int markerOffset = stream.Data.AsSpan().IndexOf(marker);

            //if (markerOffset < 0)
            //    throw new InvalidOperationException("Could not find RTF start marker (DEMX)");

            //dir.Logger.LogInformation($"XMED markerOffset={markerOffset}");

            //// After DEMX: [4 bytes flags] + [4 bytes RTF size] = 8 bytes
            //int rtfLenOffset = markerOffset + 8;

            //if (rtfLenOffset + 4 > stream.Size)
            //    throw new InvalidOperationException("XMED header too short to contain RTF length");

            //uint textLen = BinaryPrimitives.ReadUInt32LittleEndian(
            //    new ReadOnlySpan<byte>(stream.Data, rtfLenOffset, 4)
            //);
            //textLen = 1083;
            //int rtfStart = rtfLenOffset + 4;
            //int remaining = stream.Size - rtfStart;

            //if (textLen > remaining)
            //    throw new InvalidOperationException($"XMED textLen={textLen} too large for remaining stream. rtfStart={rtfStart},remaining={remaining},stream.Size={stream.Size} ");

            //var rtfBytes = new BufferView(stream.Data, rtfStart, (int)textLen);
            //result.Rtf = Encoding.UTF8.GetString(rtfBytes.Data, rtfBytes.Offset, rtfBytes.Size).TrimEnd('\0');

            //// DO NOT parse styles here — it's in another block, not in RTF data.
            //dir.Logger.LogInformation("RTF Preview (clean): " + Regex.Replace(result.Rtf, @"[^\x20-\x7E]", "?"));

            //var stream = new ReadStream(view, Endianness.BigEndian);
            //var result = new CastMemberTextRead();

            //// Skip 8-byte XMED header (XFIR signature, version, etc.)
            //stream.Seek(8);

            //// Skip variable-length header block until RTF text start marker (e.g., ASCII "SRVF", 0x53525646)
            //int markerOffset = stream.Data.AsSpan().IndexOf(new byte[] { 0x53, 0x52, 0x45, 0x56 }); // 'SREV'
            //if (markerOffset < 0)
            //    throw new InvalidOperationException("XMED chunk missing expected text marker.");

            //// Extract full RTF stream starting at marker
            //stream.Seek(markerOffset);
            //int remaining = stream.Size - stream.Pos;
            //var rtfBytes = stream.ReadByteView(remaining);
            ////result.RtfBytes = rtfBytes.Slice(); // or .ToArray() if you prefer

            //// (Optional) Try to extract readable ASCII text as preview/debug
            //string asciiPreview = Regex.Replace(Encoding.ASCII.GetString(rtfBytes.Data, rtfBytes.Offset, rtfBytes.Size), @"[^\x20-\x7E]", "?");
            //dir.Logger.LogInformation($"ASCII preview: {asciiPreview}");
            //result.RtfBytes = rtfBytes;
            //var rtfSpan = new ReadOnlySpan<byte>(rtfBytes.Data, rtfBytes.Offset, rtfBytes.Size);
            //int styleRunCount = 0;
            //for (int i = 0; i <= rtfSpan.Length - 12; i += 12)
            //{
            //    int start = BinaryPrimitives.ReadInt32BigEndian(rtfSpan.Slice(i, 4));
            //    int length = BinaryPrimitives.ReadInt32BigEndian(rtfSpan.Slice(i + 4, 4));
            //    int fontId = BinaryPrimitives.ReadInt32BigEndian(rtfSpan.Slice(i + 8, 4));

            //    if (start < 0 || length <= 0 || start + length > 10000) break;

            //    result.Styles.Add(new TextStyleRun
            //    {
            //        Start = start,
            //        Length = length,
            //        FontName = $"FontID_{fontId}", // Placeholder
            //        FontSize = 12 // Placeholder
            //    });

            //    styleRunCount++;
            //    if (styleRunCount > 100) break;
            //}

            return result;
        }



    }
}