using System;
using System.IO;
using AbstUI.Primitives;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.Behaviors;
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

namespace Blingo.PacMan.Core;


public class PacManProjectFactory : IBlingoProjectFactory
{
    /// <summary>
    /// Name used throughout the project when referring to the root movie.
    /// </summary>
    public const string MovieName = "Blingo PacMan";
    private BlingoProjectSettings? _settings;
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
                    _settings = s;
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
        if (player is null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        var dataCast = player.CastLib("Data");
        if (dataCast is null)
        {
            return;
        }

        // The original Director project stored registration points on each bitmap so the
        // timeline coordinates line up with the hand-authored stage layout. We no longer
        // have that metadata, so we preload each sheet and explicitly assign the anchors
        // that the JavaScript remake expected. Large background images use a top-left
        // registration point because the Lingo score positioned them by absolute offsets,
        // while character sheets keep a centred registration point so sub-rect selections
        // remain anchored in the middle of the sprite.
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
            // Background bitmaps were positioned via raw offsets in Director, so we keep
            // the registration point at the top-left corner. This mirrors the coordinate
            // system from the score where the first pixel sits at (0,0).
            ConfigureBitmap(name, _ => new APoint(0f, 0f));
        }

        var centredSheets = new[] { "characters", "misc", "mspacman", "pills", "sprites" };
        foreach (var name in centredSheets)
        {
            // Sprite sheets were trimmed in the original authoring tool with the reg point
            // centred on each frame. We mimic that behaviour by centring the registration
            // point on the full sheet. Individual sprite source rectangles stay aligned
            // around their mid-points which matches the JavaScript remake's math.
            ConfigureBitmap(name, m => new APoint(m.Width / 2f, m.Height / 2f));
        }

        var soundsCast = player.CastLib("Sounds");
        if (soundsCast is null)
        {
            return;
        }

        // The original score looped the backing track on channel 1 so the attract screen
        // ambience keeps running. Matching that behaviour here prevents the music from
        // stopping after the first play-through.
        var backgroundSound = soundsCast.GetMember<BlingoMemberSound>("S_back");
        if (backgroundSound is not null)
        {
            backgroundSound.Loop = true;
        }
    }

    /// <summary>
    /// Recreates the meticulously hand-positioned sprite setup from the original movie.
    /// </summary>
    public void InitSprites()
    {
        if (_movie == null)
        {
            return;
        }

        var frameCount = Math.Max(1, _movie.FrameCount);
        _movie.AddFrameBehavior<PacManStayOnFrameBehavior>(1);

        var stageWidth = _blingoPlayer?.Stage.Width ?? _settings?.StageWidth ?? 730;
        var stageHeight = _blingoPlayer?.Stage.Height ?? _settings?.StageHeight ?? 500;
        var centerX = stageWidth / 2f;
        var centerY = stageHeight / 2f;

        var background = _movie.AddSprite(1, 1, frameCount, -230, -230, sprite => sprite.Lock = true);
        background.SetMember("start");

        var controller = _movie.AddSprite("PacMan.Controller", sprite =>
        {
            sprite.BeginFrame = 1;
            sprite.EndFrame = frameCount;
            sprite.LocH = centerX;
            sprite.LocV = centerY;
            sprite.LocZ = 100;
            sprite.Puppet = true;
            sprite.Lock = true;
            sprite.Visibility = false;
        });
        controller.AddBehavior<PacManGameBehavior>();

        var hudBanner = _movie.AddSprite(5, 1, frameCount, centerX, 40f, sprite =>
        {
            sprite.Puppet = true;
            sprite.Lock = true;
        });
        hudBanner.SetMember("misc");
        hudBanner.MemberSourceRect = new ARect(126, 4, 174, 52);

        var startPrompt = _movie.AddSprite(6, 1, frameCount, centerX, 120f, sprite =>
        {
            sprite.Puppet = true;
            sprite.Lock = true;
        });
        startPrompt.SetMember("misc");
        startPrompt.MemberSourceRect = new ARect(68, 6, 112, 54);

        var livesAnchor = _movie.AddSprite(20, 1, frameCount, 80f, stageHeight - 40f, sprite =>
        {
            sprite.Puppet = true;
            sprite.Lock = true;
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

        var bonusesAnchor = _movie.AddSprite(21, 1, frameCount, stageWidth - 80f, stageHeight - 40f, sprite =>
        {
            sprite.Puppet = true;
            sprite.Lock = true;
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
    }
}

