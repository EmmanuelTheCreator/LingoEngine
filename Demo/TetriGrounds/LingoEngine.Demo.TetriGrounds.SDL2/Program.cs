using LingoEngine.Demo.TetriGrounds.Core;
using LingoEngineSDL2;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.Tetris.SDL2
{

    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            TetriGroundsSetup.RegisterServices(services, c => c
                        .WithLingoSdlEngine("TetriGrounds", 640, 480));
            var serviceProvider = services.BuildServiceProvider();

            TetriGroundsSetup.SetupGame(serviceProvider);
            var game = TetriGroundsSetup.StartGame(serviceProvider);

            SdlGameHost.Run(serviceProvider, game.LingoPlayer.Clock);
        }
    }

}
