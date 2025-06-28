using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using LingoEngine.Texts;
using System.Collections.Generic;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 9_Blocks.ls
    public class BlocksParentScript : LingoParentScript, IHasStepFrameEvent
    {
        private readonly ILingoMovieEnvironment env;
        private readonly GlobalVars _global;
        private readonly GfxParentScript myGfx;
        private readonly ScoreManagerParentScript myScoreManager;
        private readonly List<List<BlockParentScript?>> myBlocks = new();
        private int myWidth;
        private int myHeight;
        private bool myFinishedBlocks;
        private int myFinishedBlocksHori;
        private int myFinishedBlocksVert;
        private int myNmbrTypes;

        public BlocksParentScript(ILingoMovieEnvironment env, GlobalVars global, GfxParentScript gfx, ScoreManagerParentScript score, int width, int height) : base(env)
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
            for (int j = 0; j < width; j++)
            {
                var col = new List<BlockParentScript?>();
                for (int i = 0; i < height; i++) col.Add(null);
                myBlocks.Add(col);
            }
            myWidth = width;
            myHeight = height;
        }

        private void CreateStartLines()
        {
            myNmbrTypes = 7;
            int NmbrLines = 0;
            var txt = Member<LingoMemberText>("T_StartLines");
            if (txt != null) int.TryParse(txt.Text, out NmbrLines);
            if (NmbrLines > myHeight - 5) NmbrLines = myHeight - 5;
            for (int yy = myHeight; yy > myHeight - NmbrLines; yy--)
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

        public bool NewBlock(int x, int y, int type)
        {
            if (y == 0) return false;
            if (myBlocks[x][y] != null) return false;
            var b = new BlockParentScript(env, _global, type);
            b.CreateBlock();
            myGfx.PositionBlock(b.GetSpriteNum(), x, y);
            myBlocks[x][y] = b;
            return true;
        }

        public bool IsBlock(int x, int y)
        {
            if (y <= 0 || x <= 0 || x > myWidth || y > myHeight) return true;
            return myBlocks[x][y] != null;
        }

        public bool FullHorizontal(int y)
        {
            for (int i = 1; i < myWidth; i++)
                if (myBlocks[i][y] == null)
                    return false;
            return true;
        }

        public void RemoveHorizontal(int y)
        {
            myScoreManager.LineRemoved();
            for (int i = 1; i < myWidth; i++)
            {
                var b = myBlocks[i][y];
                if (b != null)
                {
                    b.DestroyAnim();
                    myBlocks[i][y] = null;
                }
            }
            for (int j = y; j >= 2; j--)
            {
                for (int i = 1; i < myWidth; i++)
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
            if (FullHorizontal(y)) RemoveHorizontal(y);
        }

        public void FinishedBlocks()
        {
            myFinishedBlocks = true;
            myFinishedBlocksVert = 1;
            myFinishedBlocksHori = 1;
            _Movie.ActorList.Add(this);
        }

        public void StepFrame()
        {
            if (!myFinishedBlocks) return;
            for (; myFinishedBlocksHori <= myBlocks.Count; myFinishedBlocksHori++)
            {
                if (myFinishedBlocksVert <= myBlocks[myFinishedBlocksHori - 1].Count)
                {
                    var b = myBlocks[myFinishedBlocksHori - 1][myFinishedBlocksVert - 1];
                    b?.FinishBlock();
                }
                else
                {
                    myFinishedBlocks = false;
                    _Movie.ActorList.Remove(this);
                    break;
                }
            }
            myFinishedBlocksVert += 1;
        }

        public void DestroyBlocks()
        {
            foreach (var col in myBlocks)
                foreach (var b in col)
                    b?.Destroy();
        }

        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
            DestroyBlocks();
        }
    }
}
