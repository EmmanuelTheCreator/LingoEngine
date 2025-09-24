// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Bitmaps;
using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;

#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 28_MousePointer.ls
    /// <summary>
    /// Recreates the animated custom mouse pointer from the Director project.
    /// </summary>
    public class MousePointer : BlingoParentScript, IHasStepFrameEvent
    {
        private int myNum;
        private float myOldX;
        private float myOldY;
        private int myAnimateNum;
        private int myStartMember;
        private int myNumberMembers;
        private int myDir;
        private List<IBlingoMember> myMembers = new();

        public MousePointer(IBlingoMovieEnvironment env) : base(env) { }

        /// <summary>
        /// Assigns the sprite number that will display the mouse and preloads its animation frames.
        /// </summary>
        public void Init(int num)
        {
            myNum = num;
            myStartMember = 80;
            myNumberMembers = 5;
            myAnimateNum = 0;
            myDir = 1;
            for (int i = 0; i <= 5; i++)
            {
                myMembers.Add(_Movie.GetMember<BlingoMemberBitmap>("mouse000" + i)!);
            }
            ShowMouse();
        }

        /// <summary>
        /// Called every frame to keep the pointer aligned with the hardware cursor.
        /// </summary>
        public void StepFrame() => Refresh();

        private void Refresh()
        {
            
            bool changed = false;
            if (myOldX != _Mouse.MouseH)
            {
                myOldX = _Mouse.MouseH;
                changed = true;
            }
            if (myOldY != _Mouse.MouseV)
            {
                myOldY = _Mouse.MouseV;
                changed = true;
            }
            if (changed)
            {
                //Console.WriteLine($"Mouse at {_Mouse.MouseH},{_Mouse.MouseV}");
                var sprite = Sprite(myNum);
                sprite.SetMember(myMembers[myAnimateNum]);
                
                if (myDir == 1)
                {
                    if (myAnimateNum < myNumberMembers)
                        myAnimateNum += 1;
                    else
                        myDir = -1;
                }
                else
                {
                    if (myAnimateNum > 0)
                        myAnimateNum -= 1;
                    else
                        myDir = 1;
                }
                sprite.LocH = _Mouse.MouseH + 20;
                sprite.LocV = _Mouse.MouseV + 15;
            }
        }

        /// <summary>
        /// Makes the custom cursor visible and hides the OS cursor.
        /// </summary>
        public void ShowMouse()
        {
            _Movie.PuppetSprite(myNum, true);
            Sprite(myNum).LocZ = 1000000;
            Sprite(myNum).SetMember(myMembers[0]);
            _Movie.ActorList.Add(this);
            Sprite(myNum).Visibility = true;
            _Mouse.SetCursor(AbstUI.Primitives.AMouseCursor.Hidden);
        }
        /// <summary>
        /// Hides the custom cursor and restores the native pointer.
        /// </summary>
        public void HideMouse()
        {
            //_Movie.PuppetSprite(myNum, false);
            _Movie.ActorList.Remove(this);
            Sprite(myNum).Visibility = false;
        }

        public void Mouse_Over() { }
        public void Mouse_Restore() { }

        /// <summary>
        /// Removes the pointer from the actor list. The sprite manager will reclaim the sprite separately.
        /// </summary>
        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
        }
    }
}

