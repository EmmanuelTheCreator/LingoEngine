using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Sounds;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Core
{

    // Example interface for injection

    public interface ILingoProject
    {
        string Name { get; set; }
        ILingoMovie? ActiveMovie { get; set; }


        ILingoMovie LoadMovie(string name, bool andActivate = true);
        void CloseMovie(ILingoMovie movie);
    }
    public class LingoProject : ILingoProject
    {
        
        private Dictionary<string, LingoMovieEnvironment> _moviesByName = new();
        private List<LingoMovieEnvironment> _movies = new();
        public string Name { get; set; } = "";

        private readonly LingoPlayer _player;
        private readonly LingoKey _LingoKey;
        private readonly LingoSound _Sound;
        private readonly LingoMouse _Mouse;
        private readonly LingoStage _Stage;
        private readonly LingoCast _InternalCast;
        private readonly LingoSystem _System;
        private readonly ILingoClock _clock;

        public ILingoPlayer Player => _player;

        public ILingoKey Key => _LingoKey;

        public ILingoSound Sound => _Sound;

        public ILingoMouse Mouse => _Mouse;

        public ILingoSystem System => _System;

        public ILingoClock Clock => _clock;

        private readonly IServiceProvider _serviceProvider;

        public ILingoFrameworkFactory Factory { get; private set; }

        private readonly LingoCastLibsContainer _castLibsContainer;

        public ILingoMovie? ActiveMovie { get; set; }

        public LingoProject(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Factory = serviceProvider.GetRequiredService<ILingoFrameworkFactory>();
            _castLibsContainer = new LingoCastLibsContainer();
            _clock = new LingoClock();
            _player = new LingoPlayer();
            _LingoKey = new LingoKey();
            _Sound = Factory.CreateSound();
            _Stage = Factory.CreateStage();
            _Mouse = new LingoMouse(_Stage);
            _System = new LingoSystem();
            
        }

       
        public ILingoMovie LoadMovie(string name, bool andActivate = true)
        {
            // Create the default cast
            _castLibsContainer.AddCast("Internal");
            var movieEnv = new LingoMovieEnvironment(name, _movies.Count + 1, _player, _LingoKey, _Sound, _Mouse, _Stage, _System, Clock, Factory, _castLibsContainer, _serviceProvider);
            _movies.Add(movieEnv);
            _moviesByName.Add(name, movieEnv);
            if (andActivate)
                ActiveMovie = movieEnv.Movie;
            return movieEnv.Movie;
        }
        public void CloseMovie(ILingoMovie movie)
        {
            var typed = (LingoMovie)movie;
            var movieEnvironment = typed.GetEnvironment();
            _movies.Remove(movieEnvironment);
            _moviesByName.Remove(typed.Name);
            movieEnvironment.Dispose();
        }


       



    }
}
