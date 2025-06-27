using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;
using LingoEngine.Texts;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 23_TextCounter.ls
    public class TextCounterBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent
    {
        public int myMax = 10;
        public int myMin = 0;
        public int myValue = -1;
        public int myStep = 1;
        public int myDataSpriteNum = 1;
        public string myDataName = "";
        public int myWaitbeforeExecute = 70;
        public string myFunction = "";

        private int myWaiter;

        public TextCounterBehavior(ILingoMovieEnvironment env) : base(env){}

        public void BeginSprite()
        {
            if (myValue == -1)
            {
                myValue = SendSprite<AppliBgBehavior, int>(myDataSpriteNum,
                    c => c.GetCounterStartData(myDataName));
            }
            UpdateMe();
            myWaiter = myWaitbeforeExecute;
        }

        public void ExitFrame()
        {
            if (myWaiter < myWaitbeforeExecute)
            {
                if (myWaiter == myWaitbeforeExecute - 1)
                {
                    SendSprite(myDataSpriteNum, s => ((IHasLingoMessage)s)?.HandleMessage(myFunction, myDataName, myValue));
                }
                myWaiter++;
            }
        }

        public void Addd()
        {
            if (myValue < myMax)
            {
                myValue += myStep;
                UpdateMe();
            }
        }

        public void Deletee()
        {
            if (myValue > myMin)
            {
                myValue -= myStep;
                UpdateMe();
            }
        }

        private void UpdateMe()
        {
            if (Me.Member is LingoMemberText txt)
            {
                txt.Text = myValue.ToString();
            }
            myWaiter = 0;
        }
    }
}
