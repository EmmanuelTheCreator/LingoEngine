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

        ILingoCast AddCast(string name);
        ILingoCast GetCast(int number);
        string GetCastName(int number);
        LingoMember GetMember(string cast, int number);
        LingoMember GetMember(string name);
        bool TryGetMember(string name, out LingoMember? member);
        ILingoMovie LoadMovie(string name, bool andActivate = true);
        void CloseMovie(ILingoMovie movie);
    }
    public class LingoProject : ILingoProject
    {
        private Dictionary<string, ILingoCast> _castsByName = new();
        private List<ILingoCast> _casts = new();
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

        public ILingoCast CastLib => _InternalCast;
        public ILingoClock Clock => _clock;

        private readonly IServiceProvider _serviceProvider;

        public ILingoFrameworkFactory Factory { get; private set; }
        public ILingoMovie? ActiveMovie { get; set; }

        public LingoProject(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Factory = serviceProvider.GetRequiredService<ILingoFrameworkFactory>();
            _clock = new LingoClock();
            _player = new LingoPlayer();
            _LingoKey = new LingoKey();
            _Sound = Factory.CreateSound();
            _Stage = Factory.CreateStage();
            _Mouse = new LingoMouse(_Stage);
            _InternalCast = (LingoCast)AddCast("Internal");
            _System = new LingoSystem();
        }

        public string GetCastName(int number) => _casts[number - 1].Name;
        public ILingoCast GetCast(int number) => _casts[number - 1];

        public ILingoCast AddCast(string name)
        {
            var cast = new LingoCast(name, _casts.Count + 1);
            _casts.Add(cast);
            _castsByName.Add(name, cast);
            return cast;
        }
        public ILingoMovie LoadMovie(string name, bool andActivate = true)
        {
            var movieEnv = new LingoMovieEnvironment(name, _movies.Count + 1, _player, _LingoKey, _Sound, _Mouse, _Stage, _InternalCast, _System, Clock, Factory, _serviceProvider);
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


        public LingoMember GetMember(string cast, int number) => _castsByName[cast].Member(number);
        public LingoMember GetMember(string name)
        {
            foreach (var cast in _casts)
                if (cast.TryGetMember(name, out var member)) return member!;

            throw new Exception("Member not found with name:" + name);
        }
        public bool TryGetMember(string name, out LingoMember? member)
        {
            foreach (var cast in _casts)
                if (cast.TryGetMember(name, out member)) return true;

            member = null;
            return false;
        }



    }
}
