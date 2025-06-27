using LingoEngine.Inputs;
using LingoEngine.Pictures;

namespace LingoEngine.FrameworkCommunication
{
    /// <summary>
    /// Interface for mouse operations provided by the framework.
    /// </summary>
    public interface ILingoFrameworkMouse
    {
        /// <summary>Shows or hides the mouse cursor.</summary>
        void HideMouse(bool state);
        /// <summary>Sets a custom cursor image.</summary>
        void SetCursor(LingoMemberBitmap image);
        /// <summary>Sets a predefined cursor shape.</summary>
        void SetCursor(LingoMouseCursor value);
    }
}
