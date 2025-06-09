using System.Collections;

namespace LingoEngine.Texts
{
    public class LingoChar : ILingoChar
    {
        private string _word = "";
        /// <inheritdoc/>
        public char this[int index]
        {
            get
            {
                return _word[index - 1];
            }
        }
        /// <inheritdoc/>
        public override string ToString() => _word;
        /// <inheritdoc/>
        public int Count => _word.Length;
        internal void SetText(string word)
        {
            _word = word;
        }
        /// <inheritdoc/>
        public IEnumerator<char> GetEnumerator() => _word.GetEnumerator();
        public static implicit operator string(LingoChar lingoChar) => lingoChar.ToString();
        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is LingoChar lc && _word == lc._word;
        /// <inheritdoc/>
        public override int GetHashCode() => _word.GetHashCode();
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _word.GetEnumerator();

        public static bool operator ==(LingoChar a, LingoChar b) => a._word == b._word;
        public static bool operator !=(LingoChar a, LingoChar b) => !(a == b);
    }
}

 

