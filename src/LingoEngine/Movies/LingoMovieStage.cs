using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Movies
{
    public class LingoMovieStage
    {
        private readonly ILingoFrameworkMovieStage _lingoFrameworkMovieStage;

        public T FrameworkObj<T>() where T : ILingoFrameworkMovieStage => (T)_lingoFrameworkMovieStage;
        public LingoMovieStage(ILingoFrameworkMovieStage godotInstance)
        {
            _lingoFrameworkMovieStage = godotInstance;
        }

        internal void DrawSprite(LingoSprite sprite) => _lingoFrameworkMovieStage.DrawSprite(sprite);

        internal void RemoveSprite(LingoSprite sprite) => _lingoFrameworkMovieStage.RemoveSprite(sprite);

        internal void UpdateStage() => _lingoFrameworkMovieStage.UpdateStage();

        
    }
}
