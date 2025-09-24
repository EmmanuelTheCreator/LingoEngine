namespace Blingo.PacMan.Core
{
  
    public interface IPacManCore
    {
        void Resetgame();
    }

    internal class PacManCore : IPacManCore
    {
        private readonly GlobalVars _global;

        public PacManCore(GlobalVars global)
        {
            _global = global;
        }

        /// <inheritdoc />
        public void Resetgame()
        {
        }
    }
}

