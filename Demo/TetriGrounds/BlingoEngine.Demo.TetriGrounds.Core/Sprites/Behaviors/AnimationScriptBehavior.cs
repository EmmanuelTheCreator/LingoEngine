// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    /// <summary>
    /// Implemented by behaviours that can provide initial counter values for linked animations.
    /// </summary>
    public interface IHasCounterStartData : IBlingoSpriteBehavior
    {
        int GetCounterStartData(string data);
    }
    // Converted from 15_AnimationScript.ls
    /// <summary>
    /// Drives a frame-by-frame animation by iterating through member numbers.
    /// </summary>
    public class AnimationScriptBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasStepFrameEvent, IHasEndSpriteEvent, IBlingoPropertyDescriptionList
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

        public AnimationScriptBehavior(IBlingoMovieEnvironment env) : base(env) {}

        /// <inheritdoc />
        public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
        {
            return new BehaviorPropertyDescriptionList()
                .Add(this, x => x.myStartMembernum, "My Start membernum:", 0)
                .Add(this, x => x.myEndMembernum, "My End membernum:", 10)
                .Add(this, x => x.myValue, "My Start Value:", -1)
                .Add(this, x => x.mySlowDown, "mySlowDown:", 1)
                .Add(this, x => x.myDataSpriteNum, "My Sprite that contains info\\n(set value to -1):", 1)
                .Add(this, x => x.myDataName, "Name Info:", "1")
                .Add(this, x => x.myWaitbeforeExecute, "WaitTime before execute:", 0)
                .Add(this, x => x.myFunction, "function to execute:", "70")
            ;
        }
        public string? GetBehaviorDescription() => "Controls sprite animation based on member numbers and timing.";

        public string? GetBehaviorTooltip() => "Controls sprite animation based on member numbers and timing.";

        public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => true;


        /// <summary>
        /// Loads the starting frame and queues the behaviour on the actor list.
        /// </summary>
        public void BeginSprite()
        {
            if (!Sprite(myDataSpriteNum).HasSprite()) return;
            if (myValue == -1 || myValue < myStartMembernum || myValue > myEndMembernum)
            {
                
                myValue = SendSprite<IHasCounterStartData,int>(myDataSpriteNum, s => s.GetCounterStartData(myDataName));
                if (myValue <= 0) myValue = 0;
                if (myValue < myStartMembernum || myValue> myEndMembernum)
                    myValue = myStartMembernum;
            }
            UpdateMe();
            myWaiter = myWaitbeforeExecute;
            myAnimate = false;
            StartAnim();
        }

        /// <summary>
        /// Advances the animation when the configured slowdown permits it.
        /// </summary>
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
                        //  finished anim
                        // loop
                        //myValue = myStartMembernum;
                        // END
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

        /// <summary>
        /// Preloads the required members and starts the looping animation.
        /// </summary>
        public void StartAnim()
        {
            for (var i = myStartMembernum; i <= myEndMembernum; i++)
            {
                var member = Member(i);
                if (member != null)
                    member.Preload();
            }
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

        /// <summary>
        /// Removes the behaviour from the actor list when the sprite is deactivated.
        /// </summary>
        public void EndSprite()
        {
            if (_Movie.ActorList.GetPos(this) > 0)
                _Movie.ActorList.Remove(this);
        }

     
    }
}

