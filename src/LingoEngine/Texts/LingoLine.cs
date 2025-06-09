
using System.Collections;

namespace LingoEngine.Texts
{
    /// <inheritdoc/>
    public class LingoLine : ILingoLine
    {
        private bool _hasParsed = false;
        private string _text = "";
        private string[] _lines = [];
        private readonly Action _textHasChanged;

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
        /// <inheritdoc/>
        public override string ToString() => string.Join(Environment.NewLine, _lines);
        /// <inheritdoc/>
        public int Count
        {
            get
            {
                if (!_hasParsed) Parse();
                return _lines.Length;
            }
        }

        public LingoLine(Action textHasChanged)
        {
            _textHasChanged = textHasChanged;
        }

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
        /// <inheritdoc/>
        public ILingoWord GetWord(int lineIndex)
        {
            var line = this[lineIndex];
            return new LingoWord(line);
        }
        /// <inheritdoc/>
        public ILingoChar GetChar(int lineIndex, int wordIndex)
        {
            var word = GetWord(lineIndex);
            return word.GetChar(wordIndex);
        }
        /// <inheritdoc/>
        public ILingoChar GetChar(int lineIndex, int wordIndex, int charIndex) => GetWord(lineIndex)[wordIndex, charIndex];

        public static implicit operator string(LingoLine line) => line.ToString();
        public static explicit operator LingoLine(string text) => new LingoLine(() => { }).SetAndReturn(text);
        public LingoLine SetAndReturn(string text)
        {
            SetText(text);
            return this;
        }
        public override bool Equals(object? obj) => obj is LingoLine other && ToString() == other.ToString();

        public override int GetHashCode() => ToString().GetHashCode();
        public static bool operator ==(LingoLine a, LingoLine b) => a.Equals(b);
        public static bool operator !=(LingoLine a, LingoLine b) => !a.Equals(b);

        public IEnumerator<string> GetEnumerator()
        {
            if (!_hasParsed) Parse();
            return _lines.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

 

