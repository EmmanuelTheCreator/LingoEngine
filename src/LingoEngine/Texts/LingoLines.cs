
using System.Collections;

namespace LingoEngine.Texts
{
    /// <summary>
    /// Represents a parsed, 1-based line accessor from a text block, and provides access to words and characters per line.
    /// </summary>
    public class LingoLines : IEnumerable<string> , IEnumerable
    {
        private bool _hasParsed = false;
        private string _text = "";
        private string[] _lines = [];
        private readonly Action _textHasChanged;

        /// <summary>
        /// Gets or sets a line by 1-based index.
        /// </summary>
        public string this[int index]
        {
            get
            {
                if (!_hasParsed) Parse();
                return _lines[index - 1];
            }
            set
            {
                _lines[index - 1] = value;
                _text = string.Join(Environment.NewLine, _lines);
                Parse();
                _textHasChanged();
            }
        }

        /// <summary>
        /// Returns the entire text block.
        /// </summary>
        public override string ToString() => _text;

        /// <summary>
        /// Gets the number of lines.
        /// </summary>
        public int Count
        {
            get
            {
                if (!_hasParsed) Parse();
                return _lines.Length;
            }
        }

        public LingoLines(Action textHasChanged) => _textHasChanged = textHasChanged;

        internal void SetText(string text)
        {
            _text = text;
            _hasParsed = false;
        }

        private void Parse()
        {
            _lines = _text.Split(["\r\n", "\n"], StringSplitOptions.None);
            _hasParsed = true;
        }

        /// <summary>
        /// Gets the word object for a specific line.
        /// </summary>
        public LingoWords GetWord(int lineIndex) => new(this[lineIndex]);

        /// <summary>
        /// Gets the character accessor from a specific line and word index.
        /// </summary>
        public LingoChars GetChar(int lineIndex, int wordIndex) => GetWord(lineIndex).GetChar(wordIndex);

        /// <summary>
        /// Gets the specific character from a specific line, word, and character index.
        /// </summary>
        public LingoChars GetChar(int lineIndex, int wordIndex, int charIndex) => GetWord(lineIndex)[wordIndex, charIndex];

        public static implicit operator string(LingoLines line) => line.ToString();
        public static explicit operator LingoLines(string text) => new LingoLines(() => { }).SetAndReturn(text);

        public LingoLines SetAndReturn(string text)
        {
            SetText(text);
            return this;
        }

        public override bool Equals(object? obj) => obj is LingoLines other && ToString() == other.ToString();
        public override int GetHashCode() => ToString().GetHashCode();
        public static bool operator ==(LingoLines a, LingoLines b) => a.Equals(b);
        public static bool operator !=(LingoLines a, LingoLines b) => !a.Equals(b);

        public IEnumerator<string> GetEnumerator()
        {
            if (!_hasParsed) Parse();
            return _lines.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}



