using System.Collections;
using System.Data;

namespace LingoEngine.Texts
{
    /// <summary>
    /// Represents a parsed 1-based word accessor with optional character access for each word.
    /// </summary>
    public class LingoWords : IEnumerable<LingoWords> , IEnumerable
    {
        private bool _hasParsed = false;
        private string _text = "";
        private LingoWords[] _words = [];

        /// <summary>
        /// Gets the word at the specified 1-based index.
        /// </summary>
        public LingoWords this[int index]
        {
            get
            {
                if (!_hasParsed) Parse();
                return _words[index - 1];
            }
        }

        /// <summary>
        /// Gets the character at the specified word and character index (1-based).
        /// </summary>
        public LingoChars this[int wordIndex, int charIndex]
        {
            get
            {
                var word = this[wordIndex];
                var ch = new LingoChars();
                ch.SetText(word);
                return ch;
            }
        }

        /// <summary>
        /// Gets the number of parsed words.
        /// </summary>
        public int Count => _words.Length;

        public LingoWords(string text) => SetText(text);
        public override string ToString() => _text;

        internal void SetText(string text)
        {
            _text = text.Replace("\r"," ").Replace("\n", " ");
            _hasParsed = false;
        }

        private void Parse()
        {
            _words = _text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => new LingoWords(x)).ToArray();
            _hasParsed = true;
        }

        /// <summary>
        /// Gets the character accessor for a given word index.
        /// </summary>
        public LingoChars GetChar(int wordIndex)
        {
            var word = this[wordIndex];
            var charObj = new LingoChars();
            charObj.SetText(word);
            return charObj;
        }

        public IEnumerator<LingoWords> GetEnumerator() => _words.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator string(LingoWords w) => w.ToString();
        public static implicit operator LingoWords(string value) => new LingoWords(value);

        public override bool Equals(object? obj) => obj is LingoWords lw && ToString() == lw.ToString();
        public override int GetHashCode() => ToString().GetHashCode();
    }

}



