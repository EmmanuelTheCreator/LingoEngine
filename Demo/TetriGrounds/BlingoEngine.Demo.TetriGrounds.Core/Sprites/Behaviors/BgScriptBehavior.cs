// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using BlingoEngine.Events;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.VerboseLanguage;
using static System.Runtime.InteropServices.JavaScript.JSType;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 2_Bg Script.ls
    /// <summary>
    /// Central coordinator for the gameplay scene. It bridges inputs, player state and score management.
    /// </summary>
    public class BgScriptBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, IHasEndSpriteEvent
    {
        private PlayerBlockScript? myPlayerBlock;
        private GfxScript? myGfx;
        private BlocksScript? myBlocks;
        private ScoreManagerScript? myScoreManager;
        private readonly GlobalVars _global;
        private readonly ScoresRepository _scoresRepository;
        private int myWidth;
        private int myHeight;
        /// <summary>
        /// Stores references to the global state and repository used across the game.
        /// </summary>
        public BgScriptBehavior(IBlingoMovieEnvironment env, GlobalVars global, ScoresRepository scoresRepository) : base(env)
        {
            _global = global;
            _scoresRepository = scoresRepository;
        }

        /// <summary>
        /// Called when the sprite activates; ensures the player block is ready and plays a gong.
        /// </summary>
        public void BeginSprite()
        {
            if (myPlayerBlock != null)
                myPlayerBlock.HidePause();
            _Player.SoundPlayGong();

        }

        public void ExitFrame()
        {
        }

        /// <summary>
        /// Debug helper that prints messages using the verbose language DSL.
        /// </summary>
        public void ActionKey(object val)
        {
            // debug output
            this.Put(val).ToLog();
        }

        /// <summary>
        /// Forwards key presses to the player block so it can react.
        /// </summary>
        public void KeyAction(int val, int val2)
        {
            if (myPlayerBlock == null) return;
            myPlayerBlock.Keyyed(val);
        }

        /// <summary>
        /// Toggles the pause state in the player controller.
        /// </summary>
        public void PauseGame() => myPlayerBlock?.PauseGame();

        /// <summary>
        /// Starts a new game or restarts the current one when possible.
        /// </summary>
        public void NewGame()
        {
            if (myPlayerBlock != null)
            {
                var _pause = myPlayerBlock.GetPause();
                if(_pause==false) {
                    TeminateGame();
                    StartNewGame();
                }
            }
            else
            {
                TeminateGame();
                StartNewGame();
            }
        }

        /// <summary>
        /// Ensures all temporary state is cleared when the sprite leaves the stage.
        /// </summary>
        public void EndSprite()
        {
            TeminateGame();
        }

        /// <summary>
        /// Performs all the setup required to start an interactive TetriGrounds session.
        /// </summary>
        public void StartNewGame()
        {
            _global.GameIsRunning = true;
            if (_global.SpriteManager == null)
            {
                _global.SpriteManager = new SpriteManager(_env);
                _global.SpriteManager.Init(100);
            }
            myWidth = 11;
            myHeight = 22;
            myGfx = new GfxScript(_env);
            myScoreManager = new ScoreManagerScript(_env, _global, _scoresRepository);
            myBlocks = new BlocksScript(_env, _global, myGfx, myScoreManager, myWidth, myHeight);
            myPlayerBlock = new PlayerBlockScript(_env, _global, myGfx, myBlocks, myScoreManager, myWidth, myHeight);
            myPlayerBlock.CreateBlock();
        }

        /// <summary>
        /// Tears down the active game so the scene can return to the menu safely.
        /// </summary>
        public void TeminateGame()
        {
            _global.GameIsRunning = false;
            myPlayerBlock?.Destroy();
            myBlocks?.Destroy();
            myGfx?.Destroy();
            myScoreManager?.Destroy();
            myPlayerBlock = null;
            myBlocks = null;
            myGfx = null;
            myScoreManager = null;
        }

        /// <summary>
        /// Performs a hard drop when the space bar is pressed.
        /// </summary>
        public void SpaceBar() => myPlayerBlock?.LetBlockFall();

       


       
    }
}

