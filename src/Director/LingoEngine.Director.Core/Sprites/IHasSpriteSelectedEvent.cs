using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Sprites
{
    public interface IHasSpriteSelectedEvent
    {
        void SpriteSelected(ILingoSprite sprite);
    }
}
