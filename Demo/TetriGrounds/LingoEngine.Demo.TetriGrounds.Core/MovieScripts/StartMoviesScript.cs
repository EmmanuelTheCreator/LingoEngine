using LingoEngine.Demo.TetriGrounds.Core;
using LingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.MovieScripts
{
    public class StartMoviesScript : LingoMovieScript, IHasStartMovieEvent
    {
        private readonly GlobalVars _global;
        private readonly IArkCore arkCore;

        public StartMoviesScript(ILingoMovieEnvironment env, GlobalVars globalVars, IArkCore arkCore) : base(env)
        {
            _global = globalVars;
            this.arkCore = arkCore;
        }



        public void StartMovie()
        {
        }


    }
}