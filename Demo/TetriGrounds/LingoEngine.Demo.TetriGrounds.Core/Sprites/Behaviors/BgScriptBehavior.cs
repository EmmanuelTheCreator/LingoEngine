using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Demo.TetriGrounds.Core.ParentScripts;

namespace LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 2_Bg Script.ls
    public class BgScriptBehavior : LingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, IHasLingoMessage
    {
        private PlayerBlockParentScript? myPlayerBlock;
        private GfxParentScript? myGfx;
        private BlocksParentScript? myBlocks;
        private ScoreManagerParentScript? myScoreManager;
        private readonly GlobalVars _global;
        ILingoMovieEnvironment _env;
        public BgScriptBehavior(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _env = env;
            _global = global;
        }

        public void BeginSprite()
        {
            // if no player block hide a sprite (not implemented)
        }

        public void ExitFrame()
        {
        }

        public void ActionKey(object val)
        {
            // debug output
            Put(val);
        }

        public void KeyAction(int val)
        {
            myPlayerBlock?.Keyyed(val);
        }

        public void PauseGame() => myPlayerBlock?.PauseGame();

        public void NewGame()
        {
            if (myPlayerBlock != null)
            {
                // var _pause = myPlayerBlock.GetPause()
                // if(_pause==false) { teminateGame(); StartNewGame(); }
            }
            else
            {
                TeminateGame();
                StartNewGame();
            }
        }

        public void StartNewGame()
        {
            if (_global.SpriteManager == null)
            {
                _global.SpriteManager = new SpriteManagerParentScript(_env);
                _global.SpriteManager.Init(100);
            }
            myWidth = 11;
            myHeight = 22;
            myGfx = new GfxParentScript(_env);
            myScoreManager = new ScoreManagerParentScript(_env, _global);
            myBlocks = new BlocksParentScript(_env, _global, myGfx, myScoreManager, myWidth, myHeight);
            myPlayerBlock = new PlayerBlockParentScript(_env, _global, myGfx, myBlocks, myScoreManager, myWidth, myHeight);
            myPlayerBlock.CreateBlock();
        }

        public void TeminateGame()
        {
            myPlayerBlock?.Destroy();
            myBlocks?.Destroy();
            myGfx?.Destroy();
            myScoreManager?.Destroy();
            myPlayerBlock = null;
            myBlocks = null;
            myGfx = null;
            myScoreManager = null;
        }

        public void SpaceBar() => myPlayerBlock?.LetBlockFall();

        private int myWidth;
        private int myHeight;

        public void HandleMessage(string message, params object[] args)
        {
            switch (message)
            {
                case "KeyAction":
                    if (args.Length > 0 && args[0] is int v) KeyAction(v);
                    break;
                case "PauseGame":
                    PauseGame();
                    break;
                case "SpaceBar":
                    SpaceBar();
                    break;
                case "NewGame":
                    NewGame();
                    break;
            }
        }
    }
}
