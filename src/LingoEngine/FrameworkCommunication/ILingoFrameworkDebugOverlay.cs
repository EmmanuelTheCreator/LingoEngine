namespace LingoEngine.FrameworkCommunication
{
    /// <summary>
    /// Provides a debug overlay that can display text information while the
    /// engine is running.
    /// </summary>
    public interface ILingoFrameworkDebugOverlay
    {
        /// <summary>Prepares a line of debug text.</summary>
        int PrepareLine(int id,string text);
        void ShowDebugger();
        void HideDebugger();

        /// <summary>Begins a debug drawing batch.</summary>
        void Begin();
        /// <summary>Sets the text for a prepared line.</summary>
        void SetLineText(int id, string text);
        /// <summary>Ends the debug drawing batch.</summary>
        void End();
    }
}
