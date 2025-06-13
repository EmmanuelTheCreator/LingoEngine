namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkDebugOverlay
    {
        void Begin();
        void RenderLine(int x, int y, string text);
        void End();
    }
}
