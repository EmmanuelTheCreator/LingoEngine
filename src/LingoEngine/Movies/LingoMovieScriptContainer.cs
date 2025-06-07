
namespace LingoEngine.Movies
{
    internal class LingoMovieScriptContainer
    {
        private readonly Dictionary<Type, LingoMovieScript> _movieScripts = new();
        private readonly ILingoMovieEnvironment _movieEnvironment;
        private readonly LingoEventMediator _eventMediator;

        public LingoMovieScriptContainer(ILingoMovieEnvironment movieEnvironment, LingoEventMediator eventMediator)
        {
            _movieEnvironment = movieEnvironment;
            _eventMediator = eventMediator;
        }
        public void Add<T>() where T : LingoMovieScript
        {
            var ms = _movieEnvironment.Factory.CreateMovieScript<T>((LingoMovie)_movieEnvironment.Movie);
            _movieScripts.Add(typeof(T), ms);
            _eventMediator.Subscribe(ms);
        }
        public void Remove(LingoMovieScript ms)
        {
            _movieScripts.Remove(ms.GetType());
            _eventMediator.Unsubscribe(ms);
        }

        public TResult? Call<T, TResult>(Func<T, TResult> action) where T : LingoMovieScript
        {
            var type = typeof(T);
            var script = Get<T>();
            if (script != null)
                return action(script);
            return default;
        }
        public void Call<T>(Action<T> action) where T : LingoMovieScript
        {
            var type = typeof(T);
            var script = Get<T>();
            if (script != null)
                action(script);
        }

        internal T? Get<T>() where T : LingoMovieScript
        {
            if (_movieScripts.TryGetValue(typeof(T), out var ms)) return ms as T;
            return null;
        }

        internal void CallAll(Action<LingoMovieScript> actionOnAll)
        {
            foreach (var ms in _movieScripts.Values)  
                actionOnAll(ms); 
        }
    }
}
