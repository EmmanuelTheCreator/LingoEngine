using AbstUI.SDL2.Components.Base;
using AbstUI.SDL2.Core;
using AbstUI.SDL2.Windowing;

namespace AbstUI.SDL2.Components.Containers;

internal static class AbstSdlControlPopupExtensions
{
    public static (int x, int y) GetScreenPosition(this AbstSDLComponentContext ctx)
    {
        int x = 0, y = 0;
        var current = ctx;
        while (current != null)
        {
            x += current.X + (int)current.OffsetX;
            y += current.Y + (int)current.OffsetY;
            // it seem if we do this, it scroll doubles and badly aligns
            //if (current.Component is AbstSdlScrollViewer sv)
            //{
            //    x -= (int)sv.ScrollHorizontal;
            //    //y -= (int)(sv.ScrollVertical/10); // + current.Y;
            //}
            if (current.Component is AbstSdlWindow)
            {
                y += AbstSdlWindow._titleBarHeight;
            }
            current = current.VisualParent;
        }
        return (x, y);
    }

    public static void PositionBelow(this AbstSdlComponent popup, AbstSDLComponentContext ownerContext, float offsetY)
    {
        var (sx, sy) = ownerContext.GetScreenPosition();
        popup.X = sx;
        popup.Y = sy + offsetY;
    }
}

