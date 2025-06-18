using LingoEngine.Members;
using LingoEngine.Primitives;

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






        /// <summary>
        /// Indicates whether the field is editable at runtime. Equivalent to the Lingo 'editable' property.
        /// </summary>
        bool Editable { get; set; }

        /// <summary>
        /// Enables or disables word wrapping in the field.
        /// </summary>
        bool WordWrap { get; set; }


        /// <summary>
        /// Gets or sets the scroll position of the field.
        /// </summary>
        int ScrollTop { get; set; }

        /// <summary>
        /// Font name used in the field (e.g., "Arial").
        /// Corresponds to Lingo's textFont property.
        /// </summary>
        string Font { get; set; }

        /// <summary>
        /// Font size for the field text, in points.
        /// Corresponds to Lingo's textSize property.
        /// </summary>
        int FontSize { get; set; }

        /// <summary>
        /// Font style flags: 
        /// combination of bold (1), italic (2), underline (4).
        /// Corresponds to Lingo's textStyle property.
        /// </summary>
        LingoTextStyle FontStyle { get; set; }

        /// <summary>
        /// Text color in a Lingo-compatible color format.
        /// Corresponds to Lingo's textColor property.
        /// </summary>
        LingoColor TextColor { get; set; }

        /// <summary>
        /// Gets or sets whether the text is bold.
        /// </summary>
        bool Bold { get; set; }

        /// <summary>
        /// Gets or sets whether the text is italic.
        /// </summary>
        bool Italic { get; set; }

        /// <summary>
        /// Gets or sets whether the text is underlined.
        /// </summary>
        bool Underline { get; set; }

        /// <summary>
        /// Gets or sets the alignment of the text in the field: 0 = left, 1 = center, 2 = right.
        /// </summary>
        LingoTextAlignment Alignment { get; set; }

        /// <summary>
        /// Gets or sets the margin (padding) around the text inside the field.
        /// </summary>
        int Margin { get; set; }
    }


}



