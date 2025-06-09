
using LingoEngine.Primitives;

namespace LingoEngine.Texts
{
    public interface ILingoFrameworkMemberField : ILingoFrameworkMemberTextBase
    {

        /// <summary>
        /// Enables or disables word wrapping in the field.
        /// </summary>
        bool WordWrap { get; set; }

        /// <summary>
        /// Gets or sets the scroll position of the field.
        /// </summary>
        int ScrollTop { get; set; }
        string TextFont { get; set; }
        int TextSize { get; set; }
        LingoTextStyle TextStyle { get; set; }
        LingoColor TextColor { get; set; }
        int FontSize { get; set; }
        LingoTextAlignment Alignment { get; set; }
        int Margin { get; set; }

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
        void Copy(string text);

        /// <summary>
        /// Cuts the current selection to the clipboard.
        /// </summary>
        void Cut();

        /// <summary>
        /// Pastes clipboard content into the field at the caret position.
        /// </summary>
        void Paste();
        string PasteClipboard();
    }
}
