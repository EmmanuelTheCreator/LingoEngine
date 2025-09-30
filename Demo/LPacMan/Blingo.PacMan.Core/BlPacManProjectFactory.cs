using AbstUI.Primitives;
using AbstUI.Texts;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using Blingo.PacMan.Core.Sprites.ParentScripts;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Projects;
using BlingoEngine.Sprites;
using BlingoEngine.Setup;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;

namespace Blingo.PacMan.Core;


public class BlPacManProjectFactory : IBlingoProjectFactory
{
    public static int GameWidth => BlPacManTheme.Stage.Width;
    public static int GameHeight => BlPacManTheme.Stage.Height;

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
                    s.MaxSpriteChannelCount = 400;
                    s.StageWidth = GameWidth;
                    s.StageHeight = GameHeight;
                })
                .ForMovie(MovieName, s => s
                    .AddScriptsFromAssembly()
                )
                .ServicesBlingo(s => s
                    .AddSingleton<BlPacManCore>()
                    .AddSingleton<BlPacManRepository>()
                    .AddSingleton<IPacManRandomSource, PacManRandomSource>()
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
                    c.Add(BlingoMemberType.Bitmap, 0, "pacman", Path.Combine("Media", "Data", "pacman.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "pills", Path.Combine("Media", "Data", "pills.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "sprites", Path.Combine("Media", "Data", "sprites.png"));
                    c.Add(BlingoMemberType.Bitmap, 0, "start", Path.Combine("Media", "Data", "start.png"));
                })
            .AddCastLib("Texts", true, c => c
                .AddTextMember("T_Label_HighScore", "High Score", AbstTextAlignment.Center, 85)
                .AddTextMember("T_HighScore", "00000", AbstTextAlignment.Center, 75)
                .AddTextMember("T_Player1_Label","1UP")
                .AddTextMember("T_Player2_Label","2UP")
                .AddTextMember("T_Player1_Score","00000", AbstTextAlignment.Left, 75)
                .AddTextMember("T_Player2_Score", "00000", AbstTextAlignment.Right,75)
                .AddTextMember("T_Player1_Text","Player One", AbstTextAlignment.Center, 85, AColor.FromHex("#55eeee"))
                .AddTextMember("T_Player2_Text","Player Two", AbstTextAlignment.Center, 85, AColor.FromHex("#55eeee"))
                .AddTextMember("T_Ready","Ready!", AbstTextAlignment.Center, 75, AColor.FromHex("#ff0000"))
                .AddTextMember("T_Start","Start")
            )
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
        //if (player is null)
        //    throw new ArgumentNullException(nameof(player));

        var dataCast = player.CastLib("Data")!;
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
            return;
      
        var frameCount = Math.Max(GameStartFrame, 60);
        _movie.AddFrameBehavior<BlPacManStayOnFrameBehavior>(5);
        _movie.AddFrameBehavior<BlPacManGameBehavior>(GameStartFrame);

        var stageWidth = _blingoPlayer?.Stage.Width ?? _settings?.StageWidth ?? 730;
        var stageHeight = _blingoPlayer?.Stage.Height ?? _settings?.StageHeight ?? 500;
        var centerX = stageWidth / 2f;
        var centerY = stageHeight / 2f;
        var sprSize = TileMath.SpriteSize;
        var sprStartX = 10;
        var sprStartY = 10;

        _movie.AddSprite(1, 1, frameCount, 0, 0, sprite =>
            {
                sprite.Name = "ContinousBackground";
                sprite.Lock = true;
                sprite.SetMemberRect(ARect.New(0, 0, 10, 10));
                sprite.Width = GameWidth;
                sprite.Height = GameHeight;
            })
            .SetMember("start")
            .AddBehavior<BlPacManSoundBehavior>();

        _movie.AddSprite(PCSpriteNums.GameBG, 1, GameStartFrame - 1, 0, 0, sprite =>
            {
                sprite.Lock = true;
                sprite.Width = GameWidth;
                sprite.Height = GameHeight;
            })
            .SetMember("start")
            .AddBehavior<BlPacManMenuStartBehavior>();

        _movie.AddSprite(PCSpriteNums.BtnStart, 1, GameStartFrame - 1, (_movie.Width / 2) - 50, (_movie.Height / 2) +30).SetMember("T_Start"); // Button Start




        //_movie.AddSprite(5, GameStartFrame, frameCount, centerX, centerY, x => x.Name = "MenuStart"); //.AddBehavior<BlPacManMenuStartBehavior>(); // Start game button

        //var hudBanner = _movie.AddSprite(6, GameStartFrame, frameCount, centerX, 40f, c => c.MemberSourceRect = new ARect(63, 2, 87, 26)).SetMember("misc");

        //var startPrompt = _movie.AddSprite(7, 1, GameStartFrame - 1, centerX, 120f, c => c.MemberSourceRect = new ARect(34, 3, 56, 27)).SetMember("misc");

        _movie.AddSprite(PCSpriteNums.GameBG, GameStartFrame, frameCount, 0, 0, sprite =>
            {
                sprite.Lock = true;
                sprite.Width = GameWidth;
                sprite.Height = GameHeight;
            })
            .SetMember("maze-1");


        var row1 = 5;
        var row2 = 14;
        var rightOffset = 30;
        _movie.AddSprite(PCSpriteNums.T_Label_HighScore , GameStartFrame, frameCount, (_movie.Width / 2) - 40, row1).SetMember("T_Label_HighScore");
        _movie.AddSprite(PCSpriteNums.T_HighScore , GameStartFrame, frameCount, (_movie.Width / 2) - 40, row2).SetMember("T_HighScore");
        _movie.AddSprite(PCSpriteNums.T_Player1_Label , GameStartFrame, frameCount, 10, row1).SetMember("T_Player1_Label");
        _movie.AddSprite(PCSpriteNums.T_Player2_Label , GameStartFrame, frameCount, _movie.Width - rightOffset, row1).SetMember("T_Player2_Label");
        _movie.AddSprite(PCSpriteNums.T_Player1_Score , GameStartFrame, frameCount, 10, row2).SetMember("T_Player1_Score");
        _movie.AddSprite(PCSpriteNums.T_Player2_Score , GameStartFrame, frameCount, _movie.Width - rightOffset-50, row2).SetMember("T_Player2_Score");
        _movie.AddSprite(PCSpriteNums.T_Player1_Text ,  GameStartFrame, frameCount, (_movie.Width / 2) - 40, (_movie.Height / 2)-32).SetMember("T_Player1_Text");
        _movie.AddSprite(PCSpriteNums.T_Player2_Text , GameStartFrame, frameCount, (_movie.Width / 2) - 40, (_movie.Height / 2)-32).SetMember("T_Player2_Text");
        _movie.AddSprite(PCSpriteNums.T_Ready, GameStartFrame, frameCount, (_movie.Width / 2) - 40, (_movie.Height / 2)+ 18).SetMember("T_Ready");
        
        var roamingBonus = _movie.AddSprite(PCSpriteNums.BonusesRoaming, GameStartFrame, frameCount, sprStartX, sprStartY + sprSize*2, sprite =>
            {
                sprite.Name = "Bonuses.Roaming";
                sprite.SetMemberRect(BlPacManRoamingBonusBehavior.DefaultAnimationRect);
            })
            .SetMember("misc")
            .AddBehavior<BlPacManAnimationBehavior>()
            .AddBehavior<BlPacManRoamingBonusBehavior>();

        var pacMan = _movie.AddSprite(PCSpriteNums.PacMan, GameStartFrame, frameCount, sprStartX, sprStartY + sprSize, sprite =>
            {
                //sprite.Visibility = false;
                sprite.Name = "PacMan";
                var rect = new ARect(0, 0, sprSize, sprSize);
                sprite.SetMemberRect(rect, new APoint(rect.Width / 2f, rect.Height / 2f));
            })
            .SetMember("sprites")
            .AddBehavior<BlPacManAnimationBehavior>()
            .AddBehavior<BlPacManActorBehavior>();

        var ghostNames = BlGhostManager.GhostNames;
        for (var i = 0; i < ghostNames.Length; i++)
        {
            var ghost = _movie.AddSprite(PCSpriteNums.GhostStart + i, GameStartFrame, frameCount, sprStartX + i* sprSize, sprStartY, sprite =>
                {
                    sprite.Name = $"Ghost.{ghostNames[i]}";
                    var rect = ARect.New(i * sprSize, (i + 1) * sprSize, sprSize, sprSize);
                    sprite.SetMemberRect(rect, new APoint(rect.Width / 2f, rect.Height / 2f));
                })
                .SetMember("sprites")
                .AddBehavior<BlPacManAnimationBehavior>()
                .AddBehavior<BlPacManGhostBehavior>(behavior =>
                {
                    behavior.GhostName = ghostNames[i];
                })
                ;
        }
    }
   

}

internal static class FactoryExtensions
{
    public static IBlingoCast AddTextMember(this IBlingoCast blingoCast, string name, string text, AbstTextAlignment align = AbstTextAlignment.Left, int width = 0, AColor? color = null)
    {
        blingoCast.Add<BlingoMemberField>(0, name, c =>
        {
            c.Font = "press-start-2p";
            c.FontSize = 8;
            c.Color = color != null? color.Value: AColor.FromHex("#efefef");
            if (align != AbstTextAlignment.Left)
            {
                c.Width = width;
                c.Alignment = align;
            }
            c.Text = text.ToUpper();
        });
        return blingoCast;
    }
}