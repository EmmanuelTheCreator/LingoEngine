using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Pictures.LingoEngine;

namespace LingoEngineSDL2;

public class SdlMouse : ILingoFrameworkMouse
{
    private Lazy<LingoMouse> _lingoMouse;

    public SdlMouse(Lazy<LingoMouse> mouse)
    {
        _lingoMouse = mouse;
    }

    internal void SetLingoMouse(LingoMouse mouse) => _lingoMouse = new Lazy<LingoMouse>(() => mouse);

    public void HideMouse(bool state) { }
    public void SetCursor(LingoMemberPicture image) { }
    public void SetCursor(LingoMouseCursor value) { }
}
