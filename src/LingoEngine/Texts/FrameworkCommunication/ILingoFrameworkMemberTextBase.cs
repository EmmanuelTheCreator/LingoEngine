using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;

namespace LingoEngine.Texts.FrameworkCommunication
{
    public interface ILingoFrameworkMemberTextBase : ILingoFrameworkMember
    {
        /// <summary>
        /// The raw text contents of the member. This property can be used to read or write the full contents.
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Enables or disables word wrapping in the field.
        /// </summary>
        bool WordWrap { get; set; }

        /// <summary>
        /// Gets or sets the scroll position of the field.
        /// </summary>
        int ScrollTop { get; set; }
        string FontName { get; set; }
        int FontSize { get; set; }
        LingoTextStyle FontStyle { get; set; }
        LingoColor TextColor { get; set; }
        LingoTextAlignment Alignment { get; set; }
        int Margin { get; set; }

        /// <summary>
        /// Copies the current selection to the clipboard.
        /// </summary>
        void Copy(string text);

        string PasteClipboard();


        string ReadText();
        string ReadTextRtf();

    }
}
