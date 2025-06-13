using LingoEngine.Demo.TetriGrounds.Core;
using LingoEngine.SDL2;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Demo.TetriGrounds.SDL2
{

    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.RegisterServices(c => c
                        .WithLingoSdlEngine("TetriGrounds", 640, 480));
            var serviceProvider = services.BuildServiceProvider();

            TetriGroundsSetup.SetupGame(serviceProvider);
            TetriGroundsGame game = serviceProvider.StartGame();
            serviceProvider.GetRequiredService<SdlRootContext>().Run(game.LingoPlayer);
        }
    }

}
