// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;

#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 10_Block.ls
    /// <summary>
    /// Represents a single block tile on the playfield, mirroring the Lingo parent script that animated destruction.
    /// </summary>
    public class BlockScript : BlingoParentScript, IHasStepFrameEvent
    {
        private readonly GlobalVars _global;
        private string myMember = "Block1";
        private int myNum;
        private bool myDestroyAnim;
        private int myMemberNumAnim;

        /// <summary>
        /// Chooses a sprite member based on the requested block type and remembers the global state reference.
        /// </summary>
        public BlockScript(IBlingoMovieEnvironment env, GlobalVars global, int chosenType = 1) : base(env)
        {
            _global = global;
            string[] members = { "Block1", "Block2", "Block3", "Block4", "Block5", "Block6", "Block7" };
            if (chosenType >= 1 && chosenType <= members.Length) myMember = members[chosenType - 1];
        }

        /// <summary>
        /// Drives the destruction animation while the block is being removed.
        /// </summary>
        public void StepFrame()
        {
            if (myDestroyAnim)
            {
                if (myMemberNumAnim > 7)
                {
                    myMemberNumAnim = 0;
                    _Movie.ActorList.Remove(this);
                    Destroy();
                    return;
                }
                myMemberNumAnim += 1;
                Sprite(myNum).SetMember("Destroy" + myMemberNumAnim);
            }
        }

        /// <summary>
        /// Starts the block's destruction animation by keeping the actor alive in the list.
        /// </summary>
        public void DestroyAnim()
        {
            myDestroyAnim = true;
            myMemberNumAnim = 0;
            if (_Movie.ActorList.GetPos(this)==0)
                _Movie.ActorList.Add(this);
        }

        /// <summary>
        /// Forces the block to display its destroyed appearance without animation.
        /// </summary>
        public void FinishBlock()
        {
            myMember = "Destroy1";
            Sprite(myNum).SetMember(myMember);
        }

        /// <summary>
        /// Reserves a sprite from the sprite manager and shows the block member.
        /// </summary>
        public void CreateBlock()
        {
            myNum = _global.SpriteManager?.Sadd() ?? 0;

            var spr = Sprite(myNum);
            spr.SetMember(myMember);
            spr.Ink = 36;
        }

        /// <summary>
        /// Returns the sprite number associated with this block.
        /// </summary>
        public int GetSpriteNum() => myNum;

        /// <summary>
        /// Removes the block from the actor list and releases the sprite number back to the manager.
        /// </summary>
        public void Destroy()
        {
            if (_Movie.ActorList.GetPos(this) != 0)
                _Movie.ActorList.Remove(this);
            _global.SpriteManager?.SDestroy(myNum);
        }
    }
}

