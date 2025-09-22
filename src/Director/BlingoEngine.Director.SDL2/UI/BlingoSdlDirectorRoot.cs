using AbstUI.SDL2;
using AbstUI.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.LGodot;
using BlingoEngine.Projects;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlingoEngine.Director.SDL2.UI
{
    public class BlingoSdlDirectorRoot : IDisposable
    {
        private DirSDLMainMenu _dirMainMenu;

        public BlingoSdlDirectorRoot(IAbstSDLRootContext rootContext, BlingoPlayer player, IServiceProvider serviceProvider, BlingoProjectSettings settings)
        {
            var windowManager= serviceProvider.GetRequiredService<IAbstWindowManager>();

            _dirMainMenu = serviceProvider.GetRequiredService<DirSDLMainMenu>();
            //_dirMainMenu = serviceProvider.GetRequiredService<DirectorMainMenu>();
            _dirMainMenu.Init(_dirMainMenu.MainMenu);
            _dirMainMenu.MainMenu.OpenWindow();
            //rootContext.ComponentContainer.AddChild(_dirMainMenu);

            //godotWindowManager.RootNode.AddChild(_dirGodotMainMenu);

            windowManager.OpenWindow(DirectorMenuCodes.PropertyInspector);
            windowManager.OpenWindow(DirectorMenuCodes.CastWindow);
            windowManager.OpenWindow(DirectorMenuCodes.ScoreWindow);
            windowManager.OpenWindow(DirectorMenuCodes.StageWindow);
          //  windowManager.OpenWindow(DirectorMenuCodes.ToolsWindow);
        }

        public void Dispose()
        {

        }
    }
}

