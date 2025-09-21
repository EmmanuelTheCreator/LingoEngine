// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 7_Gfx.ls
    /// <summary>
    /// Responsible for translating grid coordinates into sprite positions.
    /// </summary>
    public class GfxScript : BlingoParentScript
    {
        private int myStartX;
        private int myStartY;
        /// <summary>
        /// Prepares the coordinate origin used to align the playfield with the background art.
        /// </summary>
        public GfxScript(IBlingoMovieEnvironment env) : base(env)
        {
            myStartX = 250;
            myStartY = 45;
        }

        /// <summary>
        /// Moves a sprite to the requested logical position, applying the origin offset.
        /// </summary>
        public void PositionBlock(int sprNum, int x, int y)
        {
            if (sprNum == 0) return;
            int xx = myStartX + x * 17;
            int yy = myStartY + y * 17;
            var spr = Sprite(sprNum);
            spr.LocH = xx;
            spr.LocV = yy;
        }

        /// <summary>
        /// Placeholder kept for parity with the original script.
        /// </summary>
        public void Destroy() { }
    }
}

