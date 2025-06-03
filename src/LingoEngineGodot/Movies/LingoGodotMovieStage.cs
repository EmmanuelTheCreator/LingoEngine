using LingoEngine;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;

namespace LingoEngineGodot.Movies
{
    public class LingoGodotMovieStage : ILingoFrameworkMovieStage, IDisposable
    {
        private LingoMovieStage _LingoMovieStage;

       

        public void DrawSprite(LingoSprite sprite)
        {
            
        }

        public void RemoveSprite(LingoSprite sprite)
        {
            
        }

        public void UpdateStage(List<LingoSprite> activeSprites)
        {
            
        }

        internal void Init(LingoMovieStage lingoInstance)
        {
            _LingoMovieStage = lingoInstance;
        }

        public void Dispose()
        {

        }
    }
}
