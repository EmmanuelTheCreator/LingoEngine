using System;
using System.Collections.Generic;
using LingoEngine.Events;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.SDL2.SDLL;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxCanvas : ILingoFrameworkGfxCanvas, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        private readonly nint _renderer;
        private readonly ILingoFontManager _fontManager;
        private readonly int _width;
        private readonly int _height;
        private nint _texture;

        public nint Texture => _texture;

        public SdlGfxCanvas(nint renderer, ILingoFontManager fontManager, int width = 640, int height = 480)
        {
            _renderer = renderer;
            _fontManager = fontManager;
            _width = width;
            _height = height;
            _texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, width, height);
        }

        private void UseTexture(Action draw)
        {
            var prev = SDL.SDL_GetRenderTarget(_renderer);
            SDL.SDL_SetRenderTarget(_renderer, _texture);
            draw();
            SDL.SDL_SetRenderTarget(_renderer, prev);
        }

        public void Clear(LingoColor color)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, 255);
                SDL.SDL_RenderClear(_renderer);
            });
        }

        public void SetPixel(LingoPoint point, LingoColor color)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, 255);
                SDL.SDL_RenderDrawPointF(_renderer, point.X, point.Y);
            });
        }

        public void DrawLine(LingoPoint start, LingoPoint end, LingoColor color, float width = 1)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, 255);
                SDL.SDL_RenderDrawLineF(_renderer, start.X, start.Y, end.X, end.Y);
            });
        }

        public void DrawRect(LingoRect rect, LingoColor color, bool filled = true, float width = 1)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, 255);
                SDL.SDL_Rect r = new SDL.SDL_Rect
                {
                    x = (int)rect.Left,
                    y = (int)rect.Top,
                    w = (int)rect.Width,
                    h = (int)rect.Height
                };
                if (filled)
                    SDL.SDL_RenderFillRect(_renderer, ref r);
                else
                    SDL.SDL_RenderDrawRect(_renderer, ref r);
            });
        }

        public void DrawCircle(LingoPoint center, float radius, LingoColor color, bool filled = true, float width = 1)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, 255);
                int segs = (int)(radius * 6);
                double step = (Math.PI * 2) / segs;
                float prevX = center.X + radius;
                float prevY = center.Y;
                for (int i = 1; i <= segs; i++)
                {
                    double angle = step * i;
                    float x = center.X + (float)(radius * Math.Cos(angle));
                    float y = center.Y + (float)(radius * Math.Sin(angle));
                    SDL.SDL_RenderDrawLineF(_renderer, prevX, prevY, x, y);
                    if (filled)
                        SDL.SDL_RenderDrawLineF(_renderer, center.X, center.Y, x, y);
                    prevX = x;
                    prevY = y;
                }
            });
        }

        public void DrawArc(LingoPoint center, float radius, float startDeg, float endDeg, int segments, LingoColor color, float width = 1)
        {
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, 255);
                double startRad = startDeg * Math.PI / 180.0;
                double endRad = endDeg * Math.PI / 180.0;
                double step = (endRad - startRad) / segments;
                float prevX = center.X + (float)(radius * Math.Cos(startRad));
                float prevY = center.Y + (float)(radius * Math.Sin(startRad));
                for (int i = 1; i <= segments; i++)
                {
                    double ang = startRad + i * step;
                    float x = center.X + (float)(radius * Math.Cos(ang));
                    float y = center.Y + (float)(radius * Math.Sin(ang));
                    SDL.SDL_RenderDrawLineF(_renderer, prevX, prevY, x, y);
                    prevX = x;
                    prevY = y;
                }
            });
        }

        public void DrawPolygon(IReadOnlyList<LingoPoint> points, LingoColor color, bool filled = true, float width = 1)
        {
            if (points.Count < 2) return;
            UseTexture(() =>
            {
                SDL.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, 255);
                for (int i = 0; i < points.Count - 1; i++)
                    SDL.SDL_RenderDrawLineF(_renderer, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
                SDL.SDL_RenderDrawLineF(_renderer, points[^1].X, points[^1].Y, points[0].X, points[0].Y);
                if (filled)
                {
                    var p0 = points[0];
                    for (int i = 1; i < points.Count - 1; i++)
                    {
                        SDL.SDL_RenderDrawLineF(_renderer, p0.X, p0.Y, points[i].X, points[i].Y);
                        SDL.SDL_RenderDrawLineF(_renderer, p0.X, p0.Y, points[i + 1].X, points[i + 1].Y);
                    }
                }
            });
        }

        public void DrawText(LingoPoint position, string text, string? font = null, LingoColor? color = null, int fontSize = 12)
        {
            UseTexture(() =>
            {
                var path = _fontManager.Get<string>(font ?? string.Empty);
                if (string.IsNullOrEmpty(path)) return;
                nint fnt = SDL_ttf.TTF_OpenFont(path, fontSize);
                if (fnt == nint.Zero) return;
                SDL.SDL_Color c = new SDL.SDL_Color { r = color?.R ?? 0, g = color?.G ?? 0, b = color?.B ?? 0, a = 255 };
                nint surf = SDL_ttf.TTF_RenderUTF8_Blended(fnt, text, c);
                if (surf == nint.Zero)
                {
                    SDL_ttf.TTF_CloseFont(fnt);
                    return;
                }
                nint tex = SDL.SDL_CreateTextureFromSurface(_renderer, surf);
                if (tex != nint.Zero)
                {
                    SDL.SDL_QueryTexture(tex, out _, out _, out int w, out int h);
                    SDL.SDL_Rect dst = new SDL.SDL_Rect { x = (int)position.X, y = (int)position.Y, w = w, h = h };
                    SDL.SDL_RenderCopy(_renderer, tex, nint.Zero, ref dst);
                    SDL.SDL_DestroyTexture(tex);
                }
                SDL.SDL_FreeSurface(surf);
                SDL_ttf.TTF_CloseFont(fnt);
            });
        }

        public void Dispose()
        {
            if (_texture != nint.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = nint.Zero;
            }
        }
    }
}
