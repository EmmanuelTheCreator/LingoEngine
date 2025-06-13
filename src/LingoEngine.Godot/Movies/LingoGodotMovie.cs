using Godot;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Primitives;

namespace LingoEngine.Godot.Movies
{
    public partial class LingoGodotMovie : ILingoFrameworkMovie, IDisposable
    {
        private Node2D _MovieNode2D;
        private LingoMovie _LingoMovie;
        private LingoGodotStage _stage;
        private readonly Action<LingoGodotMovie> _removeMethod;
        private HashSet<LingoGodotSprite> _drawnSprites = new();
        private HashSet<LingoGodotSprite> _allSprites = new();

        public Node2D GetNode2D() => _MovieNode2D;

#pragma warning disable CS8618         
        public LingoGodotMovie(LingoGodotStage stage, LingoMovie lingoInstance, Action<LingoGodotMovie> removeMethod)
#pragma warning restore CS8618 
        {
            _stage = stage;
            _LingoMovie = lingoInstance;
            _removeMethod = removeMethod;

            _MovieNode2D = new Node2D();
            //_MovieNode2D.Position = new Vector2(640/2, 480/2);

            stage.ShowMovie(this);
        }

        internal void Show()
        {
            _stage.ShowMovie(this);
        }
        internal void Hide()
        {
            _stage.HideMovie(this);
        }

        public void UpdateStage()
        {
            foreach (var godotSprite in _drawnSprites)
                godotSprite.Update();
        }

        internal void CreateSprite<T>(T lingoSprite) where T : LingoSprite
        {
            var godotSprite = new LingoGodotSprite(lingoSprite, _MovieNode2D, s =>
            {
                // Show Sprite
                _drawnSprites.Add(s);
            }, s =>
            {
                // Hide sprite
                // Remove the sprite from the timeLine
                _drawnSprites.Remove(s);
            }, s =>
            {
                // Dispose
                // Definitly remove sprite from memory when we close the movie
                _drawnSprites.Remove(s);
                _allSprites.Remove(s);
            });
            _allSprites.Add(godotSprite);
        }

        public void RemoveMe()
        {
            _removeMethod(this);
            _MovieNode2D.GetParent().RemoveChild(_MovieNode2D);
            _MovieNode2D.Dispose();
        }
        LingoPoint ILingoFrameworkMovie.GetGlobalMousePosition()
        {
            var pos = _MovieNode2D.GetGlobalMousePosition();
            return (pos.X, pos.Y);
        }

        public void Dispose()
        {
            Hide();
            RemoveMe();
        }
    }
}
