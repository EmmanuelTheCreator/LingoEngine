using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Texts
{
    public interface ILingoFrameworkMemberTextBase : ILingoFrameworkMember
    {
        /// <summary>
        /// The raw text contents of the member. This property can be used to read or write the full contents.
        /// </summary>
        string Text { get; set; }
     
     
        /// <summary>
        /// Copies the current selection to the clipboard.
        /// </summary>
        void Copy(string text);

        string PasteClipboard();


        string ReadText();
        string ReadTextRtf();

    }
}
