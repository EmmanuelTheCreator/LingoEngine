using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Events
{
    public interface IDirectorEventMediator
    {
        void Subscribe(object listener);
        void Unsubscribe(object listener);
        void RaiseSpriteSelected(LingoSprite sprite);
    }

    internal class DirectorEventMediator : IDirectorEventMediator
    {
        private readonly List<IHasSpriteSelectedEvent> _spriteSelected = new();

        public void Subscribe(object listener)
        {
            if (listener is IHasSpriteSelectedEvent spriteSelected)
                _spriteSelected.Add(spriteSelected);
        }

        public void Unsubscribe(object listener)
        {
            if (listener is IHasSpriteSelectedEvent spriteSelected)
                _spriteSelected.Remove(spriteSelected);
        }

        public void RaiseSpriteSelected(LingoSprite sprite)
            => _spriteSelected.ForEach(x => x.SpriteSelected(sprite));
    }
}
