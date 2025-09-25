using System;
using System.IO;
using System.Threading.Tasks;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.Behaviors;
using Blingo.PacMan.Core.Sprites.MovieScripts;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Projects;
using BlingoEngine.Setup;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using Microsoft.Extensions.DependencyInjection;

namespace Blingo.PacMan.Core.Game;


public class PacManProjectFactory : IBlingoProjectFactory
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
                    s.MaxSpriteChannelCount = 300;
                    s.StageWidth = 730;
                    s.StageHeight = 500;
                })
                .ForMovie(MovieName, s => s
                    .AddMovieScript<PacManStartMovieScript>()
                    .AddScriptsFromAssembly()
                )
                .ServicesBlingo(s => s
                    .AddSingleton<PacManCore>()
                    .AddSingleton<GameModelRepository>()
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
        _movie.SetScoreLabel(1, MenuLabel);
        _movie.SetScoreLabel(GameStartFrame, GameRunningLabel);
    }

    /// <summary>
    /// Initializes static member properties that were previously stored as document metadata.
    /// </summary>
    public void InitMembers(BlingoPlayer player)
    {
        if (player is null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        var dataCast = player.CastLib("Data");
        if (dataCast is null)
        {
            return;
        }

        // Preload sheets and assign registration points so sprites line up with the hand-authored stage layout.
        // Large background images use a top-left registration point because the score positions them by absolute offsets,
        // while character sheets keep a centred registration point so sub-rect selections remain anchored in the middle.
        void ConfigureBitmap(string memberName, Func<BlingoMemberBitmap, APoint> regPointFactory)
        {
            var member = dataCast.GetMember<BlingoMemberBitmap>(memberName);
            if (member is null)
            {
                return;
            }

            member.Preload();
            member.RegPoint = regPointFactory(member);
        }

        var backgroundMembers = new[] { "start", "maze", "maze-1", "maze-2", "maze-3", "maze-4" };
        foreach (var name in backgroundMembers)
        {
            // Background bitmaps are positioned via raw offsets, so keep the registration point at the top-left corner.
            ConfigureBitmap(name, _ => new APoint(0f, 0f));
        }

        var centredSheets = new[] { "characters", "misc", "mspacman", "pills", "sprites" };
        foreach (var name in centredSheets)
        {
            // Sprite sheets are trimmed with the registration point centred on each frame.
            // Centre the registration point on the full sheet so individual sprite source rectangles stay aligned around their mid-points.
            ConfigureBitmap(name, m => new APoint(m.Width / 2f, m.Height / 2f));
        }

        var soundsCast = player.CastLib("Sounds");
        if (soundsCast is null)
        {
            return;
        }

        // Loop the backing track on channel 1 so the attract screen ambience keeps running across replays.
        var backgroundSound = soundsCast.GetMember<BlingoMemberSound>("S_back");
        if (backgroundSound is not null)
        {
            backgroundSound.Loop = true;
        }
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

        var frameCount = Math.Max(GameStartFrame, _movie.FrameCount);
        _movie.AddFrameBehavior<PacManStayOnFrameBehavior>(1);

        var stageWidth = _blingoPlayer?.Stage.Width ?? _settings?.StageWidth ?? 730;
        var stageHeight = _blingoPlayer?.Stage.Height ?? _settings?.StageHeight ?? 500;
        var centerX = stageWidth / 2f;
        var centerY = stageHeight / 2f;

        var menuBackground = _movie.AddSprite("PacMan.MenuBackground", sprite =>
        {
            sprite.BeginFrame = 1;
            sprite.EndFrame = GameStartFrame - 1;
            sprite.LocH = -230;
            sprite.LocV = -230;
            sprite.Lock = true;
        });
        menuBackground.SetMember("start");

        var mazeBackground = _movie.AddSprite("PacMan.MazeBackground", sprite =>
        {
            sprite.BeginFrame = GameStartFrame;
            sprite.EndFrame = frameCount;
            sprite.LocH = -230;
            sprite.LocV = -230;
            sprite.Lock = true;
        });
        mazeBackground.SetMember("maze");

        var controller = _movie.AddSprite("PacMan.Controller", sprite =>
        {
            sprite.LocH = centerX;
            sprite.LocV = centerY;
            sprite.LocZ = 100;
            sprite.Puppet = true;
            sprite.Visibility = false;
        });
        controller.AddBehavior<PacManGameBehavior>();

        var startListener = _movie.AddSprite("PacMan.MenuStart", sprite =>
        {
            sprite.LocH = centerX;
            sprite.LocV = centerY;
            sprite.LocZ = 110;
            sprite.Puppet = true;
            sprite.Visibility = false;
        });
        startListener.AddBehavior<PacManMenuStartBehavior>();

        var hudBanner = _movie.AddSprite(5, GameStartFrame, frameCount, centerX, 40f, sprite =>
        {
            sprite.Puppet = true;
        });
        hudBanner.SetMember("misc");
        hudBanner.MemberSourceRect = new ARect(126, 4, 174, 52);

        var startPrompt = _movie.AddSprite(6, 1, GameStartFrame - 1, centerX, 120f, sprite =>
        {
            sprite.Puppet = true;
        });
        startPrompt.SetMember("misc");
        startPrompt.MemberSourceRect = new ARect(68, 6, 112, 54);

        var livesAnchor = _movie.AddSprite(20, GameStartFrame, frameCount, 80f, stageHeight - 40f, sprite =>
        {
            sprite.Puppet = true;
            sprite.Visibility = false;
        });
        livesAnchor.SetMember("sprites");
        var lifeSourceRect = new ARect(160, 2, 186, 28);
        livesAnchor.MemberSourceRect = lifeSourceRect;
        livesAnchor.AddBehavior<LivesBehavior>(behavior =>
        {
            behavior.Spacing = 36f;
            behavior.ScaleFactor = 1f;
            behavior.CastLibName = "Data";
            behavior.MemberName = "sprites";
            behavior.MemberSourceRect = lifeSourceRect;
        });

        var bonusesAnchor = _movie.AddSprite(21, GameStartFrame, frameCount, stageWidth - 80f, stageHeight - 40f, sprite =>
        {
            sprite.Puppet = true;
            sprite.Visibility = false;
        });
        bonusesAnchor.SetMember("misc");
        var bonusSourceRect = new ARect(186, 4, 238, 56);
        bonusesAnchor.MemberSourceRect = bonusSourceRect;
        bonusesAnchor.AddBehavior<BonusesBehavior>(behavior =>
        {
            behavior.Spacing = 48f;
            behavior.ScaleFactor = 0.75f;
            behavior.BonusCastLibName = "Data";
            behavior.BonusMemberName = "misc";
            behavior.MemberSourceRect = bonusSourceRect;
        });

        var pelletField = _movie.AddSprite("PacMan.Pellets", sprite =>
        {
            sprite.LocH = centerX;
            sprite.LocV = centerY;
            sprite.LocZ = 10;
            sprite.Puppet = true;
            sprite.Visibility = false;
        });
        pelletField.AddBehavior<PacManPelletFieldBehavior>();

        var powerPillField = _movie.AddSprite("PacMan.PowerPills", sprite =>
        {
            sprite.LocH = centerX;
            sprite.LocV = centerY;
            sprite.LocZ = 15;
            sprite.Puppet = true;
            sprite.Visibility = false;
        });
        powerPillField.AddBehavior<PacManPowerPillFieldBehavior>();

        var roamingBonus = _movie.AddSprite("PacMan.Bonus", sprite =>
        {
            sprite.LocH = centerX;
            sprite.LocV = centerY;
            sprite.LocZ = 20;
            sprite.Puppet = true;
            sprite.Visibility = false;
        });
        roamingBonus.AddBehavior<BlPacmanAnimationBehavior>();
        roamingBonus.AddBehavior<PacManRoamingBonusBehavior>();

        var pacMan = _movie.AddSprite("PacMan.Actor", sprite =>
        {
            sprite.LocZ = 50;
            sprite.Puppet = true;
            sprite.Visibility = false;
        });
        pacMan.AddBehavior<BlPacmanAnimationBehavior>();
        pacMan.AddBehavior<PacManActorBehavior>();

        var ghostNames = new[] { "Blinky", "Pinky", "Inky", "Clyde" };
        for (var i = 0; i < ghostNames.Length; i++)
        {
            var ghost = _movie.AddSprite($"PacMan.Ghost.{ghostNames[i]}", sprite =>
            {
                sprite.LocZ = 40 + i;
                sprite.Puppet = true;
                sprite.Visibility = false;
            });
            ghost.AddBehavior<BlPacmanAnimationBehavior>();
            ghost.AddBehavior<PacManGhostBehavior>(behavior =>
            {
                behavior.GhostName = ghostNames[i];
            });
        }
    }
}

