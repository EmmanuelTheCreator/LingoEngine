namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkMemberEmpty : ILingoFrameworkMember
    {

    }
    public interface ILingoFrameworkMember 
    {
        /// <summary>
        /// Indicates whether this image is loaded into memory.
        /// Corresponds to: member.text.loaded
        /// </summary>
        bool IsLoaded { get; }
        void CopyToClipboard();
        void Erase();
        void ImportFileInto();
        void PasteClipboardInto();
        void Preload();
        void Unload();
    }
}
