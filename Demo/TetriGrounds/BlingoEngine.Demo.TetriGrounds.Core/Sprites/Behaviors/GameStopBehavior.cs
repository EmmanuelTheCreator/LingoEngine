// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Texts;
using System.ComponentModel.DataAnnotations;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 1_Game stop.ls
    // Handles keyboard input for two players and forwards actions to another sprite
    /// <summary>
    /// Handles keyboard input for two players and forwards actions to the background controller.
    /// </summary>
    public class GameStopBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, IHasMouseEnterEvent, IHasKeyDownEvent
    {
        // key codes for players
        private int key1, key2, key3, key4, key5, key6;
        private int key7, key8, key9, key10, key11, key12;

        private int oldkey1, oldkey2, oldkey1Act1, oldkey1Act2, oldkey2Act1, oldkey2Act2;
        private int pPlayer1, pPlayer2;
        private int myTargetSprite;
        private readonly GlobalVars _global;

        /// <summary>
        /// Stores the global state reference so we can respect the game running flag.
        /// </summary>
        public GameStopBehavior(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        /// <summary>
        /// Reads the keyboard mapping from the parameters text member.
        /// </summary>
        public void BeginSprite()
        {
            var parameters = Member<IBlingoMemberTextBase>("parameters")!;
            var test = parameters.Line[21].Word(2);
            key1  = Convert.ToInt32(parameters.Line[21].Word(2)); // Up
            key2  = Convert.ToInt32(parameters.Line[21].Word(5)); // Right
            key3  = Convert.ToInt32(parameters.Line[21].Word(3)); // Down
            key4  = Convert.ToInt32(parameters.Line[21].Word(4)); // Left
            key5  = Convert.ToInt32(parameters.Line[21].Word(6)); // Action1
            key6  = Convert.ToInt32(parameters.Line[21].Word(7)); // Action2
            key7  = Convert.ToInt32(parameters.Line[22].Word(2)); // Up
            key8  = Convert.ToInt32(parameters.Line[22].Word(5)); // Right
            key9  = Convert.ToInt32(parameters.Line[22].Word(3)); // Down
            key10 = Convert.ToInt32(parameters.Line[22].Word(4)); // Left
            key11 = Convert.ToInt32(parameters.Line[22].Word(6)); // Action1
            key12 = Convert.ToInt32(parameters.Line[22].Word(7)); // Action2
            oldkey1 = 9;
            // nothing for player1
            oldkey1Act1 = 0;
            oldkey1Act2 = 0;
            oldkey2 = 9;
            // nothing for player2
            oldkey2Act1 = 0;
            oldkey2Act2 = 0;
            pPlayer1 = 1;
            pPlayer2 = 0;
            myTargetSprite = 4;
        }



        /// <summary>
        /// Handles pause and hard drop keys while respecting the game running flag.
        /// </summary>
        public void KeyDown(BlingoKeyEvent key)
        {
            if (!_global.GameIsRunning) return;
            if (key.KeyPressed(35)) SendSprite<BgScriptBehavior>(myTargetSprite, s => s.PauseGame());
            if (key.KeyPressed(49)) SendSprite<BgScriptBehavior>(myTargetSprite, s => s.SpaceBar());
        }

        public void MouseEnter(BlingoMouseEvent mouse)
        {
            //Cursor = 200;
        }

        /// <summary>
        /// Polls the keyboard state each frame and forwards actions to the background behaviour.
        /// </summary>
        public void ExitFrame()
        {
            int keyy1;
            if (_Key.KeyPressed(key2) && pPlayer1 == 1) keyy1 = 2;
            else if (_Key.KeyPressed(key4) && pPlayer1 == 1) keyy1 = 4;
            else if (_Key.KeyPressed(key1) && pPlayer1 == 1) keyy1 = 1;
            else if (_Key.KeyPressed(key3) && pPlayer1 == 1) keyy1 = 3;
            else if (_Key.ControlDown) keyy1 = 1;
            else keyy1 = 9;

            int keyy2;
            if (_Key.KeyPressed(key7) && pPlayer2 == 1) keyy2 = 1;
            else if (_Key.KeyPressed(key9) && pPlayer2 == 1) keyy2 = 3;
            else if (_Key.KeyPressed(key8) && pPlayer2 == 1) keyy2 = 2;
            else if (_Key.KeyPressed(key10) && pPlayer2 == 1) keyy2 = 4;
            else keyy2 = 9;

            int Act1_1 = _Key.KeyPressed(key5) && pPlayer1 == 1 ? 1 : 0;
            int Act1_2 = _Key.KeyPressed(key6) && pPlayer1 == 1 ? 1 : 0;
            int Act2_1 = _Key.KeyPressed(key11) && pPlayer2 == 1 ? 1 : 0;
            int Act2_2 = _Key.KeyPressed(key12) && pPlayer2 == 1 ? 1 : 0;

            if (oldkey1 != keyy1)
            {
                oldkey1 = keyy1;
                SendSprite<BgScriptBehavior>(myTargetSprite, s => s.KeyAction(keyy1, 1));
            }
            if (oldkey2 != keyy2)
            {
                oldkey2 = keyy2;
                SendSprite<BgScriptBehavior>(myTargetSprite, s => s.KeyAction(keyy2, 2));
            }
            if (oldkey1Act1 != Act1_1)
            {
                oldkey1Act1 = Act1_1;
                if (oldkey1Act1 == 1)
                    SendSprite<BgScriptBehavior>(myTargetSprite, s => s.ActionKey(new object[] { 1, 5, 0 }));
            }
            if (oldkey2Act1 != Act2_1)
            {
                oldkey2Act1 = Act2_1;
                if (oldkey2Act1 == 1)
                    SendSprite<BgScriptBehavior>(myTargetSprite, s => s.ActionKey(new object[] { 2, 5, 0 }));
            }
            if (oldkey1Act2 != Act1_2)
            {
                oldkey1Act2 = Act1_2;
                SendSprite<BgScriptBehavior>(myTargetSprite, s => s.ActionKey(new object[] { 1, 6, Act1_2 }));
            }
            if (oldkey2Act2 != Act2_2)
            {
                oldkey2Act2 = Act2_2;
                SendSprite<BgScriptBehavior>(myTargetSprite, s => s.ActionKey(new object[] { 2, 6, Act2_2 }));
            }
            _Movie.GoTo(_Movie.CurrentFrame);
        }
    }
}

