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
            services.AddTetriGrounds(c => c
                        .WithLingoSdlEngine("TetriGrounds", 640, 480));
            var serviceProvider = services.BuildServiceProvider();

            var movie = serviceProvider.SetupGame();
            var game = serviceProvider.StartGame();
            serviceProvider.GetRequiredService<SdlRootContext>().Run();
            SdlSetup.Dispose();
        }
    }

}
