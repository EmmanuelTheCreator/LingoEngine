using System;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core;

internal abstract class BlPacManItem : BlingoSpriteBehavior, ITileItem
{
    protected BlPacManItem(IBlingoMovieEnvironment env, Map map)
        : base(env)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
    }

    protected Map Map { get; }

    protected BlingoSprite2D ControlledSprite => Me;

    protected float OffsetX => MathF.Floor(ControlledSprite.Width / 2f);

    protected float OffsetY => MathF.Floor(ControlledSprite.Height / 2f);

    protected float X
    {
        get => ControlledSprite.LocH;
        set => ControlledSprite.LocH = value;
    }

    protected float Y
    {
        get => ControlledSprite.LocV;
        set => ControlledSprite.LocV = value;
    }

    public virtual Tile? GetTile()
    {
        return Map.GetTile(X, Y, true);
    }

    public virtual void Destroy()
    {
        ControlledSprite.RemoveMe();
    }

    public virtual void Hide()
    {
        ControlledSprite.Visibility = false;
    }

    public virtual void Show()
    {
        ControlledSprite.Visibility = true;
    }
}
