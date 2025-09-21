// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Events;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Texts;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
   

    // Converted from 23_TextCounter.ls
    /// <summary>
    /// Simple numeric counter that updates a text member and can be controlled via messages.
    /// </summary>
    public class TextCounterBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, IBlingoPropertyDescriptionList, IHasBlingoMessage
    {
        public int myMax { get; set; } = 10;
        public int myMin { get; set; } = 0;
        /// <summary>
        /// My Start Value
        /// </summary>
        public int myValue { get; set; } = -1;
        public int myStep { get; set; } = 1;
        /// <summary>
        /// My Sprite that contains info\n(set value to -1)
        /// </summary>
        public int myDataSpriteNum { get; set; } = 1;
        /// <summary>
        /// Name Info
        /// </summary>
        public string myDataName { get; set; } = "";
        /// <summary>
        /// WaitTime before execute
        /// </summary>
        public int myWaitbeforeExecute { get; set; } = 70;
        /// <summary>
        /// function to execute
        /// </summary>
        public string myFunction { get; set; } = "";

        private int myWaiter;
        private BlingoMemberText? _textMember;

        public TextCounterBehavior(IBlingoMovieEnvironment env) : base(env){}

        /// <inheritdoc />
        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
        {
            return new BehaviorPropertyDescriptionList()
                .Add(this, x => x.myMin, "Min Value:", 0)
                .Add(this, x => x.myMax, "Max Value:", 10)
                .Add(this, x => x.myValue, "My Start Value:", -1)
                .Add(this, x => x.myStep, "My step:", 1)
                .Add(this, x => x.myDataSpriteNum, "My Sprite that contains info\n(set value to -1):", 1)
                .Add(this, x => x.myDataName, "Name Info:", "1")
                .Add(this, x => x.myWaitbeforeExecute, "WaitTime before execute:", 70)
                .Add(this, x => x.myFunction, "function to execute:", "70");
        }

        public string? GetBehaviorDescription() => "A simple counter that can be increased or decreased with limits.";


        public string? GetBehaviorTooltip() => "A simple counter that can be increased or decreased with limits.";

        public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => true;


        /// <summary>
        /// Initialises the counter value and updates the display.
        /// </summary>
        public void BeginSprite()
        {
            _textMember = Me.Member as BlingoMemberText;
            if (myValue == -1)
            {
                if (myDataSpriteNum > 0)
                {
                    myValue = SendSprite<IHasCounterStartData, int>(myDataSpriteNum, c => c.GetCounterStartData(myDataName));
                    if (myValue <= 0) myValue = 0;
                    if (myValue < myMin || myValue > myMax) myValue = 0;
                }
            }
            UpdateMe();
            myWaiter = myWaitbeforeExecute;
        }

        /// <summary>
        /// Handles delayed execution messages once the wait time has elapsed.
        /// </summary>
        public void ExitFrame()
        {
            if (myDataSpriteNum <= 0) return;
            if (myWaiter < myWaitbeforeExecute)
            {
                if (myWaiter == myWaitbeforeExecute - 1)
                    SendSprite<ExecuteBehavior>(myDataSpriteNum, s => s.HandleMessage(myFunction, myDataName, myValue));
                myWaiter++;
            }
        }

        /// <summary>
        /// Increments the counter when it has not reached the maximum.
        /// </summary>
        public void Addd()
        {
            if (myValue < myMax)
            {
                myValue += myStep;
                UpdateMe();
            }
        }

        /// <summary>
        /// Decrements the counter when above the minimum.
        /// </summary>
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
            if (_textMember != null)
                _textMember.Text = myValue.ToString();
            myWaiter = 0;
        }

        /// <inheritdoc />
        public void HandleMessage(string myFunction, params object[]? parameters)
        {
            switch (myFunction)
            {
                case "Addd": Addd(); break;
                case "Deletee": Deletee(); break;
                default:
                    break;
            }
        }
    }
}

