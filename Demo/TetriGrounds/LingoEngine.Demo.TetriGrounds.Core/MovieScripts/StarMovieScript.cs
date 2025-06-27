using LingoEngine.Movies;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Movies.Events;

namespace LingoEngine.Demo.TetriGrounds.Core.MovieScripts
{
    // Converted from 4_StarMovie.ls
    public class StarMovieScript : LingoMovieScript, IHasStartMovieEvent, IHasStopMovieEvent
    {
        private readonly GlobalVars _global;

        public StarMovieScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }

        public void StartMovie()
        {
            var sp = ((LingoMovie)_Movie).GetServiceProvider();
            var env = sp.GetRequiredService<ILingoMovieEnvironment>();
            _global.SpriteManager = new ParentScripts.SpriteManagerParentScript(env);
            _global.SpriteManager.Init(100);
            _global.MousePointer = new ParentScripts.MousePointer(env);
            _global.MousePointer.Init(0);
        }

        public void StopMovie()
        {
            _global.SpriteManager = null;
            _global.MousePointer = null;
        }

        public string ReplaceSpaces(string str, int leng)
        {
            string thisField = str;
            for (int i = 0; i < thisField.Length; i++)
            {
                if (thisField[i] == ' ')
                    thisField = thisField.Substring(0, i) + "_" + thisField[(i + 1)..];
            }
            if (thisField.Length > leng) thisField = thisField.Substring(0, leng);
            return thisField;
        }
    }
}
