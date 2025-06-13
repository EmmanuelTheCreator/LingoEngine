using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 22_B_Execute.ls
    public class ExecuteBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasMouseEnterEvent, IHasMouseLeaveEvent, IHasMouseDownEvent
    {
        public string myFunction = "";
        public int mySpriteNum = 4;
        public int myVar1;
        public int myVar2;
        private bool myLock;
        public bool myEnableMouseClick = true;
        public bool myEnableMouseRollOver = true;
        public int myStartMember;
        public int myRollOverMember;
        public string myRollOverMemberCastLib = "";

        public ExecuteBehavior(ILingoMovieEnvironment env) : base(env){}

        public void BeginSprite()
        {
            myLock = false;
            if (myRollOverMember == -1)
            {
                Me.Blend = 0;
            }
        }

        public void MouseEnter(ILingoMouse mouse)
        {
            if (!myLock)
            {
                if (myEnableMouseRollOver)
                {
                    // gMousePointer.Mouse_Over(); not implemented
                }
                if (myRollOverMember > 0)
                {
                    Me.MemberNum = myRollOverMember;
                }
                else if (myRollOverMember == -1)
                {
                    Me.Blend = 100;
                }
            }
        }

        public void MouseLeave(ILingoMouse mouse)
        {
            if (!myLock)
            {
                if (myEnableMouseRollOver)
                {
                    // gMousePointer.Mouse_Restore();
                }
                if (myRollOverMember > 0)
                {
                    Me.MemberNum = myStartMember;
                }
                else if (myRollOverMember == -1)
                {
                    Me.Blend = 0;
                }
            }
        }

        public void MouseDown(ILingoMouse mouse)
        {
            if (!myLock && myEnableMouseClick)
            {
                if (string.IsNullOrEmpty(myFunction)) return;
                SendSprite(mySpriteNum, s => ((IHasLingoMessage)s)?.HandleMessage(myFunction, myVar1, myVar2));
            }
        }

        public void Lock() => myLock = true;
        public void UnLock() => myLock = false;
    }
}
