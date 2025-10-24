// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Texts;
#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    /// <summary>
    /// Callback contract used to notify parent scripts when overlay text finishes its animation.
    /// </summary>
    public interface IOverScreenTextParent
    {
        void TextFinished(OverScreenTextScript text);
    }
    // Converted from 6_OverScreenText.ls
    /// <summary>
    /// Animates the translucent overlay text that slides onto the screen.
    /// </summary>
    public class OverScreenTextScript : BlingoParentScript, IHasStepFrameEvent
    {
        private readonly GlobalVars _global;
        private int myNum = 48;
        private int myNum2 = 49;
        private int myCounter = 0;

        public int Duration { get; set; } = 130;
        public string Text { get; set; } = string.Empty;
        public int LocV { get; set; } = 100;
        public IOverScreenTextParent? Parent { get; set; }

        /// <summary>
        /// Creates the overlay, assigns the target members and starts the animation immediately.
        /// </summary>
        public OverScreenTextScript(IBlingoMovieEnvironment env, GlobalVars global, int duration, string text, IOverScreenTextParent parent) : base(env)
        {
            Duration = duration;
            Text = text;
            _global = global;
            _Movie.ActorList.Add(this);
            Parent = parent;
            if (Sprite(myNum).Puppet)
            {
                myNum = 46;
                myNum2 = 47;
                LocV = 130;
            }
            var over1 = Sprite(myNum);
            var over2 = Sprite(myNum2);
            over1.Puppet = true;
            over2.Puppet = true;
            over1.SetMember("T_OverScreen");
            over2.SetMember("T_OverScreen2");
            over1.LocZ = 1000;
            over2.LocZ = 999;
            over1.Ink = 36;
            over2.Ink = 36;
            Member<BlingoMemberText>("T_OverScreen")!.Text = Text;
            Member<BlingoMemberText>("T_OverScreen2")!.Text = Text;
        }

        

        /// <summary>
        /// Handles the scroll-up animation and blend fade-out.
        /// </summary>
        public void StepFrame()
        {
            if (!Sprite(myNum).HasSprite()) return;
            myCounter += 1;
            if (myCounter > Duration)
            {
                //_Movie.ActorList.Remove(this);
                //Sprite(myNum).Puppet = false;
                //Sprite(myNum2).Puppet = false;
                Parent?.TextFinished(this);
                return;
            }
            LocV += 2;
            Sprite(myNum).LocH = 140;
            Sprite(myNum2).LocH = 142;
            Sprite(myNum).LocV = LocV;
            Sprite(myNum2).LocV = LocV + 2;
            float blend = 70f - (float)myCounter / Duration * 100f;
            if (blend < 0) blend = 0;
            Sprite(myNum).Blend = (int)blend;
            Sprite(myNum2).Blend = (int)blend;

        }

        /// <summary>
        /// Releases puppet control so the overlay sprites can be reused by future notifications.
        /// </summary>
        public void Destroy()
        {
            if (_Movie.ActorList.GetPos(this) > 0)
                _Movie.ActorList.Remove(this);
            Sprite(myNum).Puppet = false;
            Sprite(myNum2).Puppet = false;
        }
    }
}

