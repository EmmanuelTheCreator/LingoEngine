using LingoEngine;
using LingoEngine.Core;
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
            _lingoPlayer.AddCastLib("Data",c => c.Add(LingoMemberType.Bitmap, 1, "MyBG", "/Medias/Data/Game.jpg"));
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
            _movie.SetScoreLabel(1, "Start");
        }
        public void InitSprites()
        {
            var MyBG = _movie.Member["MyBG"];
            _movie.AddFrameBehavior<StartGameBehavior>(3);
            _movie.AddFrameBehavior<StayOnFrameFrameScript>(4);
            _movie.AddFrameBehavior<MouseDownNavigateWithStayBehavior>(11, b => b.TickWait = 60);
            _movie.AddSprite(1, 1, 99, 320, 240).SetMember(MyBG);

        }



    }
}
