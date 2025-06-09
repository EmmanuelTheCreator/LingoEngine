namespace LingoEngine.Texts
{
    /// <summary>
    /// Represents the lines of text in a Lingo text or field member. Provides 1-based access and support for nested word/char queries.
    /// </summary>
    public interface ILingoLine : IEnumerable<string>, System.Collections.IEnumerable
    {
        /// <summary>
        /// Gets or sets the text content of a specific line using 1-based indexing.
        /// </summary>
        /// <param name="index">The 1-based index of the line.</param>
        /// <returns>The content of the specified line.</returns>
        string this[int index] { get; set; }

        /// <summary>
        /// Gets the total number of lines in the text.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns the ILingoWord object representing the words of a specific line.
        /// </summary>
        /// <param name="lineIndex">The 1-based index of the line.</param>
        /// <returns>An ILingoWord representing the words in that line.</returns>
        ILingoWord GetWord(int lineIndex);

        /// <summary>
        /// Returns the ILingoChar object representing the characters of a specific word in a specific line.
        /// </summary>
        /// <param name="lineIndex">The 1-based index of the line.</param>
        /// <param name="wordIndex">The 1-based index of the word in the line.</param>
        /// <returns>An ILingoChar object representing characters in the word.</returns>
        ILingoChar GetChar(int lineIndex, int wordIndex);

        /// <summary>
        /// Returns a specific character within a word in a line.
        /// </summary>
        /// <param name="lineIndex">The 1-based index of the line.</param>
        /// <param name="wordIndex">The 1-based index of the word in the line.</param>
        /// <param name="charIndex">The 1-based index of the character in the word.</param>
        /// <returns>The ILingoChar object containing the character at the specified location.</returns>
        ILingoChar GetChar(int lineIndex, int wordIndex, int charIndex);
    }



}
