// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    /// <summary>
    /// Allows behaviours to receive generic string-based messages.
    /// </summary>
    public interface IHasBlingoMessage : IBlingoSpriteBehavior
    {
        void HandleMessage(string myFunction, params object[]? parameters );
    }
    // Converted from 22_B_Execute.ls
    /// <summary>
    /// Provides button-like behaviour that can call functions on other sprites.
    /// </summary>
    public class ExecuteBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasMouseEnterEvent, IHasMouseLeaveEvent, IHasMouseDownEvent, IBlingoPropertyDescriptionList, IHasBlingoMessage
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
        public int myRollOverMemberCastLib;
        private readonly GlobalVars _globalVars;

        public ExecuteBehavior(IBlingoMovieEnvironment env, GlobalVars globalVars) : base(env)
        {
            _globalVars = globalVars;
        }

        /// <inheritdoc />
        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
        {
            return new BehaviorPropertyDescriptionList()
                .Add(this, x => x.myFunction, "Function:", "0")
                .Add(this, x => x.mySpriteNum, "Sprite Num:", 4)
                .Add(this, x => x.myVar1, "Var 1:", 0)
                .Add(this, x => x.myVar2, "Var 2:", 0)
                .Add(this, x => x.myEnableMouseClick, "Enable Mouseclick:", true)
                .Add(this, x => x.myEnableMouseRollOver, "Enable Mouse rollover:", true)
                .Add(this, x => x.myRollOverMember, "Rollover member:", 0)
                .Add(this, x => x.myRollOverMemberCastLib, "Rollover Castlib ('0' for auto):", 0)
            ;
        }
        public string? GetBehaviorDescription() => "Execute a function on mouse click";

        public string? GetBehaviorTooltip() => "Execute a function on mouse click";

        public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => true;

        /// <summary>
        /// Initialises the behaviour when it becomes active.
        /// </summary>
        public void BeginSprite()
        {
            myLock = false;
            if (myRollOverMember == -1)
            {
                Me.Blend = 0;
            }
        }

        /// <summary>
        /// Handles rollover visuals and optional cursor feedback.
        /// </summary>
        public void MouseEnter(BlingoMouseEvent mouse)
        {
            if (!myLock)
            {
                if (myEnableMouseRollOver && _globalVars.MousePointer != null)
                {
                    _globalVars.MousePointer.Mouse_Over();
                }
                if (myRollOverMember > 0)
                {
                    if (myRollOverMemberCastLib == 0)
                        Me.Member = Member(myRollOverMember);
                    else
                        Me.Member = Member(myRollOverMember, myRollOverMemberCastLib);
                }
                else if (myRollOverMember == -1)
                {
                    Me.Blend = 100;
                }
            }
        }

        /// <summary>
        /// Restores the default visuals when the mouse leaves the sprite.
        /// </summary>
        public void MouseLeave(BlingoMouseEvent mouse)
        {
            if (!myLock)
            {
                if (myEnableMouseRollOver && _globalVars.MousePointer != null)
                {
                    _globalVars.MousePointer.Mouse_Restore();
                }
                if (myRollOverMember > 0)
                {
                    Me.Member = Member(myStartMember);
                }
                else if (myRollOverMember == -1)
                {
                    Me.Blend = 0;
                }
            }
        }

        /// <summary>
        /// Executes the configured function when the sprite is clicked.
        /// </summary>
        public void MouseDown(BlingoMouseEvent mouse)
        {
            if (!myLock && myEnableMouseClick)
            {
                if (string.IsNullOrEmpty(myFunction)) return;
                SendSprite<IHasBlingoMessage>(mySpriteNum, s => s?.HandleMessage(myFunction, myVar1, myVar2));
            }
        }

        /// <summary>
        /// Prevents further hover or click processing.
        /// </summary>
        public void Lock() => myLock = true;
        /// <summary>
        /// Re-enables hover and click processing.
        /// </summary>
        public void UnLock() => myLock = false;

        /// <inheritdoc />
        public void HandleMessage(string myFunction, params object[]? parameters)
        {

        }
    }
}

