using LingoEngine.Sounds;

namespace LingoEngine
{

    // Example interface for injection
    public interface ILingoEnvironment
    {
        ILingoPlayer Player { get; }
        ILingoKey Key { get; }
        ILingoSound Sound { get; }
        ILingoMouse Mouse { get; }
        ILingoSystem System { get; }
        ILingoMovie Movie { get; }
        ILingoCast CastLib { get; }
        ILingoClock Clock { get; }
    }
    public interface ILingoProject : ILingoEnvironment
    {
        string Name { get; set; }

        ILingoCast AddCast(string name);
        ILingoCast GetCast(int number);
        string GetCastName(int number);
        LingoMember GetMember(string cast, int number);
        LingoMember GetMember(string name);
        bool TryGetMember(string name, out LingoMember? member);
    }
    public class LingoProject : ILingoProject
    {
        private Dictionary<string, ILingoCast> _castsByName = new();
        private List<ILingoCast> _casts = new();
        private Dictionary<string, ILingoScore> _scoresByName = new();
        private List<ILingoScore> _scores = new();
        public string Name { get; set; }

        private readonly LingoPlayer _player;
        private readonly LingoKey _LingoKey;
        private readonly LingoSound _Sound;
        private readonly LingoMouse _Mouse;
        private readonly ILingoScore _score;
        private readonly LingoCast _InternalCast;
        private readonly LingoMovie _Movie;
        private readonly LingoSystem _System;
        private readonly ILingoClock _clock;

        public ILingoPlayer Player => _player;

        public ILingoKey Key => _LingoKey;

        public ILingoSound Sound => _Sound;

        public ILingoMouse Mouse => _Mouse;

        public ILingoSystem System => _System;

        public ILingoMovie Movie => _Movie;

        public ILingoCast CastLib => _InternalCast;
        public ILingoClock Clock => _clock;

        public LingoProject(string name)
        {
            Name = name;
            _player = new LingoPlayer();
            _LingoKey = new LingoKey();
            _Sound = new LingoSound();
            _score = AddScore("Default");
            _Mouse = new LingoMouse(_score);
            _InternalCast = (LingoCast)AddCast("Internal");
            _Movie = new LingoMovie(_score);
            _System = new LingoSystem();
        }

        public string GetCastName(int number) => _casts[number - 1].Name;
        public ILingoCast GetCast(int number) => _casts[number - 1];
        public string GetScoreName(int number) => _scores[number - 1].Name;
        public ILingoScore GetScore(int number) => _scores[number - 1];

        public ILingoCast AddCast(string name)
        {
            var cast = new LingoCast(name, _casts.Count + 1);
            _casts.Add(cast);
            _castsByName.Add(name, cast);
            return cast;
        }
        public ILingoScore AddScore(string name)
        {
            var score = new LingoScore(this, name, _scores.Count + 1);
            _scores.Add(score);
            _scoresByName.Add(name, score);
            return score;
        }


        public LingoMember GetMember(string cast, int number) => _castsByName[cast].GetMember(number);
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
