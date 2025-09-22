using AbstUI.Commands;
using AbstUI.Primitives;
using AbstUI.Resources;
using BlingoEngine.Casts;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Commands;
using BlingoEngine.Sounds;
using BlingoEngine.Stages;
using BlingoEngine.Tools;
using BlingoEngine.Transitions;
using BlingoEngine.Transitions.TransitionLibrary;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace BlingoEngine.Core
{


    public class BlingoPlayer : IBlingoPlayer, IDisposable,
        IAbstCommandHandler<MovieRewindCommand>,
        IAbstCommandHandler<PlayMovieCommand>,
        IAbstCommandHandler<StepFrameCommand>
    {
        private readonly Lazy<CsvImporter> _csvImporter;
        private readonly BlingoCastLibsContainer _castLibsContainer;
        private readonly BlingoSound _sound;
        private readonly IBlingoWindow _window;
        private readonly IBlingoServiceProvider _serviceProvider;
        private readonly BlingoGlobalVars _globals;
        private Action<BlingoMovie> _actionOnNewMovie;
        private Dictionary<string, BlingoMovieEnvironment> _moviesByName = new();
        private readonly Dictionary<int, BlingoMovieEnvironment> _moviesByNumber = new();
        private List<BlingoMovieEnvironment> _movies = new();
        private List<CancellationTokenSource> _delayedActionsCts = new();

        private readonly BlingoKey _blingoKey;
        private readonly BlingoStage _stage;
        private readonly BlingoSystem _system;
        private readonly BlingoClock _clock;
        private BlingoStageMouse _mouse;
        private SynchronizationContext? _uiContext;

        public IBlingoFrameworkFactory Factory { get; private set; }
        public IBlingoServiceProvider ServiceProvider => _serviceProvider;
        public IBlingoClock Clock => _clock;
        public BlingoKey Key => _blingoKey;
        public IBlingoStage Stage => _stage;
        /// <inheritdoc/>
        public IBlingoCastLibsContainer CastLibs => _castLibsContainer;
        public IBlingoCast ActiveCastLib => _castLibsContainer.ActiveCast;

        /// <inheritdoc/>
        public IBlingoSound Sound => _sound;
        public IBlingoStageMouse Mouse => _mouse;
        public bool MediaRequiresAsyncPreload { get; set; }


        /// <inheritdoc/>
        public int CurrentSpriteNum => 1;
        /// <inheritdoc/>
        public bool NetPreset => true;
        /// <inheritdoc/>
        public bool ActiveWindow => true;
        /// <inheritdoc/>
        public bool SafePlayer => throw new NotImplementedException();
        /// <inheritdoc/>
        public string OrganizationName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ApplicationName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ApplicationPath { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ProductName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public int LastClick => 1;
        /// <inheritdoc/>
        public int LastEvent => 1;
        /// <inheritdoc/>
        public int LastKey => 1;
        /// <inheritdoc/>
        public Version ProductVersion { get; set; } = new Version(1, 0, 0, 0);
        /// <inheritdoc/>
        public Func<string> AlertHook { get; set; } = () => "";
        /// <inheritdoc/>
        bool IBlingoPlayer.SafePlayer { get; set; }
        public IBlingoMovie? ActiveMovie { get; private set; }


        public event Action<IBlingoMovie?>? ActiveMovieChanged;

        public BlingoPlayer(IBlingoServiceProvider serviceProvider, IBlingoFrameworkFactory factory, IBlingoCastLibsContainer castLibsContainer, IBlingoWindow window, IBlingoClock blingoClock, IBlingoSystem blingoSystem, IAbstResourceManager resourceManager, BlingoGlobalVars globals)
        {
            _csvImporter = new Lazy<CsvImporter>(() => new CsvImporter(resourceManager, MediaRequiresAsyncPreload));
            _actionOnNewMovie = m => { };
            _serviceProvider = serviceProvider;
            Factory = factory;
            _castLibsContainer = (BlingoCastLibsContainer)castLibsContainer;
            _sound = Factory.CreateSound(_castLibsContainer);
            _window = window;
            _clock = (BlingoClock)blingoClock;
            _system = (BlingoSystem)blingoSystem;
            _blingoKey = Factory.CreateKey();
            _stage = Factory.CreateStage(this);
            _mouse = Factory.CreateMouse(_stage);
            _uiContext = SynchronizationContext.Current;
            if (_uiContext == null)
                throw new Exception("UI SynchronizationContext not set");
            _globals = globals;
        }
        public void Dispose()
        {
            // todo
            foreach (var cts in _delayedActionsCts)
                cts.Cancel();
            _delayedActionsCts.Clear();
        }
        public void LoadStage(int width, int height, AColor backgroundColor)
        {
            Stage.Width = width;
            Stage.Height = height;
            Stage.BackgroundColor = backgroundColor;

        }

        /// <inheritdoc/>
        public void Alert(string message)
        {
            Console.WriteLine(message);
        }
        /// <inheritdoc/>
        public void AppMinimize()
        {

        }
        /// <inheritdoc/>
        public void Cursor(int cursorNum)
        {

        }
        /// <inheritdoc/>
        public void Halt()
        {

        }
        /// <inheritdoc/>
        public void Open(string applicationName)
        {

        }
        /// <inheritdoc/>
        public void Quit()
        {
        }
        /// <inheritdoc/>
        public bool WindowPresent()
        {
            return true;
        }
        public IBlingoCast CastLib(int number) => _castLibsContainer[number];
        public IBlingoCast CastLib(string name) => _castLibsContainer[name];
        public IBlingoCast? GetCast(BlingoCastRef castRef) => _castLibsContainer.GetCast(castRef);
        public IBlingoMember? GetMember(BlingoMemberRef memberRef) => _castLibsContainer.GetMember(memberRef);
        public T? GetMember<T>(BlingoMemberRef memberRef) where T : class, IBlingoMember
            => _castLibsContainer.GetMember<T>(memberRef);
        public IBlingoMovie? GetMovie(BlingoMovieRef movieRef)
        {
            if (movieRef.MovieNumber <= 0)
                return null;

            return _moviesByNumber.TryGetValue(movieRef.MovieNumber, out var environment)
                ? environment.Movie
                : null;
        }
        public IBlingoMovie NewMovie(string name, bool andActivate = true)
        {
            // Create the default cast
            if (_castLibsContainer.Count == 0)
                _castLibsContainer.AddCast("Internal");

            // Create a new movies scope, needed for behaviours.
            var scope = _serviceProvider.CreateScope();
            var transitionPlayer = new BlingoTransitionPlayer(_stage, _clock, _serviceProvider.GetRequiredService<IBlingoTransitionLibrary>());

            // Create the movie.
            var movieEnv = (BlingoMovieEnvironment)scope.ServiceProvider.GetRequiredService<IBlingoMovieEnvironment>();
            movieEnv.Init(name, _movies.Count + 1, this, _blingoKey, _sound, _mouse, _stage, _system, Clock, _castLibsContainer, scope, transitionPlayer, m =>
            {
                // On remove movie
                var movieEnvironment = m.GetEnvironment();
                _movies.Remove(movieEnvironment);
                _moviesByName.Remove(m.Name);
                _moviesByNumber.Remove(m.Number);
            }, _globals);
            var movieTyped = (BlingoMovie)movieEnv.Movie;

            // Add him
            _movies.Add(movieEnv);
            _moviesByName.Add(name, movieEnv);
            _moviesByNumber[movieTyped.Number] = movieEnv;

            Factory.AddMovie(_stage, movieTyped);

            // Add all movieScripts
            _actionOnNewMovie(movieTyped);

            // Activate him;
            if (andActivate)
            {
                SetActiveMovie(movieTyped);
            }
            return movieEnv.Movie;
        }

        public Task<IBlingoMovie> LoadMovieAsync(IBlingoMovieBuilder builder)
            => builder.BuildAsync(this);

        public void CloseMovie(IBlingoMovie movie)
        {
            var typed = (BlingoMovie)movie;
            typed.RemoveMe();
        }

        /// <summary>
        /// Format: comma split
        ///     Number,Type,Name,Registration Point,Filename
        ///     1,bitmap,BallB,"(5, 5)",
        /// </summary>
        public IBlingoPlayer LoadCastLibFromCsv(string castlibName, string pathAndFilenameToCsv, bool isInternal = false)
        {
            return LoadCastLibFromCsvAsync(castlibName, pathAndFilenameToCsv, isInternal).GetAwaiter().GetResult();
        }

        public async Task<IBlingoPlayer> LoadCastLibFromCsvAsync(string castlibName, string pathAndFilenameToCsv, bool isInternal = false)
        {
            var castLib = _castLibsContainer.AddCast(castlibName, isInternal);
            await _csvImporter.Value.ImportInCastFromCsvFileAsync(castLib, pathAndFilenameToCsv, true, x => Console.WriteLine("WARNING:" + x));
            return this;
        }
        public async Task<IBlingoPlayer> LoadAsync<TBlingoCastLibBuilder>() where TBlingoCastLibBuilder : class, IBlingoCastLibBuilder, new()
        {
            var builder = new TBlingoCastLibBuilder();
            await builder.BuildAsync(_castLibsContainer);
            return this;
        }
        public IBlingoPlayer AddCastLib(string name, bool isInternal = false, Action<IBlingoCast>? configure = null)
        {
            var castLib = _castLibsContainer.AddCast(name, isInternal);
            if (configure != null)
                configure(castLib);
            return this;
        }

        public void UnloadInternalCastLibs()
        {
            _castLibsContainer.RemoveInternal();
        }

        public void SetActiveMovie(BlingoMovie? movie)
        {
            ActiveMovie = movie;
            _stage.SetActiveMovie(movie);
            ActiveMovieChanged?.Invoke(movie);
        }

        void IBlingoPlayer.SetActiveMovie(IBlingoMovie? movie) => SetActiveMovie(movie as BlingoMovie);



        internal void SetActionOnNewMovie(Action<BlingoMovie> actionOnNewMovie)
        {
            _actionOnNewMovie = actionOnNewMovie;
        }


        #region Commands
        public bool CanExecute(MovieRewindCommand command) => ActiveMovie is BlingoMovie;

        public bool Handle(MovieRewindCommand command)
        {
            if (ActiveMovie is BlingoMovie movie)
                movie.Rewind();
            return true;
        }


        public bool CanExecute(StepFrameCommand command) => ActiveMovie is BlingoMovie;

        public bool Handle(StepFrameCommand command)
        {
            if (ActiveMovie is not BlingoMovie movie) return true;
            int offset = command.Offset;
            if (movie.IsPlaying)
            {
                var steps = Math.Abs(offset);
                for (int i = 0; i < steps; i++)
                {
                    if (offset > 0) movie.NextFrame();
                    else movie.PrevFrame();
                }
            }
            else
            {
                var target = MathCompat.Clamp(movie.Frame + offset, 1, movie.FrameCount);
                movie.GoToAndStop(target);
            }
            return true;
        }

        public bool CanExecute(PlayMovieCommand command) => ActiveMovie is BlingoMovie;

        public bool Handle(PlayMovieCommand command)
        {
            if (ActiveMovie is not BlingoMovie movie) return true;
            if (command.Frame.HasValue)
                movie.GoTo(command.Frame.Value);
            if (movie.IsPlaying)
                movie.Halt();
            else
                movie.Play();
            return true;
        }

        public bool CanExecute(SetFrameLabelCommand command) => ActiveMovie is BlingoMovie;

        public IBlingoEventMediator GetEventMediator() => _serviceProvider.GetRequiredService<IBlingoEventMediator>();

        public void ReplaceMouseObj(BlingoStageMouse newMouse)
        {
            _mouse = newMouse;
            foreach (var movie in _movies)
                movie.SetMouse(newMouse);
        }






        #endregion

        public void RunDelayed(Action action, int milliseconds, CancellationTokenSource? cts = null)
        {
            cts ??= new CancellationTokenSource();
            _delayedActionsCts.Add(cts);
            Task.Delay(milliseconds, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    if (_uiContext != null)
                        _uiContext.Post(_ => action(), null);
                    else
                        action();
                }
                _delayedActionsCts.Remove(cts);
            }, TaskScheduler.Default);
        }

        // Call: await RunOnUIThreadAsync(() => { ... }, ct);
        public Task RunOnUIThreadAsync(Action action, CancellationToken ct = default)
        {
            var ctx = _uiContext;
            if (ctx == null || SynchronizationContext.Current == ctx)
            {
                ct.ThrowIfCancellationRequested();
                action();
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            ctx.Post(_ =>
            {
                if (ct.IsCancellationRequested) { tcs.TrySetCanceled(ct); return; }
                try { action(); tcs.TrySetResult(null); }
                catch (Exception ex) { tcs.TrySetException(ex); }
            }, null);
            return tcs.Task;
        }

        // Call: await RunOnUIThreadAsync(async () => { await ... }, ct);
        public Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> func, CancellationToken ct = default)
        {
            var ctx = _uiContext;
            if (ctx == null || SynchronizationContext.Current == ctx)
                return ct.IsCancellationRequested ? Task.FromCanceled<T>(ct) : func();

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            ctx.Post(async _ =>
            {
                if (ct.IsCancellationRequested) { tcs.TrySetCanceled(ct); return; }
                try
                {
                    var result = await func().ConfigureAwait(false);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }, null);

            return tcs.Task;
        }
        public Task<T> RunOnUIThreadAsync<T>(Func<T> func, CancellationToken ct = default)
        {
            var ctx = _uiContext;
            if (ctx == null || SynchronizationContext.Current == ctx)
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<T>(ct);
                return Task.FromResult(func());
            }

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            ctx.Post(_ =>
            {
                if (ct.IsCancellationRequested) { tcs.TrySetCanceled(ct); return; }
                try
                {
                    var result = func();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }, null);

            return tcs.Task;
        }


        // If you want synchronous/blocking (careful: deadlocks if already on UI thread):
        public void RunOnUIThread(Action action)
        {
            var ctx = _uiContext;
            if (ctx == null || SynchronizationContext.Current == ctx) { action(); return; }
            ctx.Send(_ => action(), null);
        }

        private class UiContext : SynchronizationContext
        {
            private readonly BlockingCollection<(SendOrPostCallback, object?)> _queue = new();

            public UiContext()
            {
                // Start loop on the UI thread
                var thread = new Thread(Run) { IsBackground = false };
                thread.Start();
            }

            private void Run()
            {
                SynchronizationContext.SetSynchronizationContext(this);

                foreach (var (d, state) in _queue.GetConsumingEnumerable())
                    d(state);
            }

            public override void Post(SendOrPostCallback d, object? state)
                => _queue.Add((d, state));
        }
    }
}




