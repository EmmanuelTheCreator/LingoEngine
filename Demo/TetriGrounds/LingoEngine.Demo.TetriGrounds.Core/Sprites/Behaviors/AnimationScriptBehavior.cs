using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using LingoEngine.Sprites.Events;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 15_AnimationScript.ls
    public class AnimationScriptBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasStepFrameEvent, IHasEndSpriteEvent
    {
        public int myEndMembernum = 10;
        public int myStartMembernum = 0;
        public int myValue = -1;
        public int mySlowDown = 1;
        public int myDataSpriteNum = 1;
        public string myDataName = "";
        public int myWaitbeforeExecute = 0;
        public string myFunction = "";

        private int myWaiter;
        private bool myAnimate;
        private int mySlowDownCounter;

        public AnimationScriptBehavior(ILingoMovieEnvironment env) : base(env) {}

        public void BeginSprite()
        {
            if (myValue == -1)
            {
                // not implemented: get start data from another sprite
                myValue = myStartMembernum;
            }
            UpdateMe();
            myWaiter = myWaitbeforeExecute;
            myAnimate = false;
            StartAnim();
        }

        public void StepFrame()
        {
            if (myAnimate)
            {
                if (mySlowDownCounter == mySlowDown)
                {
                    mySlowDownCounter = 0;
                    if (myValue <= myEndMembernum)
                    {
                        myValue += 1;
                        UpdateMe();
                    }
                    else
                    {
                        _Movie.ActorList.Remove(this);
                        myAnimate = false;
                    }
                }
                else
                {
                    mySlowDownCounter += 1;
                }
            }
        }

        private void StartAnim()
        {
            myAnimate = true;
            mySlowDownCounter = 0;
            myValue = myStartMembernum;
            _Movie.ActorList.Add(this);
        }

        private void UpdateMe()
        {
            Me.MemberNum = myValue;
            myWaiter = 0;
        }

        public void EndSprite()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}
