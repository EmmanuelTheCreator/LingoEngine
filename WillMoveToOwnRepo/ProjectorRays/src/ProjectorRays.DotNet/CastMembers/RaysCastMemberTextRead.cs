using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using System.Buffers.Binary;
using System.Collections.Generic;
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
            public RayColor ForeColor { get; set; }
            public RayColor BackColor { get; set; }
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
                        ForeColor = new RayColor(0, 8, 255),
                        BackColor = new RayColor(0, 0, 0)
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

    public class TextPart
    {
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Text { get; set; } = string.Empty;
        public ushort FontId { get; set; }
        public ushort FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public RayColor ForeColor { get; set; }
        public RayColor BackColor { get; set; }
        public string HexBefore { get; set; } = string.Empty;
        public string HexAfter { get; set; } = string.Empty;
        public uint LetterSpacing { get; set; }
        public TextAlignment Alignment { get; set; }
        public int SpacingBefore { get; private set; }
        public int SpacingAfter { get; private set; }
        public float LeftMargin { get; private set; }
    }

  
    /*
    var hex = BitConverter.ToString(view.Data, view.Offset, Math.Min(64, view.Size));
            dir.Logger.LogInformation($"XMED raw start (64 bytes): {hex}");
    */
    /// <summary>
    /// Extracted field/text style info from an XMED chunk.
    /// </summary>
    public class RaysCastMemberTextRead
    {
        private List<StyleEntry> StyleEntries { get; } = new();
        private BufferView RtfBytes { get; set; } = BufferView.Empty;

        public string Text { get; set; } = string.Empty;
        public List<TextStyleRun> Styles { get; set; } = new();
        public List<TextPart> TextParts { get; set; } = new();
        public int TextLength { get; private set; }
        public bool WordWrap { get; set; }
        public bool Editable { get; set; }
        public bool Scrollable { get; set; }
        public string RtfText { get; private set; }
        public Dictionary<int, string> StyleFonts { get; } = new();
        
        public string? MemberName { get; private set; }
       
        public uint? FieldTextLength { get; private set; }

        public static RaysCastMemberTextRead FromXmedChunk(BufferView view, RaysDirectorFile dir)
        {
            var result = new RaysCastMemberTextRead();

            // Dump the raw payload for debugging. The format is still largely
            // unknown so we treat it as opaque ASCII for now.
            var ascii = Encoding.Latin1.GetString(view.Data, view.Offset, view.Size);
            dir.Logger.LogInformation($"XMED all : {BitConverter.ToString(view.Data, view.Offset, view.Size)}");



            return result;
        }


        internal class StyleEntry
        {
            public int Offset { get; set; }
            public int F1 { get; set; }
            public int F2 { get; set; }
            public int F3 { get; set; }
            public int F4 { get; set; }
            public int StyleId { get; set; }
        }
    }
}
