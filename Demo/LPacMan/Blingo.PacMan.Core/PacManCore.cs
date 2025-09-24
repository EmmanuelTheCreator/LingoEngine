using System;
using Blingo.PacMan.Core.Sprites.Behaviors;
using BlingoEngine.Core;

namespace Blingo.PacMan.Core
{

    public interface IPacManCore
    {
        void Resetgame();
    }

    internal class PacManCore : IPacManCore
    {
        private readonly GlobalVars _global;
        private readonly IBlingoPlayer _player;

        public PacManCore(GlobalVars global, IBlingoPlayer player)
        {
            _global = global ?? throw new ArgumentNullException(nameof(global));
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        /// <inheritdoc />
        public void Resetgame()
        {
            _global.ClearGlobals();

            _player.Sound.StopAll();

            var movie = _player.ActiveMovie;
            if (movie is null)
            {
                return;
            }

            movie.GoTo(PacManProjectFactory.IntroLabel);

            movie.SendAllSprites<PacManGameBehavior>(behavior => behavior.ResetToAttract());
        }
    }
}
