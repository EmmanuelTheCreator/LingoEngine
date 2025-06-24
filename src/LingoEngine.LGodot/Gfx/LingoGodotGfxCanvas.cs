using Godot;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Events;
using LingoEngine.LGodot.Primitives;
using System;
using System.Linq;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxCanvas"/>.
    /// </summary>
    public partial class LingoGodotGfxCanvas : Control, ILingoFrameworkGfxCanvas, IDisposable
    {
        private readonly ILingoFontManager _fontManager;
        private LingoMargin _margin = LingoMargin.Zero;

        private Color? _clearColor;
        private readonly List<(Vector2 start, Vector2 end, Color color, float width)> _lines = new();
        private readonly List<(Rect2 rect, Color color, bool filled, float width)> _rects = new();
        private readonly List<(Vector2 center, float radius, Color color, bool filled, float width)> _circles = new();
        private readonly List<(Vector2 center, float radius, float start, float end, int segs, Color color, float width)> _arcs = new();
        private readonly List<(Vector2[] points, Color color, bool filled, float width)> _polys = new();
        private readonly List<(Vector2 pos, string text, string? font, Color color, int size)> _texts = new();
        private readonly List<(Vector2 pos, Color color)> _pixels = new();
        private bool _dirty;

        public LingoGodotGfxCanvas(LingoGfxCanvas canvas, ILingoFontManager fontManager, int width, int height)
        {
            _fontManager = fontManager;
            canvas.Init(this);
            Size = new Vector2(width, height);
        }

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        public LingoMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        private void MarkDirty()
        {
            if (!_dirty)
            {
                _dirty = true;
                QueueRedraw();
            }
        }

        public override void _Draw()
        {
            if (_clearColor is Color clr)
                DrawRect(new Rect2(0, 0, Size.X, Size.Y), clr, true);

            foreach (var p in _pixels)
                DrawRect(new Rect2(p.pos.X, p.pos.Y, 1, 1), p.color, true);

            foreach (var l in _lines)
                DrawLine(l.start, l.end, l.color, l.width);

            foreach (var r in _rects)
                DrawRect(r.rect, r.color, r.filled, r.width);

            foreach (var c in _circles)
            {
                if (c.filled)
                    DrawCircle(c.center, c.radius, c.color);
                else
                    DrawArc(c.center, c.radius, 0, 360, 32, c.color, c.width);
            }

            foreach (var a in _arcs)
                DrawArc(a.center, a.radius, a.start, a.end, a.segs, a.color, a.width);

            foreach (var p in _polys)
            {
                if (p.filled)
                    DrawPolygon(p.points, new[] { p.color });
                else
                    DrawPolyline(p.points, p.color, p.width, true);
            }

            foreach (var t in _texts)
            {
                Font font = _fontManager.Get<FontFile>(t.font ?? "") ?? ThemeDB.FallbackFont;
                DrawString(font, t.pos, t.text, HorizontalAlignment.Left, -1, t.size, t.color);
            }

            _lines.Clear();
            _rects.Clear();
            _circles.Clear();
            _arcs.Clear();
            _polys.Clear();
            _texts.Clear();
            _pixels.Clear();
            _clearColor = null;
            _dirty = false;
        }

        public void Clear(LingoColor color)
        {
            _lines.Clear();
            _rects.Clear();
            _texts.Clear();
            _pixels.Clear();
            _clearColor = color.ToGodotColor();
            MarkDirty();
        }

        public void SetPixel(LingoPoint point, LingoColor color)
        {
            _pixels.Add((point.ToVector2(), color.ToGodotColor()));
            MarkDirty();
        }

        public void DrawLine(LingoPoint start, LingoPoint end, LingoColor color, float width = 1)
        {
            _lines.Add((start.ToVector2(), end.ToVector2(), color.ToGodotColor(), width));
            MarkDirty();
        }

        public void DrawRect(LingoRect rect, LingoColor color, bool filled = true, float width = 1)
        {
            _rects.Add((new Rect2(rect.Left, rect.Top, rect.Width, rect.Height), color.ToGodotColor(), filled, width));
            MarkDirty();
        }

        public void DrawCircle(LingoPoint center, float radius, LingoColor color, bool filled = true, float width = 1)
        {
            _circles.Add((center.ToVector2(), radius, color.ToGodotColor(), filled, width));
            MarkDirty();
        }

        public void DrawArc(LingoPoint center, float radius, float startDeg, float endDeg, int segments, LingoColor color, float width = 1)
        {
            _arcs.Add((center.ToVector2(), radius, startDeg, endDeg, segments, color.ToGodotColor(), width));
            MarkDirty();
        }

        public void DrawPolygon(IReadOnlyList<LingoPoint> points, LingoColor color, bool filled = true, float width = 1)
        {
            var arr = points.Select(p => p.ToVector2()).ToArray();
            _polys.Add((arr, color.ToGodotColor(), filled, width));
            MarkDirty();
        }

        public void DrawText(LingoPoint position, string text, string? font = null, LingoColor? color = null, int fontSize = 12)
        {
            _texts.Add((position.ToVector2(), text, font, color.HasValue ? color.Value.ToGodotColor() : Colors.Black, fontSize));
            MarkDirty();
        }

        public void Dispose()
        {
            QueueFree();
        }
    }
}
