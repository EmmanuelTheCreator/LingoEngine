using BlingoEngine.Events;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Game
{
    internal class BlPacManSoundManager : BlingoSpriteBehavior,IHasKeyDownEvent
    {
        private readonly GlobalVars _globals;

        public BlPacManSoundManager(IBlingoMovieEnvironment env, GlobalVars globals) : base(env)
        {
            _globals = globals;
        }

        /// <inheritdoc />
        public void KeyDown(BlingoKeyEvent key)
        {
            // 'S' toggles sound for the attract mode.
            if (key.KeyPressed(83))
                ToggleSound();
        }

        /// <summary>
        /// Toggles the attract music playback while updating the muted flag.
        /// </summary>
        private void ToggleSound()
        {
            var muted = !_globals.State.IsMuted;
            _globals.State.IsMuted = muted;

            if (muted)
                _Player.SoundStopBack();
            else
                _Player.SoundPlayBack();
        }
    }
}
