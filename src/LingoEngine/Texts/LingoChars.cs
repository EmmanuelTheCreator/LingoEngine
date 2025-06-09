using System.Collections;

namespace LingoEngine.Texts
{
    /// <summary>
    /// Represents a 1-based character accessor for a given word or string.
    /// </summary>
    public class LingoChars : IEnumerable<char> , IEnumerable
    {
        private string _word = "";

        /// <summary>
        /// Gets the character at the specified 1-based index.
        /// </summary>
        public char this[int index] => _word[index - 1];

        /// <summary>
        /// Returns the full string represented by this character wrapper.
        /// </summary>
        public override string ToString() => _word;

        /// <summary>
        /// The number of characters.
        /// </summary>
        public int Count => _word.Length;

        internal void SetText(string word) => _word = word;

        public IEnumerator<char> GetEnumerator() => _word.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _word.GetEnumerator();

        public static implicit operator string(LingoChars lingoChar) => lingoChar.ToString();

        public override bool Equals(object? obj) => obj is LingoChars lc && _word == lc._word;
        public override int GetHashCode() => _word.GetHashCode();
        public static bool operator ==(LingoChars a, LingoChars b) => a._word == b._word;
        public static bool operator !=(LingoChars a, LingoChars b) => !(a == b);
    }

}



