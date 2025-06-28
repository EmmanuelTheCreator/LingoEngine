using LingoEngine.Bitmaps;
using LingoEngine.Primitives;
using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// High level drawing surface used by the engine.
    /// Rendering back-ends provide the <see cref="ILingoFrameworkGfxCanvas"/>
    /// implementation which performs the actual drawing operations.
    /// </summary>
    public class LingoGfxCanvas : LingoGfxNodeLayoutBase<ILingoFrameworkGfxCanvas>
    {

        public void Clear(LingoColor color) => _framework.Clear(color);
        public void SetPixel(LingoPoint point, LingoColor color) => _framework.SetPixel(point, color);
        public void DrawLine(LingoPoint start, LingoPoint end, LingoColor color, float width = 1)
            => _framework.DrawLine(start, end, color, width);
        public void DrawRect(LingoRect rect, LingoColor color, bool filled = true, float width = 1)
            => _framework.DrawRect(rect, color, filled, width);
        public void DrawCircle(LingoPoint center, float radius, LingoColor color, bool filled = true, float width = 1)
            => _framework.DrawCircle(center, radius, color, filled, width);
        public void DrawArc(LingoPoint center, float radius, float startDeg, float endDeg, int segments, LingoColor color, float width = 1)
            => _framework.DrawArc(center, radius, startDeg, endDeg, segments, color, width);
        public void DrawPolygon(IReadOnlyList<LingoPoint> points, LingoColor color, bool filled = true, float width = 1)
            => _framework.DrawPolygon(points, color, filled, width);
        public void DrawText(LingoPoint position, string text, string? font = null, LingoColor? color = null, int fontSize = 12, int width = -1)
            => _framework.DrawText(position, text, font, color, fontSize, width);
        public void DrawPicture(ILingoImageTexture texture, int width, int height, LingoPoint position)
            => _framework.DrawPicture(texture, width, height, position);
        public void DrawPicture(byte[] data, int width, int height, LingoPoint position, LingoPixelFormat format)
            => _framework.DrawPicture(data, width, height, position, format);
    }
}
