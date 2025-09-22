using System;
using AbstUI.SDL2.SDLL;

namespace AbstUI.SDL2.Tools;

public static class SDLRectHelpers
{
    public static SDL.SDL_Rect IntersectWith(this SDL.SDL_Rect rect, SDL.SDL_Rect clip)
    {
        int x1 = Math.Max(rect.x, clip.x);
        int y1 = Math.Max(rect.y, clip.y);
        int x2 = Math.Min(rect.x + rect.w, clip.x + clip.w);
        int y2 = Math.Min(rect.y + rect.h, clip.y + clip.h);
        if (x2 <= x1 || y2 <= y1)
            return new SDL.SDL_Rect { x = 0, y = 0, w = 0, h = 0 };
        return new SDL.SDL_Rect { x = x1, y = y1, w = x2 - x1, h = y2 - y1 };
    }

    public static bool ContainsPoint(this SDL.SDL_Rect rect, float x, float y)
        => rect.w > 0 && rect.h > 0 && x >= rect.x && x <= rect.x + rect.w && y >= rect.y && y <= rect.y + rect.h;
}
