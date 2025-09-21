// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using AbstUI.Inputs;
using AbstUI.Primitives;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Texts;
using System.Xml.Linq;

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    /// <summary>
    /// Controls the on-screen keyboard used to capture new high-score names.
    /// </summary>
    internal class EnterHighScoreBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
    {
        private readonly GlobalVars _global;
        private IBlingoMemberTextBase? _inputText;
        private IBlingoMemberTextBase? _popupTitle;
        private IAbstJoystickKeyboard? _keyboard;
        private IEnumerable<int> _spriteNums = [];
        private string _name = "";
        private Action<string>? _onNameEntered;

        public EnterHighScoreBehavior(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        /// <summary>
        /// Captures references to the text members used by the pop-up and hides them until needed.
        /// </summary>
        public void BeginSprite()
        {
            _inputText = Member<IBlingoMemberTextBase>("T_InputText")!;
            _popupTitle = Member<IBlingoMemberTextBase>("T_PopupTitle")!;
            _inputText.Text = "";
            foreach (var sn in _spriteNums)
                Sprite(sn).Visibility = false;
        }
        /// <summary>
        /// Ensures the keyboard instance is disposed when the sprite is removed.
        /// </summary>
        public void EndSprite()
        {
            if (_keyboard != null)
            {
                _keyboard.Close();
                _keyboard = null!;
            }
        }
        /// <summary>
        /// Returns the last name entered by the player.
        /// </summary>
        public string GetName() => _name;

        /// <summary>
        /// Displays the keyboard and wires event handlers for user input.
        /// </summary>
        public void Show(Action<string> onNameEntered)
        {
            _onNameEntered = onNameEntered;
            if (_inputText == null || _popupTitle == null || _inputText ==null) return;
            _keyboard = CreateJoystickKeyboard(c => c.Title = "Type your name", AbstJoystickKeyboard.KeyboardLayoutType.Azerty, false, new APoint(170, 310));
            _keyboard.BackgroundColor = AColor.FromHex("#42432dAA");
            _keyboard.TextColor = AColor.FromHex("#c5c528");
            _keyboard.SelectedBackgroundColor = AColor.FromHex("#37362a");
            _keyboard.UpdateStyle();
            _keyboard.MaxLength = 15;
            _keyboard.KeySelected += KeySelected;
            _keyboard.Closed += Closed;
            _keyboard.EnterPressed += EnterPressed;
            _name = "";
            _inputText.Text = "";
            _popupTitle.Text = "New highscore! Type your name:";
            foreach (var sn in _spriteNums)
                Sprite(sn).Visibility = true;
        }

        /// <summary>
        /// Triggers when the virtual keyboard's enter key is pressed.
        /// </summary>
        private void EnterPressed()
        {
            if (_keyboard == null) return;
            _name = _keyboard.Text;
            _global.PlayerName = _keyboard.Text;
            _keyboard.Close();
            _onNameEntered?.Invoke(_name);
        }

        /// <summary>
        /// Cleans up event handlers when the keyboard is dismissed.
        /// </summary>
        private void Closed()
        {
            if (_keyboard == null) return;
            //_keyboard.Close();
            _keyboard.KeySelected -= KeySelected;
            _keyboard.Closed -= Closed;
            _keyboard.EnterPressed -= EnterPressed;
            _keyboard = null;
            foreach (var sn in _spriteNums)
                Sprite(sn).Visibility = false;
        }

        /// <summary>
        /// Updates the text member as the player types.
        /// </summary>
        private void KeySelected(string chara)
        {
            if (_inputText == null || _keyboard == null) return;
            _inputText.Text = _keyboard.Text;
            
            //_keyboard.UpdateStyle();
        }

        /// <summary>
        /// Sets which sprites should be shown or hidden when the high-score prompt is active.
        /// </summary>
        internal void SetSpriteNums(IEnumerable<int> spritenums) => _spriteNums = spritenums;
    }
}

