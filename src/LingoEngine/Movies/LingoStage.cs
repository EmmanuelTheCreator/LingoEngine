using LingoEngine.FrameworkCommunication;
using LingoEngine.Members;

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
             

        public ILingoFrameworkStage FrameworkObj() => _lingoFrameworkMovieStage;
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
            sprite.AddKeyframes((frame, sprite.LocH, sprite.LocV, sprite.Rotation, sprite.Skew));
        }

        internal void UpdateKeyFrame(LingoSprite sprite)
        {
            if (!RecordKeyframes || ActiveMovie == null)
                return;
            int frame = ActiveMovie.CurrentFrame;
            sprite.UpdateKeyframe(frame, sprite.LocH, sprite.LocV, sprite.Rotation, sprite.Skew);
        }

        internal void SetSpriteTweenOptions(LingoSprite sprite, bool positionEnabled, bool rotationEnabled,
            bool skewEnabled, bool foregroundColorEnabled, bool backgroundColorEnabled, bool blendEnabled,
            float curvature, bool continuousAtEnds, bool speedSmooth, float easeIn, float easeOut)
        {
            sprite.SetSpriteTweenOptions(positionEnabled, rotationEnabled, skewEnabled,
                foregroundColorEnabled, backgroundColorEnabled, blendEnabled,
                curvature, continuousAtEnds, speedSmooth, easeIn, easeOut);
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
            if (ActiveMovie == null)
                return null;

            bool skipLockedSprites = !ActiveMovie.IsPlaying;
            return ActiveMovie.GetSpriteUnderMouse(skipLockedSprites);
        }

        public Animations.LingoSpriteMotionPath? GetSpriteMotionPath(LingoSprite sprite)
        {
            if (sprite == null) return null;
            return sprite.CallActor<Animations.LingoSpriteAnimator, Animations.LingoSpriteMotionPath>(
                a => a.GetMotionPath(sprite.BeginFrame, sprite.EndFrame));
        }
    }
}
