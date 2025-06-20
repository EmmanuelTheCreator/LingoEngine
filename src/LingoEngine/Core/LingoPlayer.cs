using System;
using LingoEngine.Casts;
using LingoEngine.Commands;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Sounds;
using LingoEngine.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Core
{


    public class LingoPlayer : ILingoPlayer,
        ICommandHandler<RewindMovieCommand>,
         ICommandHandler<PlayMovieCommand>,
        ICommandHandler<StepFrameCommand>,
        ICommandHandler<SetScoreLabelCommand>,
        ICommandHandler<AddFrameLabelCommand>,
        ICommandHandler<UpdateFrameLabelCommand>
    {
        private Lazy<CsvImporter> _csvImporter = new Lazy<CsvImporter>(() => new CsvImporter());
        private readonly LingoCastLibsContainer _castLibsContainer;
        private readonly LingoSound _sound;
        private readonly ILingoWindow _window;
        private readonly IServiceProvider _serviceProvider;
        private Action<LingoMovie> _actionOnNewMovie;
        private Dictionary<string, LingoMovieEnvironment> _moviesByName = new();
        private List<LingoMovieEnvironment> _movies = new();


        private readonly LingoKey _LingoKey;
        private readonly LingoMouse _Mouse;
        private readonly LingoStage _Stage;
        private readonly LingoSystem _System;
        private readonly LingoClock _clock;
        public ILingoFrameworkFactory Factory { get; private set; }

        public ILingoClock Clock => _clock;
        public LingoKey Key => _LingoKey;
        public LingoStage Stage => _Stage;
        /// <inheritdoc/>
        public ILingoCast ActiveCastLib => _castLibsContainer.ActiveCast;
        /// <inheritdoc/>
        public ILingoSound Sound => _sound;


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
        bool ILingoPlayer.SafePlayer { get; set; }
        public ILingoMovie? ActiveMovie { get; private set; }
        public event Action<ILingoMovie?>? ActiveMovieChanged;

        public LingoPlayer(IServiceProvider serviceProvider, ILingoFrameworkFactory factory, ILingoCastLibsContainer castLibsContainer, ILingoWindow window, ILingoClock lingoClock, ILingoSystem lingoSystem)
        {
            _actionOnNewMovie= m => { };
            _serviceProvider = serviceProvider;
            Factory = factory;
            _castLibsContainer = (LingoCastLibsContainer)castLibsContainer;
            _sound = Factory.CreateSound(_castLibsContainer);
            _window = window;
            _clock = (LingoClock)lingoClock;
            _System = (LingoSystem)lingoSystem;
            _LingoKey = Factory.CreateKey();
            _Stage = Factory.CreateStage(this);
            _Mouse = Factory.CreateMouse(_Stage);
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

        public ILingoMovie NewMovie(string name, bool andActivate = true)
        {
            // Create the default cast
            if (_castLibsContainer.Count == 0) 
                _castLibsContainer.AddCast("Internal");

            // Create a new movies scope, needed for behaviours.
            var scope = _serviceProvider.CreateScope();

            // Create the movie.
            var movieEnv = (LingoMovieEnvironment)scope.ServiceProvider.GetRequiredService<ILingoMovieEnvironment>();
            movieEnv.Init(name, _movies.Count + 1, this, _LingoKey, _sound, _Mouse, _Stage, _System, Clock, _castLibsContainer, scope,m =>
            {
                // On remove movie
                var movieEnvironment = m.GetEnvironment();
                _movies.Remove(movieEnvironment);
                _moviesByName.Remove(m.Name);
            });
            var movieTyped = (LingoMovie)movieEnv.Movie;

            // Add him
            _movies.Add(movieEnv);
            _moviesByName.Add(name, movieEnv);

            Factory.AddMovie(_Stage, movieTyped);

            // Add all movieScripts
            _actionOnNewMovie(movieTyped);

            // Activate him;
            if (andActivate)
            {
                SetActiveMovie(movieTyped);
            }
            return movieEnv.Movie;
        }
        public void CloseMovie(ILingoMovie movie)
        {
            var typed = (LingoMovie)movie;
            typed.RemoveMe();
        }

        /// <summary>
        /// Format: comma split
        ///     Number,Type,Name,Registration Point,Filename
        ///     1,bitmap,BallB,"(5, 5)",
        /// </summary>
        public ILingoPlayer LoadCastLibFromCsv(string castlibName, string pathAndFilenameToCsv)
        {
            var castLib = _castLibsContainer.AddCast(castlibName);
            _csvImporter.Value.ImportInCastFromCsvFile(castLib, pathAndFilenameToCsv);
            return this;
        }

        public ILingoPlayer AddCastLib(string name, Action<ILingoCast>? configure = null)
        {
            var castLib = _castLibsContainer.AddCast(name);
            if (configure != null)
                configure(castLib);
            return this;
        }

        public void SetActiveMovie(LingoMovie? movie)
        {
            ActiveMovie = movie;
            _Stage.SetActiveMovie(movie);
            ActiveMovieChanged?.Invoke(movie);
        }

        void ILingoPlayer.SetActiveMovie(ILingoMovie? movie) => SetActiveMovie(movie as LingoMovie);

        internal void LoadMovieScripts(IEnumerable<LingoMovieScript> enumerable)
        {
            throw new NotImplementedException();
        }

        internal void SetActionOnNewMovie(Action<LingoMovie> actionOnNewMovie)
        {
            _actionOnNewMovie = actionOnNewMovie;
        }


        #region Commands
        public bool CanExecute(RewindMovieCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(RewindMovieCommand command)
        {
            if (ActiveMovie is LingoMovie movie)
                movie.GoTo(1);
            return true;
        }


        public bool CanExecute(StepFrameCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(StepFrameCommand command)
        {
            if (ActiveMovie is not LingoMovie movie) return true;
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
                var target = Math.Clamp(movie.Frame + offset, 1, movie.FrameCount);
                movie.GoToAndStop(target);
            }
            return true;
        }

        public bool CanExecute(PlayMovieCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(PlayMovieCommand command)
        {
            if (ActiveMovie is not LingoMovie movie) return true;
            if (command.Frame.HasValue)
                movie.GoTo(command.Frame.Value);
            if (movie.IsPlaying)
                movie.Halt();
            else
                movie.Play();
            return true;
        }

        public bool CanExecute(SetScoreLabelCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(SetScoreLabelCommand command)
        {
            if (ActiveMovie is LingoMovie movie)
                movie.SetScoreLabel(command.FrameNumber, command.Name);
            return true;
        }

        public bool CanExecute(AddFrameLabelCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(AddFrameLabelCommand command)
        {
            if (ActiveMovie is LingoMovie movie)
                movie.SetScoreLabel(command.FrameNumber, command.Name);
            return true;
        }

        public bool CanExecute(UpdateFrameLabelCommand command) => ActiveMovie is LingoMovie;

        public bool Handle(UpdateFrameLabelCommand command)
        {
            if (ActiveMovie is LingoMovie movie)
            {
                movie.SetScoreLabel(command.PreviousFrame, null);
                movie.SetScoreLabel(command.NewFrame, command.Name);
            }
            return true;
        }
        #endregion
    }
}



