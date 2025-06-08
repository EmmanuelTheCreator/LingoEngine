using LingoEngine.Core;
using LingoEngine.Pictures.LingoEngine;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkMouse
    {
        void HideMouse(bool state);
        void SetCursor(LingoMemberPicture image);
        void SetCursor(LingoMouseCursor value);
    }
}
