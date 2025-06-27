using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 16_AppliBg.ls
    public class AppliBgBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, IHasEndSpriteEvent
    {
        private bool myCheckStartData;
        private bool myStop;
        private int myStartLines;
        private int myStartLevel;
        public AppliBgBehavior(ILingoMovieEnvironment env) : base(env){}

        public void BeginSprite()
        {
            myCheckStartData = true;
        }

        public void DataLoaded(string data, object obj){}
        public void SendData(string type, object data){}

        public void ExitFrame()
        {
            if (myCheckStartData)
            {
                if (myStop)
                    _Movie.GoTo(_Movie.CurrentFrame);
                else
                    _Movie.GoTo("Game");
            }
        }

        public void GameFinished(int score){}
        public void ReturnFromSaveScore(string data){}
        public void PersonalHighscores(){}
        public void ShowGeneralScores(){}
        public void RefeshHighScores(){}
        public void NewText(string text){}
        public void TextFinished(object obj){}
        public void DestroyOverscreenTxt(){}
        public int GetCounterStartData(string type) => 0;

        public void EndSprite(){}
    }
}
