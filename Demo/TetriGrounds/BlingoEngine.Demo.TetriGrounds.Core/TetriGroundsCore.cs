// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

namespace BlingoEngine.Demo.TetriGrounds.Core
{
    /// <summary>
    /// Defines the minimal contract for game state orchestration that the TetriGrounds demo expects.
    /// </summary>
    public interface ITetriGroundsCore
    {
        /// <summary>
        /// Resets any transient state created while the game was running. The original Lingo project
        /// called this when leaving the game to guarantee a clean restart.
        /// </summary>
        void Resetgame();
    }

    /// <summary>
    /// Default implementation that currently acts as a placeholder while the original behaviour is ported.
    /// </summary>
    internal class TetriGroundsCore : ITetriGroundsCore
    {
        private readonly GlobalVars _global;

        /// <summary>
        /// Stores the shared <see cref="GlobalVars"/> instance for later use when the port is completed.
        /// </summary>
        public TetriGroundsCore(GlobalVars global)
        {
            _global = global;
        }

        /// <inheritdoc />
        public void Resetgame()
        {
            // The original Lingo script toggled a number of flags here. Keeping the stub in place
            // makes it obvious that the behaviour still needs to be implemented without breaking callers.
        }
    }
}

