// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using AbstUI.Primitives;
using AbstUI.Resources;
using BlingoEngine.Animations;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using BlingoEngine.Demo.TetriGrounds.Core.Sprites.Globals;
using BlingoEngine.FilmLoops;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Primitives;
using BlingoEngine.Projects;
using BlingoEngine.Setup;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Texts;
using BlingoEngine.Transitions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BlingoEngine.Demo.TetriGrounds.Core;

/// <summary>
/// Registers all TetriGrounds services, content and movie wiring so the demo can be bootstrapped in any host.
/// </summary>
public class TetriGroundsProjectFactory : IBlingoProjectFactory
{
    /// <summary>
    /// Name used throughout the project when referring to the root movie.
    /// </summary>
    public const string MovieName = "TetriGrounds";
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
            //.AddFont("Arcade", Path.Combine("Media", "Fonts", "arcade.ttf"))
            //.AddFont("Bikly", Path.Combine("Media", "Fonts", "bikly.ttf"))
            //.AddFont("8Pin Matrix", Path.Combine("Media", "Fonts", "8PinMatrix.ttf"))
            //.AddFont("Earth", Path.Combine("Media", "Fonts", "earth.ttf"))
            //.AddFont("Tahoma", Path.Combine("Media", "Fonts", "Tahoma.ttf"))
            .WithProjectSettings(s =>
                {
                    s.ProjectFolder = "..\\";
                    s.ProjectName = "TetriGrounds";
                    s.CodeFolder = "..\\BlingoEngine.Demo.TetriGrounds.Core\\";
                    s.MaxSpriteChannelCount = 300;
                    s.MaxSpriteChannelCount = 800;
                    s.StageWidth = 730;
                    s.StageHeight = 500; // original : 547
                })
                .ForMovie(MovieName, s => s
                    .AddScriptsFromAssembly()

                // As an example, you can add them manually too:

                // .AddMovieScript<StartMoviesScript>() => MovieScript
                // .AddBehavior<MouseDownNavigateBehavior>()  -> Behavior
                // .AddParentScript<BlockParentScript>() -> Parent script
                )
                .ServicesBlingo(s => s
                    .AddSingleton<IArkCore, TetriGroundsCore>()
                    .AddSingleton<ScoresRepository>()
                    )
                ;
    }



    /// <summary>
    /// Loads all required cast libraries and keeps a reference to the player for later movie initialization.
    /// </summary>
    public async Task LoadCastLibsAsync(IBlingoCastLibsContainer castlibContainer, BlingoPlayer blingoPlayer)
    {

        _blingoPlayer = blingoPlayer;

        await blingoPlayer.LoadCastLibFromCsvAsync("InternalExt", Path.Combine("Media", "InternalExt", "Members.csv"), true);
        await blingoPlayer.LoadCastLibFromCsvAsync("Data", Path.Combine("Media", "Data", "Members.csv"));
        blingoPlayer.AddCastLib("Sounds", true, c =>
        {
            c.Add(BlingoMemberType.Sound, 0, "S_Click", Path.Combine("Media", "Sounds", "click.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_BtnStart", Path.Combine("Media", "Sounds", "btnstart.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_Gong", Path.Combine("Media", "Sounds", "gong.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_DeleteRow", Path.Combine("Media", "Sounds", "deleterow.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_GO", Path.Combine("Media", "Sounds", "go.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_LevelUp", Path.Combine("Media", "Sounds", "level_up.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_Shhh1", Path.Combine("Media", "Sounds", "shhh1.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_Terminated", Path.Combine("Media", "Sounds", "terminated.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_Died", Path.Combine("Media", "Sounds", "die.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_Nature", Path.Combine("Media", "Sounds", "nature.mp3"));

            c.Add(BlingoMemberType.Sound, 0, "S_BlockFall1", Path.Combine("Media", "Sounds", "blockfall_1.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_BlockFall2", Path.Combine("Media", "Sounds", "blockfall_2.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_BlockFall3", Path.Combine("Media", "Sounds", "blockfall_3.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_BlockFall4", Path.Combine("Media", "Sounds", "blockfall_4.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_BlockFall5", Path.Combine("Media", "Sounds", "blockfall_5.mp3"));

            c.Add(BlingoMemberType.Sound, 0, "S_RowsDeleted_2", Path.Combine("Media", "Sounds", "rows_deleted_2.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_RowsDeleted_3", Path.Combine("Media", "Sounds", "rows_deleted_3.mp3"));
            c.Add(BlingoMemberType.Sound, 0, "S_RowsDeleted_4", Path.Combine("Media", "Sounds", "rows_deleted_4.mp3"));
        });
        blingoPlayer.CastLib("Sounds").GetMember<BlingoMemberSound>("S_Nature")!.Loop = true;
        InitMembers(blingoPlayer);
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
    /// Builds the TetriGrounds movie, registers score labels and kicks off sprite initialization.
    /// </summary>
    public IBlingoMovie LoadMovie(IBlingoPlayer blingoPlayer)
    {
        _movie = blingoPlayer.NewMovie(MovieName);
        _movie.GetRequiredService<IAbstResourceManager>().ProjectFolder = "TetriGrounds";
        _movie.GetRequiredService<ScoresRepository>().LoadHighScores(_movie);
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
        _movie.SetScoreLabel(75, "FilmLoop Test");
    }

    /// <summary>
    /// Initializes static member properties that were previously stored as document metadata.
    /// </summary>
    public void InitMembers(BlingoPlayer player)
    {
        //var textColor = AColor.FromHex("#999966");
        //var text = player.CastLib(2).GetMember<BlingoMemberText>("T_data");
        //text!.Color = textColor;
        CreateBirdFilmLoop(player);
    }

    /// <summary>
    /// Recreates the meticulously hand-positioned sprite setup from the original movie.
    /// </summary>
    public void InitSprites()
    {
        if (_movie == null) return;

        //TestTextChanging();
        //TestFilmLoops();
        //return;

        //_movie.AddSprite(1, 1, 64, 519, 343).SetMember("bell0039")
        //    .AddBehavior<AnimationScriptBehavior>(b =>
        //    {
        //        b.myStartMembernum = 67;
        //        b.myEndMembernum = 108;
        //        b.mySlowDown = 2;
        //        b.myValue = -1;
        //        // My Sprite that contains info
        //        b.myDataSpriteNum = 1;
        //        // Name Info
        //        b.myDataName = "1";
        //        b.myWaitbeforeExecute = 0;
        //        //b.myFunction = 70;
        //    });
        //_movie.AddFrameBehavior<GameStopBehavior>(10);


        //_movie.AddSprite(1, 1, 64, 519, 343) //, c => c.InkType = Primitives.BlingoInkType.BackgroundTransparent)
        //    .SetMember("B_Play")
        //    .AddBehavior<ButtonStartGameBehavior>();
        //_movie.AddFrameBehavior<GameStopBehavior>(10);
        //return;
        //TestPuppetSprite();
        //return;

        new TetriGroundsMembersSetup(_blingoPlayer!).InitMembers();
        var castData = _movie.CastLib["Data"];
        var MyBG = _movie.Member["Game"];


        _movie.Transitions.Add(54, new BlingoTransitionFrameSettings { Duration = 0.5f, TransitionId = 37 }); // 37.: Venetian blinds
        _movie.AddFrameBehavior<GameStopBehavior>(60);
        //_movie.AddFrameBehavior<WaiterFrameScript>(1);
        //_movie.AddFrameBehavior<StayOnFrameFrameScript>(4);
        //_movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(11, b => b.TickWait = 60);
        _movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(2, b => { b.TickWait = 1; b.FrameOffsetOnClick = 40; });

        _movie.AddSprite(4, 54, 64, 336, 241).AddBehavior<BgScriptBehavior>().SetMember("Game");// BG GAme
        _movie.AddSprite(5, 56, 64, 591, 36, c => { c.Width = 193; c.Height = 35; }).SetMember("TetriGrounds_s"); // LOGO
        _movie.AddSprite(6, 59, 64, 503, 438).SetMember(9, 2); // copyright text
        var sprite = _movie.AddSprite(7, 60, 64, 441, 92).SetMember("T_data"); // level





        // Button play
        _movie.AddSprite(9, 60, 64, 519, 343).SetMember("B_Play").AddBehavior<ButtonStartGameBehavior>(); // Button play
        var memberT_NewGame = _movie.Member["T_NewGame"];
        _movie.AddSprite(11, 60, 64, 497, 334).SetMember(memberT_NewGame); // Text New Game on Button

        var memberScore = _movie.Member["T_Score"];
        _movie.AddSprite(12, 60, 64, 486, 148).SetMember(memberScore);

        Func<ExecuteBehavior, int, ExecuteBehavior> configureBExecute = (c, spritenum) =>
        {
            c.mySpriteNum = spritenum;
            c.myEnableMouseClick = true;
            c.myEnableMouseRollOver = true;
            c.myRollOverMember = -1;
            return c;
        };
        Func<TextCounterBehavior, int, string, TextCounterBehavior> configureTextCounter = (c, maxValue, dataName) =>
        {
            c.myMin = 0;
            c.myMax = maxValue;
            c.myValue = 0;
            c.myStep = 1;
            c.myDataSpriteNum = 0;
            c.myDataName = dataName; // "StartLines";
            c.myWaitbeforeExecute = 200;
            c.myFunction = "SendData";
            return c;
        };

        _movie.AddSprite(14, 54, 64, 507, 414).SetMember(47, 2); // Text start lines
        _movie.AddSprite(15, 54, 64, 619, 386).SetMember("B_back").AddBehavior<ExecuteBehavior>(c => configureBExecute(c, 17).myFunction = "Deletee"); // btn back
        _movie.AddSprite(16, 54, 64, 663, 387).SetMember("B_more").AddBehavior<ExecuteBehavior>(c => configureBExecute(c, 17).myFunction = "Addd"); ; // btn next
        _movie.AddSprite(17, 54, 64, 629, 381).SetMember("T_StartLevel").AddBehavior<TextCounterBehavior>(c => { configureTextCounter(c, 15, "StartLevel").myMin = 1; c.myValue = 1; }); // Value start level
        _movie.AddSprite(18, 54, 64, 507, 379).SetMember(48, 2); // Text start level
        _movie.AddSprite(19, 54, 64, 620, 422).SetMember("B_back2").AddBehavior<ExecuteBehavior>(c => configureBExecute(c, 21).myFunction = "Deletee"); ; // btn back
        _movie.AddSprite(20, 54, 64, 663, 423).SetMember("B_more").AddBehavior<ExecuteBehavior>(c => configureBExecute(c, 21).myFunction = "Addd"); ; // btn next
        _movie.AddSprite(21, 54, 64, 629, 419).SetMember("T_StartLines").AddBehavior<TextCounterBehavior>(c => configureTextCounter(c, 10, "StartLines")); // Value start lines

        _movie.AddSprite(22, 55, 64, 463, 62).SetMember("bell0039") // Bell anim
            .AddBehavior<AnimationScriptBehavior>(b =>
            {
                b.myStartMembernum = 100;
                b.myEndMembernum = 140;
                b.myValue = 100;
                b.mySlowDown = 2;
                // My Sprite that contains info
                b.myDataSpriteNum = 1;
                // Name Info
                b.myDataName = "1";
                b.myWaitbeforeExecute = 0;
                //b.myFunction = 70;
            });

        _movie.AddSprite(24, 59, 64, 94, 129).SetMember("T_InternetScoresNames");
        _movie.AddSprite(25, 59, 64, 159, 132).SetMember("T_InternetScores");
        _movie.AddSprite(26, 59, 64, 91, 50).SetMember(39, 2); // Text highscores
        _movie.AddSprite(27, 59, 64, 91, 50).SetMember(39, 2); // Text All
        _movie.AddSprite(28, 59, 64, 94, 113).SetMember(41, 2); // Text personal
        _movie.AddSprite(29, 59, 64, 95, 313).SetMember("T_InternetScoresNamesP");
        _movie.AddSprite(30, 59, 64, 151, 313).SetMember("T_InternetScoresP");
        _movie.AddSprite(35, 59, 64, 363, 238, c => c.Blend = 0).SetMember("pause").Visibility = false;
        // Start Animation
        var memberLoading = _movie.CastLib.GetMember<IBlingoMemberTextBase>(56, 2)!;
        var logoSprite1 = (IBlingoSprite2DLight)_movie.AddSprite(3, 2, 53, 600, 60).SetMember("TetriGrounds_s");
        logoSprite1.AddKeyframes(
            new BlingoKeyFrameSetting { Frame = 1, Position = new APoint(600, 60), Blend = 10 },
            new BlingoKeyFrameSetting { Frame = 10, Position = new APoint(306, 124), Blend = 100 },
            new BlingoKeyFrameSetting { Frame = 20, Position = new APoint(356, 237), Blend = 100 },
            (22, 366, 252), (24, 356, 237));
        var loadingSprite1 = (IBlingoSprite2DLight)_movie.AddSprite(2, 19, 53, 132, 268).SetMember(memberLoading);
        var birdSprite1 = (IBlingoSprite2DLight)_movie.AddSprite(6, 2, 31, 751, 394).SetMember("BirdAnim");
        birdSprite1.AddKeyframes((1, 751, 394), (16, 444, 273), (31, -33, 316));

        _movie.AddSprite(36, 59, 64, 363, 238, c => { c.LocZ = 500; }).SetMember("alert"); //.Visibility = false;
        _movie.AddSprite(37, 59, 64, 200, 200, c => { c.LocZ = 502; }).SetMember("T_PopupTitle"); // the player name text member
        _movie.AddSprite(38, 59, 64, 200, 230, c => { c.LocZ = 502; }).SetMember("T_InputText").AddBehavior<EnterHighScoreBehavior>(c => c.SetSpriteNums([36, 37, 38])); // the player name text member
    }



    /// <summary>
    /// Builds the looping bird animation that flutters across the intro sequence.
    /// </summary>
    private void CreateBirdFilmLoop(IBlingoPlayer blingoPlayer)
    {
        var dataCastlib = blingoPlayer.CastLib("Data")!;
        var birdAnim = dataCastlib.GetMember<BlingoFilmLoopMember>("BirdAnim")!;
        birdAnim.Loop = true;
        birdAnim.AddSprite(dataCastlib.Member["mouse0000"]!, 1, 1, 1);
        birdAnim.AddSprite(dataCastlib.Member["mouse0001"]!, 1, 2, 2);
        birdAnim.AddSprite(dataCastlib.Member["mouse0002"]!, 1, 3, 3);
        birdAnim.AddSprite(dataCastlib.Member["mouse0003"]!, 1, 4, 4);
        birdAnim.AddSprite(dataCastlib.Member["mouse0004"]!, 1, 5, 5);
        birdAnim.AddSprite(dataCastlib.Member["mouse0005"]!, 1, 6, 6);
        birdAnim.AddSprite(dataCastlib.Member["mouse0004"]!, 1, 7, 7);
        birdAnim.AddSprite(dataCastlib.Member["mouse0003"]!, 1, 8, 8);
        birdAnim.AddSprite(dataCastlib.Member["mouse0002"]!, 1, 9, 9);
        birdAnim.AddSprite(dataCastlib.Member["mouse0001"]!, 1, 10, 10);
        birdAnim.AddSprite(dataCastlib.Member["mouse0000"]!, 1, 11, 11);
    }

    /// <summary>
    /// Quick helper used while porting film loops from Director; remains here for debugging.
    /// </summary>
    private void TestFilmLoops()
    {
        _movie!.AddSprite(5, 3, 299, 50, 150, c =>
        {
            c.Name = "BirdAnim2";
            //c.AddKeyframes((7, 70, 250), (15, 80, 250), (20, 50, 150));
        }).SetMember("BirdAnim");
        var birdAnim = _blingoPlayer!.CastLib("Data")!.GetMember<BlingoFilmLoopMember>("BirdAnim")!;
        var spritebirds = new BlingoFilmLoopMemberSprite(birdAnim)
        {
            LocH = 0,
            LocV = 0,
            InkType = BlingoInkType.Matte,
            BeginFrame = 1,
            EndFrame = 60,
            Channel = 1,
            Name = "birdV_Root"
        };

        _movie.AddFrameBehavior<StayOnFrameFrameScript>(20);
    }

    /// <summary>
    /// Diagnostic helper that validates text updates and colour changes at runtime.
    /// </summary>
    private void TestTextChanging()
    {
        var castData = _movie!.CastLib["Data"];
        var textMember = castData.GetMember<IBlingoMemberTextBase>("T_data")!;
        textMember.Width = 191;

        _movie.AddSprite(2, 2, 10, 441, 92).SetMember("Block1");
        var sprite = _movie.AddSprite(3, 2, 10, 441, 92).SetMember("T_data"); // level
        _movie!.AddFrameBehavior<TestTetrigroundsBehavior>(5);
    }

    /// <summary>
    /// Manual experiment that explores puppet-sprite behaviour when porting from Director.
    /// </summary>
    private void TestPuppetSprite()
    {
        _movie!.AddSprite(5, 2, 64, 519, 343).SetMember("B_Play").AddBehavior<ButtonStartGameBehavior>(); // Button play
        _movie.AddFrameBehavior<StayOnFrameFrameScript>(10);
    }
}

