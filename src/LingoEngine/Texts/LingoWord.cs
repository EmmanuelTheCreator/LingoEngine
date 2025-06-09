using System.Collections;
using System.Data;

namespace LingoEngine.Texts
{
    public class LingoWord : ILingoWord
    {
        private bool _hasParsed = false;
        private string _text = "";
        private LingoWord[] _words = [];

        public ILingoWord this[int index]
        {
            get
            {
                if (!_hasParsed) Parse();
                return _words[index - 1];
            }
        }
        public ILingoChar this[int wordIndex, int charIndex]
        {
            get
            {
                var word = this[wordIndex];
                var ch = new LingoChar();
                ch.SetText((LingoWord)word);
                return ch;
            }
        }


        public int Count => _words.Length;



        public LingoWord(string text)
        {
            SetText(text);
        }

        public override string ToString() => _text;

        internal void SetText(string text)
        {
            _text = text;
            _hasParsed = false;

        }
        private void Parse()
        {
            _words = _text.Split([' '], StringSplitOptions.RemoveEmptyEntries).Select(x => new LingoWord(x)).ToArray();
            _hasParsed = true;
        }

        public ILingoChar GetChar(int wordIndex)
        {
            var word = this[wordIndex];
            var charObj = new LingoChar();
            charObj.SetText((LingoWord)word);
            return charObj;
        }

        public IEnumerator<ILingoWord> GetEnumerator() => _words.AsEnumerable().GetEnumerator();
        public override bool Equals(object? obj) => obj is LingoWord lw && ToString() == lw.ToString();
        public override int GetHashCode() => ToString().GetHashCode();

        IEnumerator<ILingoWord> IEnumerable<ILingoWord>.GetEnumerator() => _words.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public static implicit operator string(LingoWord w) => w.ToString();

        public static implicit operator LingoWord(string value) => new LingoWord(value);
    }
}

 

