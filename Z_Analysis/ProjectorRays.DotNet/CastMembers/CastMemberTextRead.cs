using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using System.Buffers.Binary;
using System.Globalization;
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
        public List<string> FontNames { get; } = new();
        public string? MemberName { get; private set; }
        public int SpacingBefore { get; private set; }
        public int SpacingAfter { get; private set; }
        public float LeftMargin { get; private set; }
        public uint? FieldTextLength { get; private set; }

        public static CastMemberTextRead FromXmedChunk(BufferView view, DirectorFile dir)
        {
            var result = new CastMemberTextRead();

            // Todo: Check Field or Text
            if (true)
                ReadText(view, dir);
            else
                ReadField(view, dir);

            
            return result;
        }

        private static void ReadField(BufferView view, DirectorFile dir)
        {

            // try extracting common styles
            // try extracting styles
            // Try extracting Run texts
            // try fiding text <> style mapping
            // try inserting styles in run texts.








            // OLD
            //// Dump the raw payload for debugging. The format is still largely
            //// unknown so we treat it as opaque ASCII for now.
            //var ascii = Encoding.Latin1.GetString(view.Data, view.Offset, view.Size);
            //dir.Logger.LogInformation($"XMED all : {BitConverter.ToString(view.Data, view.Offset, view.Size)}");

            //result.RtfText = ascii;

            //// Field casts use a little-endian length value at offset 0x4C
            //if (view.Size > 0x50)
            //{
            //    var hdr = Encoding.ASCII.GetString(view.Data, view.Offset + 0x4C, 4);
            //    if (hdr != "XFIR")
            //    {
            //        result.FieldTextLength = BinaryPrimitives.ReadUInt32LittleEndian(view.Data.AsSpan(view.Offset + 0x4C, 4));
            //    }
            //}

            //// Parse text runs manually. Runs are encoded as
            ////   0x00 <digits> ',' <text> 0x03
            //// where the digits represent the character count for the run.
            //int pos = 0;
            //while (pos < view.Size - 1)
            //{
            //    if (view.Data[view.Offset + pos] == 0x00)
            //    {
            //        int p = pos + 1;
            //        int lenVal = 0;
            //        while (p < view.Size && view.Data[view.Offset + p] >= (byte)'0' && view.Data[view.Offset + p] <= (byte)'9')
            //        {
            //            lenVal = lenVal * 10 + (view.Data[view.Offset + p] - (byte)'0');
            //            p++;
            //        }
            //        if (p < view.Size && view.Data[view.Offset + p] == (byte)',')
            //        {
            //            p++;
            //            int start = p;
            //            while (p < view.Size && view.Data[view.Offset + p] != 0x03)
            //                p++;
            //            if (p < view.Size)
            //            {
            //                string txt = Encoding.Latin1.GetString(view.Data, view.Offset + start, p - start);
            //                var run = new TextRun
            //                {
            //                    Offset = pos,
            //                    Length = lenVal,
            //                    Text = txt,
            //                    HexBefore = BitConverter.ToString(view.Data, Math.Max(0, view.Offset + pos - 8), Math.Min(8, pos)),
            //                    HexAfter = BitConverter.ToString(view.Data, view.Offset + p + 1, Math.Min(16, view.Size - (p + 1)))
            //                };
            //                result.Runs.Add(run);
            //                result.TextLength += lenVal;
            //                pos = p + 1;
            //                continue;
            //            }
            //        }
            //    }
            //    pos++;
            //}

            //if (result.Runs.Count > 0)
            //{
            //    result.Text = string.Join("\n", result.Runs.ConvertAll(r => r.Text));
            //    result.TextLength = result.Text.Length;
            //}



            //// Try to capture font names. They appear after a "40," marker
            //// followed by a single control byte and a null terminator.
            //var idx = ascii.IndexOf("40,");
            //while (idx >= 0 && idx + 4 < ascii.Length)
            //{
            //    byte len = view.Data[view.Offset + idx + 3];
            //    if (len > 0 && idx + 4 + len <= view.Size)
            //    {
            //        string font = Encoding.Latin1.GetString(view.Data, view.Offset + idx + 4, len);
            //        result.FontNames.Add(font);
            //    }
            //    idx = ascii.IndexOf("40,", idx + 3);
            //}
            //if (result.FontNames.Count > 0)
            //{
            //    result.FontName = result.FontNames[^1];
            //}

            //// Try to detect the color table entry. Two formats have been seen:
            ////  1. an 8-digit hex string before the constant 000600040001
            ////  2. three groups like 01CC00 01FF00 016600 representing "CCFF66".
            //var colorMatch = Regex.Match(ascii, "([A-F0-9]{8})000600040001");
            //if (colorMatch.Success)
            //{
            //    if (uint.TryParse(colorMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out uint rgb))
            //    {
            //        result.ForeColor = new LingoColor(rgb & 0xFFFFFF);
            //    }
            //}
            //else
            //{
            //    var tableMatch = Regex.Match(ascii, "\x01([0-9A-F]{4})\x01([0-9A-F]{4})\x01([0-9A-F]{4})");
            //    if (tableMatch.Success)
            //    {
            //        string col = tableMatch.Groups[1].Value.Substring(0,2) +
            //                     tableMatch.Groups[2].Value.Substring(0,2) +
            //                     tableMatch.Groups[3].Value.Substring(0,2);
            //        if (uint.TryParse(col, System.Globalization.NumberStyles.HexNumber, null, out uint rgb2))
            //            result.ForeColor = new LingoColor(rgb2);
            //    }
            //}

            //// Attempt to detect an embedded member name string. In many casts
            //// the name appears as an ASCII sequence surrounded by null bytes or
            //// prefixed with a length byte.
            //var nameMatch = Regex.Match(ascii, "\x00([A-Za-z ]{3,20})\x00");
            //if (nameMatch.Success)
            //{
            //    string candidate = nameMatch.Groups[1].Value;
            //    if (!candidate.StartsWith("NoTexture") && !candidate.StartsWith("TestData"))
            //        result.MemberName = candidate;
            //}
            //else
            //{
            //    // Fallback for variants like "My field" or "MyText" stored without nulls
            //    var direct = Regex.Match(ascii, "(MyText|My field)");
            //    if (direct.Success)
            //        result.MemberName = direct.Value;
            //}


            //// Alignment and style flags appear around offsets 0x18-0x19
            //if (view.Size > 0x19)
            //{
            //    byte styleByte = view.Data[view.Offset + 0x18];
            //    byte flagsByte = view.Data[view.Offset + 0x19];

            //    result.Alignment = flagsByte switch
            //    {
            //        0x1A when styleByte == 0xBE => TextAlignment.Left,
            //        0x15 when styleByte == 0xD0 => TextAlignment.Right,
            //        _ => TextAlignment.Center
            //    };

            //    result.Bold = !(styleByte == 0xB8 && flagsByte == 0x17);
            //    result.Italic = styleByte == 0x30 && flagsByte == 0x1A;
            //    result.Underline = styleByte == 0x9C && flagsByte == 0x16;
            //    result.WordWrap = !(styleByte == 0xF4 && flagsByte == 0x19);
            //}

            //// Font size seems to be stored as a 32-bit little endian value at offset 0x40
            //if (view.Size > 0x44)
            //{
            //    result.FontSize = (ushort)BinaryPrimitives.ReadUInt32LittleEndian(view.Data.AsSpan(view.Offset + 0x40, 4));
            //}

            //// Letter spacing byte at 0x18 changes with the letterSpace_6 variant
            //if (view.Size > 0x18)
            //{
            //    result.LetterSpacing = view.Data[view.Offset + 0x18];
            //}

            //// Heuristically locate spacing values
            //static int FindBytes(ReadOnlySpan<byte> data, ReadOnlySpan<byte> pattern)
            //{
            //    for (int i = 0; i <= data.Length - pattern.Length; i++)
            //    {
            //        if (data.Slice(i, pattern.Length).SequenceEqual(pattern))
            //            return i;
            //    }
            //    return -1;
            //}

            //int sbIndex = FindBytes(view.Data.AsSpan(view.Offset, view.Size), new byte[] { 0x0D, 0x00, 0x00, 0x00 });
            //if (sbIndex >= 0)
            //    result.SpacingBefore = 13;

            //int saIndex = FindBytes(view.Data.AsSpan(view.Offset, view.Size), new byte[] { 0x09, 0x00, 0x00, 0x00 });
            //if (saIndex >= 0)
            //    result.SpacingAfter = 9;

            //if (view.Size > 0x4DE)
            //    result.LeftMargin = BitConverter.ToSingle(view.Data, view.Offset + 0x4DA);

        }

        private static void ReadText(BufferView view, DirectorFile dir)
        {
            // todo
        }
    }
}
