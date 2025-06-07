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
        void CopyToClipBoard();
        void Erase();
        void ImportFileInto();
        void PasteClipBoardInto();
        void Preload();
        void Unload();
    }
}
