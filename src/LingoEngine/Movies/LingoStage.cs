using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Movies
{
    /// <summary>
    /// You have one stage for all movies
    /// </summary>
    public class LingoStage
    {
        private readonly ILingoFrameworkStage _lingoFrameworkMovieStage;

        public bool RecordKeyframes { get; set; }

        public LingoMovie? ActiveMovie { get; private set; }
        public LingoMember? MouseMemberUnderMouse
        {
            get
            {
                if (ActiveMovie == null) return null;
                return ActiveMovie.MouseMemberUnderMouse();
            }
        }
             

        public T Framework<T>() where T : class, ILingoFrameworkStage => (T)_lingoFrameworkMovieStage;
        public LingoStage(ILingoFrameworkStage godotInstance)
        {
            _lingoFrameworkMovieStage = godotInstance;
        }

        internal void AddKeyFrame(LingoSprite sprite)
        {
            if (!RecordKeyframes || ActiveMovie == null)
                return;
            int frame = ActiveMovie.CurrentFrame;
            sprite.Animator.AddKeyFrame(frame, sprite.LocH, sprite.LocV, sprite.Rotation, sprite.Skew);
        }

        public void SetActiveMovie(LingoMovie? lingoMovie)
        {
            if (ActiveMovie != null)
                ActiveMovie.Hide();
            ActiveMovie = lingoMovie;
            if (lingoMovie != null)
                lingoMovie.Show();
            _lingoFrameworkMovieStage.SetActiveMovie(lingoMovie);
        }

     
        internal LingoSprite? GetSpriteUnderMouse()
        {
            if (ActiveMovie == null) return null;
            return ActiveMovie.GetSpriteUnderMouse();
        }
    }
}
