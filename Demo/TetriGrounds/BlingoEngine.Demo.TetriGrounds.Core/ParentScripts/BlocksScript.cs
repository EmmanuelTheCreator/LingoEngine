// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Primitives;
using BlingoEngine.Texts;
using System.Collections.Generic;
#pragma warning disable IDE1006 // Naming Styles


namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 9_Blocks.ls
    /// <summary>
    /// Manages the playfield grid, handles line clearing and keeps track of all spawned block parent scripts.
    /// </summary>
    public class BlocksScript : BlingoParentScript, IHasStepFrameEvent
    {
        private readonly IBlingoMovieEnvironment env;
        private readonly GlobalVars _global;
        private readonly GfxScript myGfx;
        private readonly ScoreManagerScript myScoreManager;
        private readonly BlingoList<BlingoList<BlockScript?>> myBlocks = new();
        private int myWidth;
        private int myHeight;
        private bool myFinishedBlocks;
        private int myFinishedBlocksHori;
        private int myFinishedBlocksVert;
        private int myNmbrTypes;

        /// <summary>
        /// Creates a grid controller with the requested dimensions and seeds the initial garbage lines.
        /// </summary>
        public BlocksScript(IBlingoMovieEnvironment env, GlobalVars global, GfxScript gfx, ScoreManagerScript score, int width, int height) : base(env)
        {
            this.env = env;
            _global = global;
            myGfx = gfx;
            myScoreManager = score;
            InitGrid(width, height);
            CreateStartLines();
        }

        private void InitGrid(int width, int height)
        {
            myBlocks.Clear();
            for (int j = 1; j <= width; j++)
            {
                var col = new BlingoList<BlockScript?>();
                for (int i = 1; i <= height; i++) col.Add(null);
                myBlocks.Add(col);
            }
            myWidth = width;
            myHeight = height;
        }

        private void CreateStartLines()
        {
            myNmbrTypes = 7;
            int nmbrLines = 0;
            var txt = Member<BlingoMemberText>("T_StartLines");
            if (txt != null) int.TryParse(txt.Text, out nmbrLines);
            if (nmbrLines > myHeight - 5) nmbrLines = myHeight - 5;
            //nmbrLines = 1; // for testing
            for (int yy = myHeight; yy > myHeight - nmbrLines; yy--)
            {
                bool empty = false;
                for (int xx = 1; xx < myWidth; xx++)
                {
                    int type = Random(myNmbrTypes + myNmbrTypes);
                    if (!empty && xx == myWidth - 1) type = myNmbrTypes + 1;
                    if (type > myNmbrTypes) empty = true;
                    else NewBlock(xx, yy, type);
                }
            }
        }

        /// <summary>
        /// Spawns a new block of the given type at the provided grid location.
        /// </summary>
        public bool NewBlock(int x, int y, int type)
        {
            if (y == 0) return false;
            if (myBlocks[x][y] != null) return false;
            var b = new BlockScript(env, _global, type);
            b.CreateBlock();
            myGfx.PositionBlock(b.GetSpriteNum(), x, y);
            myBlocks[x][y] = b;
            return true;
        }

        /// <summary>
        /// Checks whether a block occupies the supplied coordinate. Out-of-bounds counts as solid.
        /// </summary>
        public bool IsBlock(int x, int y)
        {
            if (y <= 0 || x <= 0 || x > myWidth || y > myHeight) return true;
            return myBlocks[x][y] != null;
        }

        /// <summary>
        /// Determines if the specified row is completely filled.
        /// </summary>
        public bool FullHorizontal(int y)
        {
            for (int i = 1; i < myWidth; i++)
            {
                var block = myBlocks[i][y];
                if (block == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Clears a full row, plays the classic sound effect and shifts blocks down.
        /// </summary>
        public void RemoveHorizontal(int y)
        {

            _Player.SoundPlayRemoveRow();
            myScoreManager.LineRemoved();
            for (int i = 1; i <= myWidth; i++)
            {
                var b = myBlocks[i][y];
                if (b != null)
                {
                    b.DestroyAnim();
                    myBlocks[i][y] = null;
                }
            }
            //  fall down all the blocks above
            for (int j = y; j >= 2; j--)
            {
                for (int i = 1; i <= myWidth; i++)
                {
                    var up = myBlocks[i][j - 1];
                    if (up != null)
                    {
                        myBlocks[i][j] = up;
                        myGfx.PositionBlock(up.GetSpriteNum(), i, j);
                        myBlocks[i][j - 1] = null;
                    }
                }
            }
            //  redo an horizontalcheck
            if (FullHorizontal(y)) RemoveHorizontal(y);
        }

        // function that is executed on the end of the game
        public void FinishedBlocks()
        {
            myFinishedBlocks = true;
            myFinishedBlocksVert = 1;
            myFinishedBlocksHori = 1;
            _Movie.ActorList.Add(this);
            
        }

        /// <summary>
        /// Performs the staggered highlight animation once the game has ended.
        /// </summary>
        public void StepFrame()
        {
            if (!myFinishedBlocks) return;
            if (myFinishedBlocksVert > myBlocks[myFinishedBlocksHori].Count)
            {
                myFinishedBlocks = false;
                _Movie.ActorList.Remove(this);
                return;
            }
            for (int i = 0; i < myBlocks.Count; i++)
            {
                myFinishedBlocksHori = i+1;
                var b = myBlocks[myFinishedBlocksHori][myFinishedBlocksVert];
                b?.FinishBlock();
            }
            myFinishedBlocksVert += 1;
        }

        /// <summary>
        /// Destroys all tracked block instances and clears the internal grid representation.
        /// </summary>
        public void DestroyBlocks()
        {
            var clone = myBlocks.ToArray();
            foreach (var col in clone)
            {
                if (col == null) continue;
                var cloneCol = col.ToArray();
                foreach (BlockScript? b in cloneCol)
                {
                    b?.Destroy();
                    var index = col.IndexOf(b);
                    col[index] = null;
                }
            }
        }

        /// <summary>
        /// Removes the controller from the actor list and clears all blocks.
        /// </summary>
        public void Destroy()
        {
            if (_Movie.ActorList.GetPos(this) != 0)
                _Movie.ActorList.Remove(this);
            DestroyBlocks();
        }
    }
}

