using Director.IO;
using Director.Primitives;

namespace Director.Members
{

    public class TextCastMember : CastMember
    {
        public string Text { get; private set; } = string.Empty;
        public string FontName { get; private set; } = string.Empty;
        public ushort FontSize { get; private set; }
        public ushort FontId { get; private set; }
        public bool Bold { get; private set; }
        public bool Italic { get; private set; }
        public bool Underline { get; private set; }
        public LingoColor ForeColor { get; private set; }
        public LingoColor BackColor { get; private set; }
        public short Alignment { get; private set; }
        public bool WordWrap { get; private set; }
        public bool Editable { get; private set; }
        public bool Scrollable { get; private set; }

        public TextCastMember(Cast parent, int id)
            : base(parent, id, CastType.Text) { }

        public TextCastMember(Cast cast, int castId, SeekableReadStreamEndian stream)
    : base(cast, castId, stream)
        {
            _type = CastType.Text;

            using var reader = new BinaryReader(stream);

            ushort textLen = reader.ReadUInt16BE();
            Text = reader.ReadString(textLen);

            FontId = reader.ReadUInt16BE();
            FontSize = reader.ReadUInt16BE();
            var styleFlags = reader.ReadUInt16BE();

            Bold = (styleFlags & 0x0001) != 0;
            Italic = (styleFlags & 0x0002) != 0;
            Underline = (styleFlags & 0x0004) != 0;

            ForeColor = new LingoColor(reader.ReadUInt16BE());
            BackColor = new LingoColor(reader.ReadUInt16BE());

            Alignment = reader.ReadInt16BE();
            WordWrap = reader.ReadUInt16BE() != 0;
            Editable = reader.ReadUInt16BE() != 0;
            Scrollable = reader.ReadUInt16BE() != 0;

            if (_cast._fontMap.TryGetValue(FontId, out var mapEntry))
            {
                FontId = mapEntry.ToFont;
                FontSize = mapEntry.SizeMap.GetValueOrDefault(FontSize, FontSize);
                if (mapEntry.RemapChars)
                    Text = RemapText(Text, _cast._macCharsToWin);
            }
        }


        private string RemapText(string input, Dictionary<byte, byte> charMap)
        {
            var result = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                byte c = (byte)input[i];
                result[i] = (char)(charMap.TryGetValue(c, out var mapped) ? mapped : c);
            }
            return new string(result);
        }

        public override void Dispose()
        {
            base.Dispose();
            Text = string.Empty;
        }

        public string GetText() => Text;
        public string GetRawText() => Text;
        public void SetRawText(string text) => Text = text;

        public short GetAlignment() => Alignment;
        public LingoColor GetForeColor() => ForeColor;
        public LingoColor GetBackColor() => BackColor;
        public ushort GetTextFont() => FontId;
        public ushort GetTextSize() => FontSize;
        public (bool Bold, bool Italic, bool Underline) GetTextStyle() => (Bold, Italic, Underline);

        public void SetForeColor(LingoColor color) => ForeColor = color;
        public void SetBackColor(LingoColor color) => BackColor = color;
        public void SetColors(LingoColor fore, LingoColor back)
        {
            ForeColor = fore;
            BackColor = back;
        }
        public void SetTextFont(ushort fontId)
        {
            FontId = fontId;
            FontName = ""; // optionally reset font name
        }
        public void SetTextSize(ushort size) => FontSize = size;
        public void SetTextStyle(bool bold, bool italic, bool underline)
        {
            Bold = bold;
            Italic = italic;
            Underline = underline;
        }

        public int GetLineCount()
        {
            return string.IsNullOrEmpty(Text) ? 0 : Text.Split('\n').Length;
        }

        public int GetLineHeight()
        {
            return FontSize + 2; // simple heuristic
        }

        public int GetTextHeight()
        {
            return GetLineCount() * GetLineHeight();
        }

        public string FormatInfo()
        {
            return $"FontID: {FontId}, Size: {FontSize}, Style: {(Bold ? "B" : "")}{(Italic ? "I" : "")}{(Underline ? "U" : "")}";
        }

        // Placeholder implementations for less-used parts
        public bool HasField(string fieldName) => false;
        public bool HasChunkField(string fieldName) => false;
        public string GetField(string name) => "";
        public void SetField(string name, string value) { }
        public string GetChunkField(string name) => "";
        public void SetChunkField(string name, string value) { }

        public void ImportRTE(object rte)
        {
            // TODO: Parse RTE data and extract text + formatting
            // Placeholder until RTE structure is defined
            throw new NotImplementedException("ImportRTE is not yet implemented");
        }
        public void ImportStxt(object stxt)
        {
            // TODO: Parse Stxt and load into text member
            throw new NotImplementedException("ImportStxt is not yet implemented");
        }

        public void CreateWidget()
        {
            // TODO: Create platform-specific widget representation
            throw new NotImplementedException("CreateWidget not implemented");
        }
        public void CreateWindowOrWidget()
        {
            // TODO: Initialize window or UI element for editing/display
            throw new NotImplementedException("CreateWindowOrWidget not implemented");
        }
        public void UpdateFromWidget()
        {
            // TODO: Update internal text based on widget state
            throw new NotImplementedException("UpdateFromWidget not implemented");
        }
        public void Unload()
        {
            // TODO: Dispose widget and release resources
            throw new NotImplementedException("Unload not implemented");
        }
        public object GetWidget()
        {
            // TODO: Return current widget instance if any
            throw new NotImplementedException("GetWidget not implemented");
        }
        public bool IsWithin(int x, int y)
        {
            // TODO: Determine if point is inside visual bounds
            throw new NotImplementedException("IsWithin not implemented");
        }
        public void Load() => LoadFromStream(Stream.Null, new Resource());
    }

}
