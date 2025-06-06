namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkMemberEmpty : ILingoFrameworkMember
    {

    }
    public interface ILingoFrameworkMember 
    {
        void CopyToClipBoard();
        void Erase();
        void ImportFileInto();
        void PasteClipBoardInto();
        void Preload();
        void Unload();
    }
}
