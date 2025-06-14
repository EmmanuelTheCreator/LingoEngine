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
            var reader = stream;

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

            if (_cast.FontMap.TryGetValue(FontId, out var mapEntry))
            {
                FontId = mapEntry.ToFont;
                FontSize = mapEntry.SizeMap.GetValueOrDefault(FontSize, FontSize);
                if (mapEntry.RemapChars)
                    Text = RemapText(Text, _cast.MacCharsToWin);
            }
        }

        public TextCastMember(Cast cast, int castId, TextCastMember source)
            : base(cast, castId, CastType.Text)
        {
            source.Load();
            _loaded = true;
            _initialRect = source._initialRect;
            _boundingRect = source._boundingRect;
            if (cast == source._cast)
                _children = source._children;

            Text = source.Text;
            FontName = source.FontName;
            FontSize = source.FontSize;
            FontId = source.FontId;
            Bold = source.Bold;
            Italic = source.Italic;
            Underline = source.Underline;
            ForeColor = source.ForeColor;
            BackColor = source.BackColor;
            Alignment = source.Alignment;
            WordWrap = source.WordWrap;
            Editable = source.Editable;
            Scrollable = source.Scrollable;
        }


        private string RemapText(string input, byte[] charMap)
        {
            var result = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                byte c = (byte)input[i];
                result[i] = (char)charMap[c]; // array lookup is safe
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

        public override CastMember Duplicate(Cast cast, int castId)
        {
            return new TextCastMember(cast, castId, this);
        }

        public override string FormatInfo()
        {
            return $"FontID:{FontId} Size:{FontSize} Style:{(Bold ? "B" : "")}{(Italic ? "I" : "")}{(Underline ? "U" : "")}";
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
            switch (rte)
            {
                case Director.Primitives.RTE1 r:
                    Text = System.Text.Encoding.ASCII.GetString(r.RawData);
                    break;
                case byte[] bytes:
                    Text = System.Text.Encoding.ASCII.GetString(bytes);
                    break;
                case string str:
                    Text = str;
                    break;
            }
        }
        public void ImportStxt(object stxt)
        {
            if (stxt is Director.Tools.Stxt s)
            {
                Text = s.PrintableText;
                FontId = s.Style.FontId;
                FontSize = s.Style.FontSize;
                Bold = (s.Style.TextSlant & 0x01) != 0;
                Italic = (s.Style.TextSlant & 0x02) != 0;
                Underline = (s.Style.TextSlant & 0x04) != 0;
                ForeColor = new LingoColor((byte)(s.Style.R >> 8), (byte)(s.Style.G >> 8), (byte)(s.Style.B >> 8));
            }
        }

        public void CreateWidget()
        {
            // Widgets are not currently supported in this C# version.
        }
        public void CreateWindowOrWidget()
        {
            // Widgets are not currently supported in this C# version.
        }
        public void UpdateFromWidget()
        {
            // Placeholder for widget sync logic
        }
        public override void Unload()
        {
            Text = string.Empty;
            _loaded = false;
        }
        public object GetWidget()
        {
            // Widgets are not currently supported in this C# version.
            return null!;
        }
        public bool IsWithin(int x, int y)
        {
            return x >= _boundingRect.Left && x <= _boundingRect.Right &&
                   y >= _boundingRect.Top && y <= _boundingRect.Bottom;
        }
        
    }

}
