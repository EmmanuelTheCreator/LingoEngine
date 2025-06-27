using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using LingoEngine.Movies;
using LingoEngine.Movies.Events;
using System.Collections.Generic;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 8_PlayerBlock.ls (simplified)
    public class PlayerBlockParentScript : LingoParentScript, IHasStepFrameEvent
    {
        private readonly ILingoMovieEnvironment env;
        private readonly GlobalVars _global;
        private readonly GfxParentScript myGfx;
        private readonly BlocksParentScript myBlocks;
        private readonly ScoreManagerParentScript myScoreManager;
        private readonly List<Dictionary<string, object>> MySubBlocks = new();
        private readonly List<Dictionary<string, object>> MyNextBlocks = new();
        private readonly List<object[]> myTypeBlocks = new();
        private int myX;
        private int myY;
        private int myMaxX;
        private int myMaxY;
        private bool myPause;
        private bool myMoving;
        private int mySlowDown = 65;
        private int myWaiter;
        private int myKeyBoardWaiter;
        private int myKeyBoardTot = 11;
        private int mySlowDownFactorByLevel = 10;
        private bool myFinished;
        private int myBlockType;
        private int myNextBlockType = 1;
        private int MyNextBlockHor = 16;
        private int MyNextBlockVer = 13;
        private bool myDownPressed;
        private bool myStopKeyAction;
        private int myLastKey;

        public PlayerBlockParentScript(ILingoMovieEnvironment env, GlobalVars global, GfxParentScript gfx, BlocksParentScript blocks, ScoreManagerParentScript score, int width, int height) : base(env)
        {
            this.env = env;
            _global = global;
            myGfx = gfx;
            myBlocks = blocks;
            myScoreManager = score;
            myMaxX = width;
            myMaxY = height;
            myX = myMaxX / 2;
            myY = 2;
            AddTypeBlock(new int[,] { { 0,0 }, { -1,0 }, { 1,0 }, { 0,-1 } }, true);
            AddTypeBlock(new int[,] { { 0,0 }, { -1,0 }, { -2,0 }, { 1,0 } }, true);
            AddTypeBlock(new int[,] { { -1,-1 }, { 0,-1 }, { 0,0 }, { 1,0 } }, false);
            AddTypeBlock(new int[,] { { 1,-1 }, { 0,-1 }, { 0,0 }, { -1,0 } }, false);
            AddTypeBlock(new int[,] { { 0,-1 }, { 1,-1 }, { 0,0 }, { 1,0 } }, false);
            AddTypeBlock(new int[,] { { -1,-1 }, { -1,0 }, { 0,0 }, { 1,0 } }, true);
            AddTypeBlock(new int[,] { { 1,-1 }, { -1,0 }, { 0,0 }, { 1,0 } }, true);
            CalculateSpeed();
            UpdateNextBlock();
            StartMove();
        }

        private void CalculateSpeed()
        {
            int lvl = myScoreManager.GetLevel();
            mySlowDownFactorByLevel = 10;
            mySlowDown = 65;
            for (int i = 0; i <= lvl; i++)
            {
                mySlowDownFactorByLevel -= 1;
                if (mySlowDownFactorByLevel <= 3) mySlowDownFactorByLevel = 2;
                if (mySlowDown > 1) mySlowDown -= mySlowDownFactorByLevel;
            }
            if (mySlowDown <= 1) mySlowDown = 1;
        }

        public void Keyyed(int val)
        {
            if (myPause) return;
            myKeyBoardWaiter = 0;
            myLastKey = val;
            MoveBlock(val);
        }

        public void PauseGame()
        {
            if (myPause)
            {
                Sprite(35).Blend = 0;
                myPause = false;
            }
            else
            {
                Sprite(35).Blend = 100;
                Sprite(35).LocZ = 10010;
                myPause = true;
            }
        }

        private void MoveBlock(int val)
        {
            if (myFinished) return;
            if (val == 2) { Rightt(); myDownPressed = false; }
            else if (val == 4) { Leftt(); myDownPressed = false; }
            else if (val == 3)
            {
                if (!myStopKeyAction) myDownPressed = true; else myDownPressed = false;
            }
            else if (val == 9) { myDownPressed = false; myStopKeyAction = false; }
            else if (val == 1) { TurnBlock(); myDownPressed = false; }
        }

        public void LetBlockFall()
        {
            int starting = myY;
            for (int i = starting; i <= myMaxY; i++)
            {
                bool test = DownCheck();
                RefreshBlock();
                if (!test) break;
            }
        }

        public void StepFrame()
        {
            if (myPause) return;
            if (myLastKey <= 4 && myLastKey != 1)
            {
                if (myKeyBoardWaiter > myKeyBoardTot)
                {
                    myKeyBoardWaiter = 0;
                    MoveBlock(myLastKey);
                }
                myKeyBoardWaiter += 1;
            }
            int addon = myDownPressed ? mySlowDown : 0;
            if (myWaiter + addon > mySlowDown)
            {
                myWaiter = 0;
                DownCheck();
                RefreshBlock();
            }
            else
            {
                myWaiter += 1;
            }
        }

        private bool DownCheck()
        {
            bool check = CollitionDetect(myX, myY + 1);
            if (check)
            {
                FreezeBlock();
                ResetBlock();
                return false;
            }
            else
            {
                myY += 1;
                return true;
            }
        }

        private void ResetBlock()
        {
            if (!myFinished)
            {
                foreach (var i in MySubBlocks)
                {
                    int y = (int)i["yy"] + myY;
                    if (myBlocks.FullHorizontal(y))
                        myBlocks.RemoveHorizontal(y);
                }
                myStopKeyAction = true;
                DestroyBlock();
                CreateBlock();
                myY = 2;
                myX = myMaxX / 2;
                myScoreManager.BlockFrozen();
                myScoreManager.AddDropedBlock();
                if (myScoreManager.GetLevelUp()) CalculateSpeed();
                if (CollitionDetect(myX, myY))
                {
                    myScoreManager.GameFinished();
                    myFinished = true;
                    myBlocks.FinishedBlocks();
                    StopMove();
                    SendSprite<AppliBgBehavior>(1, s => s.GameFinished(myScoreManager.GetScore()));
                }
            }
        }

        private void FreezeBlock()
        {
            if (!myFinished)
            {
                foreach (var i in MySubBlocks)
                {
                    myBlocks.NewBlock(myX + (int)i["xx"], myY + (int)i["yy"], myBlockType);
                }
            }
        }

        private bool CollitionDetect(int x, int y)
        {
            foreach (var i in MySubBlocks)
            {
                if (myBlocks.IsBlock(x + (int)i["xx"], y + (int)i["yy"]))
                    return true;
            }
            return false;
        }

        private void TurnBlock()
        {
            myWaiter += 1;
            int offsetX = 0;
            var tempBlock = new List<(int x,int y)>();
            bool coll = false;
            foreach (var i in MySubBlocks)
            {
                int oldx = (int)i["xx"];
                int oldy = (int)i["yy"];
                int newy = oldx;
                int newx = -oldy;
                tempBlock.Add((newx, newy));
                if (myX + newx > myMaxX - 1) coll = true;
                if (myBlocks.IsBlock(myX + newx + offsetX, myY + newy)) coll = true;
            }
            if (!coll)
            {
                for (int idx = 0; idx < MySubBlocks.Count; idx++)
                {
                    MySubBlocks[idx]["xx"] = tempBlock[idx].x;
                    MySubBlocks[idx]["yy"] = tempBlock[idx].y;
                }
            }
            myX += offsetX;
            RefreshBlock();
        }

        private void Rightt()
        {
            myWaiter += 1;
            if (!CollitionDetect(myX + 1, myY))
            {
                int maxright = 0;
                foreach (var i in MySubBlocks)
                {
                    int tempx = (int)i["xx"];
                    if (tempx > maxright) maxright = tempx;
                }
                if (myX + maxright + 1 < myMaxX)
                {
                    myX += 1;
                    RefreshBlock();
                }
            }
        }

        private void Leftt()
        {
            myWaiter += 1;
            if (!CollitionDetect(myX - 1, myY))
            {
                int maxleft = 0;
                foreach (var i in MySubBlocks)
                {
                    int tempx = (int)i["xx"];
                    if (tempx < maxleft) maxleft = tempx;
                }
                if (myX - 1 + maxleft > 0)
                {
                    myX -= 1;
                    RefreshBlock();
                }
            }
        }

        private void StartMove() => _Movie.ActorList.Add(this);
        private void StopMove() => _Movie.ActorList.Remove(this);

        private void RefreshBlock()
        {
            for (int i = 0; i < MySubBlocks.Count; i++)
            {
                var obj = (BlockParentScript)MySubBlocks[i]["obj"];
                myGfx.PositionBlock(obj.GetSpriteNum(), myX + (int)MySubBlocks[i]["xx"], myY + (int)MySubBlocks[i]["yy"]);
            }
        }

        public void CreateBlock()
        {
            myBlockType = myNextBlockType;
            var chosen = (object[])myTypeBlocks[myBlockType - 1];
            var coords = (List<(int x, int y)>)chosen[0];
            for (int i = 0; i < coords.Count; i++)
            {
                var dict = new Dictionary<string, object>();
                var b = new BlockParentScript(env, _global, myBlockType);
                b.CreateBlock();
                dict["obj"] = b;
                dict["xx"] = coords[i].x;
                dict["yy"] = coords[i].y;
                MySubBlocks.Add(dict);
            }
            RefreshBlock();
            UpdateNextBlock();
        }

        private void UpdateNextBlock()
        {
            DestroyNextBlock();
            myNextBlockType = Random(myTypeBlocks.Count);
            var chosen = (object[])myTypeBlocks[myNextBlockType - 1];
            var coords = (List<(int x, int y)>)chosen[0];
            foreach (var p in coords)
            {
                var dict = new Dictionary<string, object>();
                var b = new BlockParentScript(env, _global, myNextBlockType);
                b.CreateBlock();
                dict["obj"] = b;
                dict["xx"] = p.x;
                dict["yy"] = p.y;
                MyNextBlocks.Add(dict);
                myGfx.PositionBlock(b.GetSpriteNum(), MyNextBlockHor + p.x, MyNextBlockVer + p.y);
            }
        }

        private void DestroyNextBlock()
        {
            foreach (var d in MyNextBlocks)
            {
                ((BlockParentScript)d["obj"]).Destroy();
            }
            MyNextBlocks.Clear();
        }

        public bool GetPause() => myPause;

        private void DestroyBlock()
        {
            foreach (var d in MySubBlocks)
            {
                ((BlockParentScript)d["obj"]).Destroy();
            }
            MySubBlocks.Clear();
        }

        private void AddTypeBlock(int[,] coords, bool rotate)
        {
            var list = new List<(int x, int y)>();
            for (int i = 0; i < coords.GetLength(0); i++)
                list.Add((coords[i, 0], coords[i, 1]));
            myTypeBlocks.Add(new object[] { list, rotate });
        }

        public void Destroy()
        {
            DestroyNextBlock();
            DestroyBlock();
            StopMove();
        }
    }
}
