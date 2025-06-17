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
    public enum TextAlignment
    {
        /// <summary>Left-aligned text (Lingo: 0)</summary>
        Left = 0,

        /// <summary>Center-aligned text (Lingo: 1)</summary>
        Center = 1,

        /// <summary>Right-aligned text (Lingo: 2)</summary>
        Right = 2
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
        public string HexBefore { get; set; } = string.Empty;
        public string HexAfter { get; set; } = string.Empty;
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
        public int TextLength { get; private set; }
        public ushort FontSize { get; set; }
        public uint LetterSpacing { get; set; }
        public ushort FontId { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public LingoColor ForeColor { get; set; }
        public LingoColor BackColor { get; set; }
        public TextAlignment Alignment { get; set; }
        public bool WordWrap { get; set; }
        public bool Editable { get; set; }
        public bool Scrollable { get; set; }
        public string RtfText { get; private set; }
        public string FontName { get; private set; }

        public static CastMemberTextRead FromXmedChunk(BufferView view, DirectorFile dir)
        {
            var result = new CastMemberTextRead();

            // Dump the raw payload for debugging. The format is still largely
            // unknown so we treat it as opaque ASCII for now.
            var ascii = Encoding.Latin1.GetString(view.Data, view.Offset, view.Size);
            dir.Logger.LogInformation($"XMED all : {BitConverter.ToString(view.Data, view.Offset, view.Size)}");

            result.RtfText = ascii;

            // Extract text runs. The pattern normally looks like
            //  "5,<text>\x03"  or  "1F,<text>\x03" for multi line entries.
            var runRegex = new Regex("\x00(5,|1F,)([^\x03]+)\x03", RegexOptions.Compiled);
            foreach (Match m in runRegex.Matches(ascii))
            {
                var run = new TextRun { Text = m.Groups[2].Value };
                run.Length = run.Text.Length;
                result.TextLength += run.Length;

                int byteIndex = m.Index;
                int beforeStart = Math.Max(0, byteIndex - 8);
                int beforeLen = byteIndex - beforeStart;
                run.HexBefore = BitConverter.ToString(view.Data, view.Offset + beforeStart, beforeLen);

                int afterLen = Math.Min(16, ascii.Length - (m.Index + m.Length));
                run.HexAfter = BitConverter.ToString(view.Data, view.Offset + m.Index + m.Length, afterLen);

                result.Runs.Add(run);
            }

            if (result.Runs.Count > 0)
            {
                // Join runs using new lines to provide a readable Text value
                result.Text = string.Join("\n", result.Runs.ConvertAll(r => r.Text));
                result.TextLength = result.Text.Length;
            }

            // Try to capture font names. They appear after a "40," marker
            // followed by a single control byte and a null terminator.
            var fontMatches = Regex.Matches(ascii, "40,.([A-Za-z0-9 \\*]+)\x00");
            if (fontMatches.Count > 0)
            {
                // The last entry usually corresponds to the active font.
                result.FontName = fontMatches[^1].Groups[1].Value;
            }


            // Alignment and style flags appear around offsets 0x18-0x19
            if (view.Size > 0x19)
            {
                byte styleByte = view.Data[view.Offset + 0x18];
                byte flagsByte = view.Data[view.Offset + 0x19];

                result.Alignment = flagsByte switch
                {
                    0x1A when styleByte == 0xBE => TextAlignment.Left,
                    0x15 when styleByte == 0xD0 => TextAlignment.Right,
                    _ => TextAlignment.Center
                };

                result.Bold = !(styleByte == 0xB8 && flagsByte == 0x17);
                result.Italic = styleByte == 0x30 && flagsByte == 0x1A;
                result.Underline = styleByte == 0x9C && flagsByte == 0x16;
                result.WordWrap = !(styleByte == 0xF4 && flagsByte == 0x19);
            }

            // Font size seems to be stored as a 32-bit little endian value at offset 0x40
            if (view.Size > 0x44)
            {
                result.FontSize = (ushort)BinaryPrimitives.ReadUInt32LittleEndian(view.Data.AsSpan(view.Offset + 0x40, 4));
            }

            // Letter spacing byte at 0x18 changes with the letterSpace_6 variant
            if (view.Size > 0x18)
            {
                result.LetterSpacing = view.Data[view.Offset + 0x18];
            }

            return result;
        }



    }
}
