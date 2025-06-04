using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Movies
{
    /// <summary>
    /// You have one stage for all movies
    /// </summary>
    public class LingoStage
    {
        private readonly ILingoFrameworkStage _lingoFrameworkMovieStage;

        public LingoMovie? ActiveMovie { get; private set; }

        public T FrameworkObj<T>() where T : ILingoFrameworkStage => (T)_lingoFrameworkMovieStage;
        public LingoStage(ILingoFrameworkStage godotInstance)
        {
            _lingoFrameworkMovieStage = godotInstance;
        }

        public void SetActiveMovie(LingoMovie lingoMovie)
        {
            ActiveMovie = lingoMovie;
        }

        internal void DrawSprite(LingoSprite sprite) => _lingoFrameworkMovieStage.DrawSprite(sprite);

        internal void RemoveSprite(LingoSprite sprite) => _lingoFrameworkMovieStage.RemoveSprite(sprite);

        internal void UpdateStage() => _lingoFrameworkMovieStage.UpdateStage();

        internal LingoSprite? GetSpriteUnderMouse()
        {
            if (ActiveMovie == null) return null;
            return ActiveMovie.GetSpriteUnderMouse();
        }
    }
}
