namespace LingoEngine.Demo.TetriGrounds.Core
{
    public interface IArkCore
    {
        void Resetgame();
    }
    internal class TetriGroundsCore : IArkCore
    {
        private readonly GlobalVars _global;

        public TetriGroundsCore(GlobalVars global)
        {
            _global = global;
        }

        public void Resetgame()
        {
        }
    }
}
