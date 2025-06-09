using LingoEngine.Primitives;

namespace LingoEngine.Texts
{
    public interface ILingoMemberField : ILingoMemberTextBase
    {

        /// <summary>
        /// Indicates whether the field is editable at runtime. Equivalent to the Lingo 'editable' property.
        /// </summary>
        bool Editable { get; set; }

        /// <summary>
        /// Enables or disables word wrapping in the field.
        /// </summary>
        bool WordWrap { get; set; }

        /// <summary>
        /// Gets or sets whether the field uses multiple lines.
        /// </summary>
        bool MultiLine { get; set; }

        /// <summary>
        /// Gets or sets the scroll position of the field.
        /// </summary>
        int ScrollTop { get; set; }

        /// <summary>
        /// Font name used in the field (e.g., "Arial").
        /// Corresponds to Lingo's textFont property.
        /// </summary>
        string TextFont { get; set; }

        /// <summary>
        /// Font size for the field text, in points.
        /// Corresponds to Lingo's textSize property.
        /// </summary>
        int TextSize { get; set; }

        /// <summary>
        /// Font style flags: 
        /// combination of bold (1), italic (2), underline (4).
        /// Corresponds to Lingo's textStyle property.
        /// </summary>
        LingoTextStyle TextStyle { get; set; }

        /// <summary>
        /// Text color in a Lingo-compatible color format.
        /// Corresponds to Lingo's textColor property.
        /// </summary>
        LingoColor TextColor { get; set; }

        /// <summary>
        /// Gets or sets the font size of the text in the field.
        /// </summary>
        int FontSize { get; set; }

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

        /// Returns TRUE if the field is currently focused (has keyboard input).
        /// </summary>
        bool IsFocused { get; }
        

        /// <summary>
        /// Selects a portion of text within the field from start to end index.
        /// </summary>
        void SetSelection(int start, int end);

        /// <summary>
        /// Replaces the currently selected text with the specified string.
        /// </summary>
        void ReplaceSelection(string replacement);

        /// <summary>
        /// Inserts text at the current caret position.
        /// </summary>
        void InsertText(string text);

        /// <summary>
        /// Clears the field content.
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
        /// Pastes clipboard content into the field at the caret position.
        /// </summary>
        void Paste();


    }

}



