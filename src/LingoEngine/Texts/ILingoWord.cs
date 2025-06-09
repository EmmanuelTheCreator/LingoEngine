namespace LingoEngine.Texts
{
    /// <summary>
    /// Represents word-level access to a text string, with 1-based indexing as in Lingo.
    /// Supports accessing words and characters within words.
    /// </summary>
    public interface ILingoWord : System.Collections.IEnumerable, IEnumerable<ILingoWord>
    {
        /// <summary>
        /// Gets the word at the specified 1-based index.
        /// </summary>
        /// <param name="index">1-based index of the word.</param>
        /// <returns>The word at the given index.</returns>
        ILingoWord this[int index] { get; }

        /// <summary>
        /// Gets the character at the specified word and character index (both 1-based).
        /// </summary>
        /// <param name="wordIndex">1-based index of the word.</param>
        /// <param name="charIndex">1-based index of the character within the word.</param>
        /// <returns>The character at the specified position.</returns>
        ILingoChar this[int wordIndex, int charIndex] { get; }

        /// <summary>
        /// Gets the total number of words.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Retrieves a LingoChar object representing the characters in the specified word.
        /// </summary>
        /// <param name="wordIndex">1-based index of the word.</param>
        /// <returns>A LingoChar instance for the word at the given index.</returns>
        ILingoChar GetChar(int wordIndex);
    }


}
