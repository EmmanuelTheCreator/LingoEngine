namespace LingoEngineSDL2;

public class SdlRootContext
{
    public IntPtr Window { get; }
    public IntPtr Renderer { get; }

    public SdlRootContext(IntPtr window, IntPtr renderer)
    {
        Window = window;
        Renderer = renderer;
    }
}
