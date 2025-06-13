using LingoEngine.SDL2.SDLL;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.SDL2.Core;

internal class SdlDebugOverlay : ILingoFrameworkDebugOverlay
{
    private readonly nint _renderer;

    public SdlDebugOverlay(nint renderer)
    {
        _renderer = renderer;
    }

    public void Begin() { }
    public void RenderLine(int x, int y, string text)
    {
        SDL2_gfx.stringColor(_renderer, x, y, text, 0xFFFFFFFF);
    }
    public void End() { }
}
