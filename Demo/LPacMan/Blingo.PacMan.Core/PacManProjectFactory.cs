using BlingoEngine.Casts;
using BlingoEngine.Core;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Projects;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace Blingo.PacMan.Core;


public class PacManProjectFactory : IBlingoProjectFactory
{
    /// <summary>
    /// Name used throughout the project when referring to the root movie.
    /// </summary>
    public const string MovieName = "Blingo PacMan";
    private BlingoProjectSettings _settings;
    private IBlingoMovie? _movie;
    private BlingoPlayer? _blingoPlayer;

    /// <summary>
    /// Configures dependency injection and project-level options, mimicking the original Director project setup.
    /// </summary>
    public void Setup(IBlingoEngineRegistration config)
    {
        config
            .WithGlobalVars<GlobalVars>()
            .AddFont("press-start-2p", Path.Combine("Media", "Fonts", "press-start-2p-v9-latin-regular.ttf"))
            .AddFont("press-start-2p_2", Path.Combine("Media", "Fonts", "press-start-2p-v9-latin-regular2.ttf"))
            .WithProjectSettings(s =>
                {
                    s.ProjectFolder = "..\\";
                    s.ProjectName = "BlingoPacMan";
                    s.CodeFolder = "..\\Blingo.PacMan.Core\\";
                    s.MaxSpriteChannelCount = 300;
                    s.StageWidth = 730;
                    s.StageHeight = 500; 
                })
                .ForMovie(MovieName, s => s
                    .AddScriptsFromAssembly()
                )
                .ServicesBlingo(s => s
                    .AddSingleton<IPacManCore, PacManCore>()
                    .AddSingleton<IBonusesModel, BonusesModel>()
                    .AddSingleton<IGameModelRepository, GameModelRepository>()
                    .AddSingleton<IGameModel, GameModel>()
                    )
                ;
    }



    /// <summary>
    /// Loads all required cast libraries and keeps a reference to the player for later movie initialization.
    /// </summary>
    public Task LoadCastLibsAsync(IBlingoCastLibsContainer castlibContainer, BlingoPlayer blingoPlayer)
    {

        _blingoPlayer = blingoPlayer;

        blingoPlayer
            .AddCastLib("Data", true, c =>
                {
                    c.Add(BlingoMemberType.Bitmap, 0, "characters", Path.Combine("Media", "Data", "characters.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "maze", Path.Combine("Media", "Data", "maze.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "maze-1", Path.Combine("Media", "Data", "maze-1.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "maze-2", Path.Combine("Media", "Data", "maze-2.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "maze-3", Path.Combine("Media", "Data", "maze-3.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "maze-4", Path.Combine("Media", "Data", "maze-4.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "misc", Path.Combine("Media", "Data", "misc.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "mspacman", Path.Combine("Media", "Data", "mspacman.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "pills", Path.Combine("Media", "Data", "pills.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "sprites", Path.Combine("Media", "Data", "sprites.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "start", Path.Combine("Media", "Data", "start.png"));
                })
            .AddCastLib("Sounds", true, c =>
                {
                    c.Add(BlingoMemberType.Sound, 0, "S_back", Path.Combine("Media", "Sounds", "back.mp3"));
                    c.Add(BlingoMemberType.Sound, 0, "S_bonus", Path.Combine("Media", "Sounds", "bonus.mp3"));
                    c.Add(BlingoMemberType.Sound, 0, "S_dead", Path.Combine("Media", "Sounds", "dead.mp3"));
                    c.Add(BlingoMemberType.Sound, 0, "S_dot", Path.Combine("Media", "Sounds", "dot.mp3"));
                    c.Add(BlingoMemberType.Sound, 0, "S_eat", Path.Combine("Media", "Sounds", "eat.mp3"));
                    c.Add(BlingoMemberType.Sound, 0, "S_eaten", Path.Combine("Media", "Sounds", "eaten.mp3"));
                    c.Add(BlingoMemberType.Sound, 0, "S_frightened", Path.Combine("Media", "Sounds", "frightened.mp3"));
                    c.Add(BlingoMemberType.Sound, 0, "S_intro", Path.Combine("Media", "Sounds", "intro.mp3"));
                    c.Add(BlingoMemberType.Sound, 0, "S_life", Path.Combine("Media", "Sounds", "life.mp3"));
                });
        //blingoPlayer.CastLib("Sounds").GetMember<BlingoMemberSound>("S_Nature")!.Loop = true;
        InitMembers(blingoPlayer);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates the initial movie instance that will be shown when the engine starts.
    /// </summary>
    public Task<IBlingoMovie?> LoadStartupMovieAsync(IBlingoServiceProvider serviceProvider, BlingoPlayer blingoPlayer)
    {
        _movie = LoadMovie(blingoPlayer);

        return Task.FromResult<IBlingoMovie?>(_movie);
    }

    /// <summary>
    /// Runs the startup movie and optionally auto-plays it, matching the behaviour of the classic projector.
    /// </summary>
    public void Run(IBlingoMovie movie, bool autoPlayMovie)
    {
        if (autoPlayMovie)
            movie.Play();
    }

    /// <summary>
    /// Builds the movie, registers score labels and kicks off sprite initialization.
    /// </summary>
    public IBlingoMovie LoadMovie(IBlingoPlayer blingoPlayer)
    {
        _movie = blingoPlayer.NewMovie(MovieName);
        AddLabels();
        InitSprites();
        return _movie;
    }

    /// <summary>
    /// Adds score labels to the timeline so navigating frames in the Director UI remains intuitive.
    /// </summary>
    private void AddLabels()
    {
        if (_movie == null) return;
        _movie.SetScoreLabel(2, "Intro");
        _movie.SetScoreLabel(60, "Game");
    }

    /// <summary>
    /// Initializes static member properties that were previously stored as document metadata.
    /// </summary>
    public void InitMembers(BlingoPlayer player)
    {
      
    }

    /// <summary>
    /// Recreates the meticulously hand-positioned sprite setup from the original movie.
    /// </summary>
    public void InitSprites()
    {
       
        //_movie.Transitions.Add(54, new BlingoTransitionFrameSettings { Duration = 0.5f, TransitionId = 37 }); // 37.: Venetian blinds
        _movie.AddSprite(1,1,50,-230,-230).SetMember("start"); // background
    }
}

