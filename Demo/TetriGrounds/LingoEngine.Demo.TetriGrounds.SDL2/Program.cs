using LingoEngine.Demo.TetriGrounds.Core;
using LingoEngineSDL2;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.Tetris.SDL2
{

    internal class Program
    {
        static void Main(string[] args)
        {
            var _services = new ServiceCollection();
            TetriGroundsSetup.RegisterServices(_services, c => c
                        .WithLingoSdlEngine());
            var serviePrider = _services.BuildServiceProvider();

            var movie = TetriGroundsSetup.SetupGame(serviePrider);
            TetriGroundsSetup.StartGame(serviePrider);
        }
    }

}
