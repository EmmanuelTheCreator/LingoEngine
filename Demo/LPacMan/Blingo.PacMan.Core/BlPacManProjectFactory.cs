using AbstUI.Primitives;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Projects;
using BlingoEngine.Setup;
using BlingoEngine.Sounds;
using Microsoft.Extensions.DependencyInjection;

namespace Blingo.PacMan.Core.Game;


public class BlPacManProjectFactory : IBlingoProjectFactory
{
    /// <summary>
    /// Name used throughout the project when referring to the root movie.
    /// </summary>
    public const string MovieName = "Blingo PacMan";
    public const string MenuLabel = "menu";
    public const string GameRunningLabel = "gameRunning";
    public const int GameStartFrame = 50;
    private BlingoProjectSettings? _settings;
    private IBlingoMovie? _movie;
    private BlingoPlayer? _blingoPlayer;

    /// <summary>
    /// Configures dependency injection and project-level options for the Pac-Man movie.
    /// </summary>
    public void Setup(IBlingoEngineRegistration config)
    {
        config
            .WithGlobalVars<GlobalVars>()
            .AddFont("press-start-2p", Path.Combine("Media", "Fonts", "press-start-2p-v9-latin-regular.ttf"))
            .AddFont("press-start-2p_2", Path.Combine("Media", "Fonts", "press-start-2p-v9-latin-regular2.ttf"))
            .WithProjectSettings(s =>
                {
                    _settings = s;
                    s.ProjectFolder = "..\\";
                    s.ProjectName = "BlingoPacMan";
                    s.CodeFolder = "..\\Blingo.PacMan.Core\\";
                    s.MaxSpriteChannelCount = 500;
                    s.StageWidth = 900;
                    s.StageHeight = 1200;
                })
                .ForMovie(MovieName, s => s
                    .AddScriptsFromAssembly()
                )
                .ServicesBlingo(s => s
                    .AddSingleton<BlPacManCore>()
                    .AddSingleton<GameModelRepository>()
                    );
    }


    #region Members

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
    /// Initializes static member properties that were previously stored as document metadata.
    /// </summary>
    public void InitMembers(BlingoPlayer player)
    {
        return;
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        var dataCast = player.CastLib("Data");
        if (dataCast is null)
            return;

        // Preload sheets and assign registration points so sprites line up with the hand-authored stage layout.
        // Large background images use a top-left registration point because the score positions them by absolute offsets,
        // while character sheets keep a centred registration point so sub-rect selections remain anchored in the middle.
        void ConfigureBitmap(string memberName, Func<BlingoMemberBitmap, APoint> regPointFactory)
        {
            var member = dataCast.GetMember<BlingoMemberBitmap>(memberName);
            if (member is null)
                return;

            member.Preload();
            member.RegPoint = regPointFactory(member);
        }

        //var backgroundMembers = new[] { "start", "maze", "maze-1", "maze-2", "maze-3", "maze-4" };
        //foreach (var name in backgroundMembers)
        //{
        //    // Background bitmaps are positioned via raw offsets, so keep the registration point at the top-left corner.
        //    ConfigureBitmap(name, _ => new APoint(0f, 0f));
        //}

        var centredSheets = new[] { "characters", "misc", "mspacman", "pills", "sprites" };
        foreach (var name in centredSheets)
        {
            // Sprite sheets are trimmed with the registration point centred on each frame.
            // Centre the registration point on the full sheet so individual sprite source rectangles stay aligned around their mid-points.
            ConfigureBitmap(name, m => new APoint(m.Width / 2f, m.Height / 2f));
        }

        // Loop the backing track on channel 1 so the attract screen ambience keeps running across replays.
        var backgroundSound = _blingoPlayer!.CastLib("Sounds").GetMember<BlingoMemberSound>("S_back");
        if (backgroundSound is not null)
            backgroundSound.Loop = true;
    } 
    #endregion



    /// <summary>
    /// Creates the initial movie instance that will be shown when the engine starts.
    /// </summary>
    public Task<IBlingoMovie?> LoadStartupMovieAsync(IBlingoServiceProvider serviceProvider, BlingoPlayer blingoPlayer)
    {
        var globals = serviceProvider.GetRequiredService<GlobalVars>();
        _movie = blingoPlayer.NewMovie(MovieName);
        AddLabels();
        InitSprites();
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
    /// Adds score labels to the timeline so navigating frames in the Director UI remains intuitive.
    /// </summary>
    private void AddLabels()
    {
        if (_movie == null) return;
        _movie.SetScoreLabel(1, MenuLabel);
        _movie.SetScoreLabel(GameStartFrame, GameRunningLabel);
    }

    

    /// <summary>
    /// Builds the sprite setup expected by the Pac-Man timeline.
    /// </summary>
    public void InitSprites()
    {
        if (_movie == null)
        {
            return;
        }

        var frameCount = Math.Max(GameStartFrame, 60);
        _movie.AddFrameBehavior<BlPacManStayOnFrameBehavior>(5);
        _movie.AddFrameBehavior<BlPacManGameBehavior>(GameStartFrame);

        var stageWidth = _blingoPlayer?.Stage.Width ?? _settings?.StageWidth ?? 730;
        var stageHeight = _blingoPlayer?.Stage.Height ?? _settings?.StageHeight ?? 500;
        var centerX = stageWidth / 2f;
        var centerY = stageHeight / 2f;

        _movie.AddSprite(1,1, GameStartFrame - 1,0,0, sprite => sprite.Lock = true).SetMember("start").AddBehavior<BlPacManMenuStartBehavior>(); ;

        _movie.AddSprite(2, GameStartFrame, frameCount,0,0, sprite =>sprite.Lock = true).SetMember("maze");

        
        _movie.AddSprite(4, GameStartFrame, frameCount, centerX, centerY, x => x.Name = "SoundManager").AddBehavior<BlPacManSoundManager>();

        _movie.AddSprite(5, GameStartFrame, frameCount, centerX, centerY, x => x.Name = "MenuStart"); //.AddBehavior<BlPacManMenuStartBehavior>(); // Start game button

        var hudBanner = _movie.AddSprite(6, GameStartFrame, frameCount, centerX, 40f,c => c.MemberSourceRect = new ARect(63, 2, 87, 26)).SetMember("misc");

        var startPrompt = _movie.AddSprite(7, 1, GameStartFrame - 1, centerX, 120f, c => c.MemberSourceRect = new ARect(34, 3, 56, 27)).SetMember("misc");

        var lifeSourceRect = new ARect(80, 1, 93, 14);
        var livesAnchor = _movie.AddSprite(20, GameStartFrame, frameCount, 80f, stageHeight - 40f, sprite =>
               {
                   sprite.Visibility = false;
                   sprite.MemberSourceRect = lifeSourceRect;
               })
            .SetMember("sprites")
            .AddBehavior<LivesBehavior>(behavior =>
            {
                behavior.Spacing = 18f;
                behavior.ScaleFactor = 1f;
                behavior.MemberSourceRect = lifeSourceRect;
            });

        var bonusSourceRect = new ARect(93, 2, 119, 28);
        var bonusesAnchor = _movie.AddSprite(21, GameStartFrame, frameCount, stageWidth - 80f, stageHeight - 40f, sprite =>
                {
                    sprite.Visibility = false;
                    sprite.MemberSourceRect = bonusSourceRect;
                })
            .SetMember("misc")
            .AddBehavior<BonusesBehavior>(behavior =>
                {
                    behavior.Spacing = 24f;
                    behavior.ScaleFactor = 0.75f;
                    behavior.MemberSourceRect = bonusSourceRect;
                });

        //var pelletField = _movie.AddSprite(22, GameStartFrame, frameCount, centerX, centerY, sprite =>
        //    {
        //        sprite.Visibility = false;
        //    }).AddBehavior<BlPacManPelletFieldBehavior>();

        //var powerPillField = _movie.AddSprite(23, GameStartFrame, frameCount, centerX, centerY, sprite =>
        //    {
        //        sprite.Visibility = false;
        //    }).AddBehavior<BlPacManPowerPillManager>();

        var roamingBonus = _movie.AddSprite(24, GameStartFrame, frameCount, centerX, centerY, sprite =>
            {
                sprite.Visibility = false;
            })
            .AddBehavior<BlPacManAnimationBehavior>()
            .AddBehavior<BlPacManRoamingBonusBehavior>();

        var pacMan = _movie.AddSprite(50, GameStartFrame, frameCount, centerX, centerY, sprite =>
            {
                sprite.Visibility = false;
            })
            .AddBehavior<BlPacManAnimationBehavior>()
            .AddBehavior<BlPacManActorBehavior>();

        var ghostNames = BlGhostManager.GhostNames;
        for (var i = 0; i < ghostNames.Length; i++)
        {
            var ghost = _movie.AddSprite(30+i, GameStartFrame, frameCount, centerX, centerY,  sprite =>
                {
                    sprite.Name = $"Ghost.{ghostNames[i]}";
                    sprite.Visibility = false;
                })
                .AddBehavior<BlPacManAnimationBehavior>()
                .AddBehavior<BlPacManGhostBehavior>(behavior =>
                {
                    behavior.GhostName = ghostNames[i];
                });
        }
    }
}

