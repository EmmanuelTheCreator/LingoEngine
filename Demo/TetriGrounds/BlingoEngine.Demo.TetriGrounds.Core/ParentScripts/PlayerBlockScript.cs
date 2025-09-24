// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using System.Collections.Generic;
using System.Reflection.Emit;
#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 8_PlayerBlock.ls (simplified)
    /// <summary>
    /// Controls the active tetromino, handling keyboard input, collision checks and spawning new pieces.
    /// </summary>
    public class PlayerBlockScript : BlingoParentScript, IHasStepFrameEvent
    {
        private readonly IBlingoMovieEnvironment env;
        private readonly GlobalVars _global;
        private readonly GfxScript myGfx;
        private readonly BlocksScript myBlocks;
        private readonly ScoreManagerScript myScoreManager;
        private readonly List<Dictionary<string, object>> MySubBlocks = new();
        private readonly List<Dictionary<string, object>> MyNextBlocks = new();
        private readonly List<object[]> myTypeBlocks = new();
        private int myX;
        private int myY;
        private int myWidth;
        private int myMaxX;
        private int myMaxY;
        private bool myPause;
        private bool myMoving;
        private int _level;
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
        private DateTime _currentBlockStartedAt;

        /// <summary>
        /// Prepares the controller with references to supporting services and seeds the random piece list.
        /// </summary>
        public PlayerBlockScript(IBlingoMovieEnvironment env, GlobalVars global, GfxScript gfx, BlocksScript blocks, ScoreManagerScript score, int width, int height) : base(env)
        {
            this.env = env;
            _global = global;
            myGfx = gfx;
            myBlocks = blocks;
            myScoreManager = score;
            myMaxX = width;
            myMaxY = height;
           
            AddTypeBlock(new int[,] { { 0,0 }, { -1,0 }, { 1,0 }, { 0,-1 } }, true);    // 'T' Block, true-false = rotate(else its flip axis)
            AddTypeBlock(new int[,] { { 0,0 }, { -1,0 }, { -2,0 }, { 1,0 } }, true);    // '-' Block
            AddTypeBlock(new int[,] { { -1,-1 }, { 0,-1 }, { 0,0 }, { 1,0 } }, false);  // 'z' Block
            AddTypeBlock(new int[,] { { 1,-1 }, { 0,-1 }, { 0,0 }, { -1,0 } }, false);  // 'z' Block invers
            AddTypeBlock(new int[,] { { 0,-1 }, { 1,-1 }, { 0,0 }, { 1,0 } }, false);   // '#' Block Cubus
            AddTypeBlock(new int[,] { { -1,-1 }, { -1,0 }, { 0,0 }, { 1,0 } }, true);   // 'L' Block 
            AddTypeBlock(new int[,] { { 1,-1 }, { -1,0 }, { 0,0 }, { 1,0 } }, true);    // 'L' Block invers
            myX = myMaxX / 2;
            myY = 2;
            myWidth = 1;
            myMoving = false;
            _level = myScoreManager.GetLevel();

            CalculateSpeed();
            if (mySlowDown <= 1) mySlowDown = 1;
            myWaiter = 0;
            myKeyBoardWaiter = 0;
            myKeyBoardTot = 11;
            myNextBlockType = 1;
            MyNextBlockHor = 16;
            MyNextBlockVer = 13;
            mySlowDownFactorByLevel = 10;
            UpdateNextBlock();
            StartMove();
            myStopKeyAction = false;
            HidePause();
        }

        private void CalculateSpeed()
        {
            _level = myScoreManager.GetLevel();
            mySlowDownFactorByLevel = 10;
            mySlowDown = 65;
            for (int i = 0; i <= _level; i++)
            {
                mySlowDownFactorByLevel -= 1;
                if (mySlowDownFactorByLevel <= 3) mySlowDownFactorByLevel = 2;
                if (mySlowDown > 1) mySlowDown -= mySlowDownFactorByLevel;
            }
            if (mySlowDown <= 1) mySlowDown = 1;
        }

        /// <summary>
        /// Entry point for keyboard events coming from <see cref="BgScriptBehavior"/>.
        /// </summary>
        public void Keyyed(int val)
        {
            if (myPause) return;
            myKeyBoardWaiter = 0;
            myLastKey = val;
            MoveBlock(val);
        }

        /// <summary>
        /// Toggles the pause overlay while keeping the game state intact.
        /// </summary>
        public void PauseGame()
        {
            if (myPause)
                HidePause();
            else
            {
                Sprite(35).Blend = 100;
                Sprite(35).Visibility = true;
                myPause = false;
                Sprite(35).LocZ = 1010;
                myPause = true;
            }
        }
        /// <summary>
        /// Ensures the pause overlay is hidden, usually when starting a fresh game.
        /// </summary>
        internal void HidePause()
        {
            Sprite(35).Blend = 0;
            Sprite(35).Visibility = false;
            myPause = false;
            Sprite(35).LocZ = 35;
        }
        private void MoveBlock(int val)
        {
            if (myFinished) return;
            // Right
            if (val == 2) { 
                Rightt(); 
                myDownPressed = false; 
            }
            else if (val == 4) 
            { 
                Leftt(); 
                myDownPressed = false; 
            }
            else if (val == 3)
            {
                if (!myStopKeyAction) 
                    // Down
                    myDownPressed = true; 
                else 
                    myDownPressed = false;
            }
            else if (val == 9) 
            { 
                // nothing
                myDownPressed = false; 
                myStopKeyAction = false; 
            }
            else if (val == 1) 
            { 
                // up
                TurnBlock(); 
                myDownPressed = false; 
            }
        }

        /// <summary>
        /// Hard drop: moves the active block down until a collision is detected.
        /// </summary>
        public void LetBlockFall()
        {
            int starting = myY;
            for (int i = starting; i <= myMaxY; i++)
            {
                bool test = DownCheck(myY, true);
                RefreshBlock();
                if (!test) break;
            }
        }

        /// <summary>
        /// Advances the block based on input repeat rate and gravity.
        /// </summary>
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
                DownCheck(myY,false);
                RefreshBlock();
            }
            else
            {
                myWaiter += 1;
            }
        }

        private bool DownCheck(int rowsLeft, bool hardDrop)
        {
            bool check = CollitionDetect(myX, myY + 1);
            if (check)
            {
                _Player.SoundPlayBlockDown((int)Math.Floor((float)((float)rowsLeft / myMaxY) *10));
                FreezeBlock();
                ResetBlock(hardDrop);
                return false;
            }
            else
            {
                myY += 1;
                return true;
            }
        }

        private void ResetBlock(bool hardDrop)
        {
            if (myFinished)
                return;

            var dropDuration = DateTime.UtcNow - _currentBlockStartedAt;
            var rowsTravelled = Math.Max(0, myY - 2);

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
            myScoreManager.BlockFrozen(dropDuration, rowsTravelled);// add score when you freeze a block
            myScoreManager.AddDropedBlock(hardDrop); // add that there's a block dropped

            // check if we go a level up
            if (myScoreManager.GetLevelUp())
            {
                _Player.SoundPlayGong();
                _Player.SoundPlayLevelUp();
                CalculateSpeed();
            }
            if (CollitionDetect(myX, myY))
            {
                // Game is teriinated
                GameTerminated();
            }
        }

        /// <summary>
        /// Handles the game-over sequence by showing UI, restoring sounds and resetting globals.
        /// </summary>
        private void GameTerminated()
        {
            
            myScoreManager.GameFinished();
            myFinished = true;
            myBlocks.FinishedBlocks();
            StopMove();
            _global.GameIsRunning = false;
            _global.MousePointer!.ShowMouse();
            Sprite(9).Visibility = true; // show start button
            Sprite(11).Visibility = true; // show start button

            // play sounds
            _Player.SoundPlayDied();
            _Player.RunDelayed(_Player.SoundPlayNature, 900);
            _Player.RunDelayed(_Player.SoundPlayTerminated, 500);
            //SendSprite<AppliBgBehavior>(1, s => s.GameFinished(myScoreManager.GetScore()));
        }

        private void FreezeBlock()
        {
            if (myFinished)
                return;
            
            foreach (var i in MySubBlocks)
            {
                myBlocks.NewBlock(myX + (int)i["xx"], myY + (int)i["yy"], myBlockType);
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
        // ----------------------------------------
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
        // ----------------------------------------
        private void StartMove()
        {
            _Movie.ActorList.Add(this);
            myMoving = true;
        }

        private void StopMove()
        {
            _Movie.ActorList.Remove(this);
            myMoving = false;
        }
        // ----------------------------------------
        private void RefreshBlock()
        {
            for (int i = 0; i < MySubBlocks.Count; i++)
            {
                var obj = (BlockScript)MySubBlocks[i]["obj"];
                myGfx.PositionBlock(obj.GetSpriteNum(), myX + (int)MySubBlocks[i]["xx"], myY + (int)MySubBlocks[i]["yy"]);
            }
        }
        // ----------------------------------------
        /// <summary>
        /// Creates the sub-sprites that make up the active tetromino and positions them correctly.
        /// </summary>
        public void CreateBlock()
        {
            myBlockType = myNextBlockType;
            var chosen = (object[])myTypeBlocks[myBlockType - 1];
            // create subBlocks
            var coords = (List<(int x, int y)>)chosen[0];
            for (int i = 0; i < coords.Count; i++)
            {
                var dict = new Dictionary<string, object>();
                var b = new BlockScript(env, _global, myBlockType);
                b.CreateBlock();
                dict["obj"] = b;
                dict["xx"] = coords[i].x;
                dict["yy"] = coords[i].y;
                MySubBlocks.Add(dict);
            }
            RefreshBlock();
            UpdateNextBlock();
            _currentBlockStartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Randomises and renders the next piece preview.
        /// </summary>
        private void UpdateNextBlock()
        {
            DestroyNextBlock();
            myNextBlockType = Random(myTypeBlocks.Count);
            var chosen = (object[])myTypeBlocks[myNextBlockType - 1];
            var coords = (List<(int x, int y)>)chosen[0];
            foreach (var p in coords)
            {
                var dict = new Dictionary<string, object>();
                var b = new BlockScript(env, _global, myNextBlockType);
                b.CreateBlock();
                dict["obj"] = b;
                dict["xx"] = p.x;
                dict["yy"] = p.y;
                MyNextBlocks.Add(dict);
                myGfx.PositionBlock(b.GetSpriteNum(), MyNextBlockHor + p.x, MyNextBlockVer + p.y);
            }
        }

        /// <summary>
        /// Removes the preview sprites so the list can be reused for the next selection.
        /// </summary>
        private void DestroyNextBlock()
        {
            foreach (var d in MyNextBlocks)
            {
                ((BlockScript)d["obj"]).Destroy();
            }
            MyNextBlocks.Clear();
        }

        /// <summary>
        /// Reports the current pause state so other behaviours can check it before toggling.
        /// </summary>
        public bool GetPause() => myPause;

        /// <summary>
        /// Releases the sprites that compose the active block.
        /// </summary>
        private void DestroyBlock()
        {
            foreach (var d in MySubBlocks)
            {
                ((BlockScript)d["obj"]).Destroy();
            }
            MySubBlocks.Clear();
        }

        /// <summary>
        /// Registers a tetromino configuration, mirroring the arrays from the original scripts.
        /// </summary>
        private void AddTypeBlock(int[,] coords, bool rotate)
        {
            var list = new List<(int x, int y)>();
            for (int i = 0; i < coords.GetLength(0); i++)
                list.Add((coords[i, 0], coords[i, 1]));
            myTypeBlocks.Add(new object[] { list, rotate });
        }

        /// <summary>
        /// Cleans up resources so the playfield can be torn down safely.
        /// </summary>
        public void Destroy()
        {
            DestroyNextBlock();
            DestroyBlock();
            StopMove();
        }

      
    }
}

