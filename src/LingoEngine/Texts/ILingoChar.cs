namespace LingoEngine.Texts
{
    /// <summary>
    /// Represents character-level access to a text string, with 1-based indexing as in Lingo.
    /// </summary>
    public interface ILingoChar : System.Collections.IEnumerable, IEnumerable<char>
    {
        /// <summary>
        /// Gets the character at the specified 1-based index.
        /// </summary>
        /// <param name="index">1-based index of the character to retrieve.</param>
        /// <returns>The character at the given index.</returns>
        char this[int index] { get; }

        /// <summary>
        /// Gets the total number of characters.
        /// </summary>
        int Count { get; }
    }



}
