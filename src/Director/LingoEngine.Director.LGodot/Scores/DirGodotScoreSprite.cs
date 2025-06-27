using Godot;
using LingoEngine.Sprites;

namespace LingoEngine.Director.LGodot.Scores;

internal class DirGodotScoreSprite
{
    internal readonly LingoSprite Sprite;
    internal bool Selected;
    internal readonly bool ShowLabel;
    internal bool IsFrameBehaviour;

    internal DirGodotScoreSprite(LingoSprite sprite, bool showLabel = true, bool isFrameBehaviour = false)
    {
        Sprite = sprite;
        ShowLabel = showLabel;
        IsFrameBehaviour = isFrameBehaviour;
    }

    internal void Draw(CanvasItem canvas, Vector2 position, float width, float height, Font font)
    {
        var baseColor = new Color("#ccccff");
        if (Sprite.Lock)
            baseColor = baseColor.Lightened(0.7f);
        if (Selected)
            baseColor = baseColor.Darkened(0.25f);
        canvas.DrawRect(new Rect2(position.X, position.Y, width, height), baseColor);

        float radius = 3f;
        var startCenter = new Vector2(position.X + 3f, position.Y + height / 2f);
        var endCenter = new Vector2(position.X + width - 3f, position.Y + height / 2f);
        canvas.DrawCircle(startCenter, radius, Colors.White);
        canvas.DrawCircle(endCenter, radius, Colors.White);
        canvas.DrawArc(startCenter, radius, 0, 360, 8, Colors.Black);
        canvas.DrawArc(endCenter, radius, 0, 360, 8, Colors.Black);

        if (ShowLabel && Sprite.Member != null)
        {
            canvas.DrawString(font, new Vector2(position.X + 8, position.Y + font.GetAscent() - 6),
                Sprite.Member.Name ?? string.Empty, HorizontalAlignment.Left, width - 8, 9, Colors.Black);
        }
    }
}
