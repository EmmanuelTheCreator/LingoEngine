using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;

namespace LingoEngine.Director.Core.UI
{
    public static class GfxWrapPanelExtensions
    {
        public static GfxWrapPanelBuilder Compose(this LingoGfxWrapPanel panel, ILingoFrameworkFactory factory)
        {
            var builder = new GfxWrapPanelBuilder(panel, factory);
            return builder;
        }
    }
}
