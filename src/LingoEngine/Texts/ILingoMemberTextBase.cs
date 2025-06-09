
using LingoEngine.Core;

namespace LingoEngine.Texts
{
    public interface ILingoMemberTextBase : ILingoMember
    {
        /// <summary>
        /// The raw text contents of the member. This property can be used to read or write the full contents.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Returns or sets individual lines of the text, 1-based indexing.
        /// </summary>
        LingoLines Line { get; }

        /// <summary>
        /// Returns or sets individual words of the text, 1-based indexing.
        /// </summary>
        LingoWords Word { get; }

        /// <summary>
        /// Returns or sets individual characters of the text, 1-based indexing.
        /// </summary>
        LingoChars Char { get; }

        /// <summary>
        /// Clears the contents of the member.
        /// </summary>
        void Clear();

        /// <summary>
        /// Copies the current selection to the clipboard.
        /// </summary>
        void Copy();

        /// <summary>
        /// Cuts the current selection to the clipboard.
        /// </summary>
        void Cut();

        /// <summary>
        /// Pastes the clipboard contents at the current selection.
        /// </summary>
        void Paste();

        /// <summary>
        /// Inserts the given text at the current selection.
        /// </summary>
        void InsertText(string text);

        /// <summary>
        /// Replaces the currently selected text with the given replacement.
        /// </summary>
        void ReplaceSelection(string replacement);

        /// <summary>
        /// Selects a range of characters within the text. 1-based inclusive range.
        /// </summary>
        void SetSelection(int start, int end);
    }


}



