using LingoEngine;
using LingoEngine.Core;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals;
using LingoEngine.Movies;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.TetriGrounds.Core
{
    public class TetriGroundsGame
    {
        public const string MovieName = "Aaark Noid";
        private readonly Dictionary<int, int> _blocks = new();
        private readonly IServiceProvider _serviceProvider;
        private LingoPlayer _lingoPlayer;
        private ILingoMovie _movie;


        public LingoPlayer LingoPlayer => _lingoPlayer;

        public TetriGroundsGame(IServiceProvider serviceProvider)
        {
            _lingoPlayer = serviceProvider.GetRequiredService<ILingoEngineRegistration>().Build();
            _serviceProvider = serviceProvider;
        }
        public ILingoMovie LoadMovie()
        {
            _lingoPlayer
                .LoadCastLibFromCsv("Data", Path.Combine("Medias", "Data", "Members.csv"))
                .LoadCastLibFromCsv("InternalExt", Path.Combine("Medias", "InternalExt", "Members.csv"))
                ;
            //_lingoPlayer.AddCastLib("Data",c => c.Add(LingoMemberType.Bitmap, 1, "MyBG", "/Medias/Data/Game.jpg"));
            _movie = _lingoPlayer.NewMovie(MovieName);

            AddLabels();
            try
            {
                InitSprites();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return _movie;
        }
        public void Play()
        {
            _movie.Play();
        }
        private void AddLabels()
        {
            _movie.SetScoreLabel(11, "Game");
        }
        public void InitSprites()
        {
            var MyBG = _movie.Member["MyBG"];
            _movie.AddFrameBehavior<GameStopBehavior>(3);
            //_movie.AddFrameBehavior<StayOnFrameFrameScript>(4);
            //_movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(11, b => b.TickWait = 60);
            _movie.AddSprite(1, 1, 15, 320, 240).AddBehavior<AppliBgBehavior>().SetMember(MyBG);
            _movie.AddSprite(2, 1, 5, 320, 240).SetMember(25);
            _movie.AddSprite(4, 3, 15, 320, 240).AddBehavior<BgScriptBehavior>().SetMember("Game");
            _movie.AddSprite(5, 1, 5, 320, 240).SetMember("TetriGrounds_s");
            _movie.AddSprite(7, 11, 15, 320, 240).SetMember("T_data");
            _movie.AddSprite(9, 11, 15, 320, 240).SetMember("B_Play");

        }



    }
}
