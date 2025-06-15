using System.Text;
using Director.Fonts;
using Director.IO;

namespace Director.Tools
{
    public class Stxt
    {
        private readonly Cast _cast;

        private readonly int _size;
        private FontStyle _style = new();

        private string _rtext = string.Empty; // raw
        private string _ftext = string.Empty; // formatted
        private string _ptext = string.Empty; // printable

        /// <summary>
        /// Gets the style information extracted from the STXT chunk.
        /// </summary>
        public FontStyle Style => _style;

        /// <summary>
        /// Gets the raw text data as stored in the STXT resource.
        /// </summary>
        public string RawText => _rtext;

        /// <summary>
        /// Gets the formatted text including style escape sequences.
        /// </summary>
        public string FormattedText => _ftext;

        /// <summary>
        /// Gets the printable UTF-8 representation of the text.
        /// </summary>
        public string PrintableText => _ptext;

        public Stxt(Cast cast, SeekableReadStreamEndian textStream)
        {
            _cast = cast;

            _textType = 0; // kTextTypeFixed (placeholder)
            _textAlign = 0; // kTextAlignLeft (placeholder)
            _textShadow = 0;
            _unk1f = _unk2f = _unk3f = 0;
            _size = (int)textStream.Length;

            if (_size == 0)
                return;

            uint offset = textStream.ReadUInt32();
            if (offset != 12)
                throw new InvalidDataException("Stxt init: unhandled offset");

            uint strLen = textStream.ReadUInt32BE();
            uint dataLen = textStream.ReadUInt32BE();
            string text = textStream.ReadString((int)strLen);

            ushort formattingCount = textStream.ReadUInt16BE();
            int prevPos = 0;

            while (formattingCount > 0)
            {
                ushort currentFont = _style.FontId;
                _style.Read(textStream, _cast);

                if (prevPos > _style.FormatStartOffset)
                    throw new InvalidDataException("Style offset out of order");

                string textPart = string.Empty;
                while (prevPos != _style.FormatStartOffset && text.Length > 0)
                {
                    char f = text[0];
                    textPart += f;
                    text = text.Substring(1);
                    if (f == '\x01')
                        _ftext += '\x01';
                    prevPos++;
                }

                _rtext += textPart;
                var encoding = FontEncodingHelper.DetectFontEncoding(cast.Platform, currentFont);
                string u32TextPart = encoding.GetString(Encoding.Default.GetBytes(textPart));
                _ptext += u32TextPart;
                _ftext += u32TextPart;

                string format = $"\x01\x0e{_style.FontId:X4}{_style.TextSlant:X2}{_style.FontSize:X4}{_style.R:X4}{_style.G:X4}{_style.B:X4}";
                _ftext += format;

                formattingCount--;
            }

            _rtext += text;
            {
                var encoding = FontEncodingHelper.DetectFontEncoding(cast.Platform, _style.FontId);
                string u32Text = encoding.GetString(Encoding.Default.GetBytes(text));
                _ptext += u32Text;
                _ftext += u32Text;
            }
        }

        private int _textType;
        private int _textAlign;
        private int _textShadow;
        private int _unk1f;
        private int _unk2f;
        private int _unk3f;
    }
}
