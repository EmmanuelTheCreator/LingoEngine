namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkDebugOverlay
    {
        int PrepareLine(int id,string text);
        void ShowDebugger();
        void HideDebugger();

        void Begin();
        void SetLineText(int id, string text);
        void End();
    }
}
