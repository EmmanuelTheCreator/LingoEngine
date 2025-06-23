using LingoEngine.Primitives;
using System;
using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific drawing surface implementation.
    /// </summary>
    public interface ILingoFrameworkGfxCanvas : IDisposable
    {
        void Clear(LingoColor color);
        void SetPixel(LingoPoint point, LingoColor color);
        void DrawLine(LingoPoint start, LingoPoint end, LingoColor color, float width = 1);
        void DrawRect(LingoRect rect, LingoColor color, bool filled = true, float width = 1);
        void DrawCircle(LingoPoint center, float radius, LingoColor color, bool filled = true, float width = 1);
        void DrawArc(LingoPoint center, float radius, float startDeg, float endDeg, int segments, LingoColor color, float width = 1);
        void DrawPolygon(IReadOnlyList<LingoPoint> points, LingoColor color, bool filled = true, float width = 1);
        void DrawText(LingoPoint position, string text, string? font = null, LingoColor? color = null, int fontSize = 12);
    }
}
