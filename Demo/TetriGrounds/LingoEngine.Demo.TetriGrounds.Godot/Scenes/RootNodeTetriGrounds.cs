using Godot;
using LingoEngine.Demo.TetriGrounds.Core;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.LGodot.Movies;
using LingoEngine.Director.Core.Events;
using LingoEngine.LGodot;
using LingoEngine.LGodot.Movies;
using Microsoft.Extensions.DependencyInjection;
using System;
using LingoEngine.Director.Core;

public partial class RootNodeTetriGrounds : Node2D
{
    private ServiceCollection _services;
    private LingoGodotPlayerControler _controller;

    //private LingoGodotPlayerControler _controller;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        try
        {
            //#if DEBUG
            //            var timer = new Timer
            //            {
            //                WaitTime = 0.1,
            //                OneShot = true,
            //                Autostart = true
            //            };

            //            // Add the timer to the scene
            //            AddChild(timer);

            //            // Connect timeout to move the window to second screen
            //            timer.Timeout += () =>
            //            {
            //                int targetScreen = 1; // 0 = primary, 1 = second monitor
            //                //Vector2I screenPosition = DisplayServer.ScreenGetPosition(targetScreen);
            //                //DisplayServer.WindowSetPosition(screenPosition);
            //                DisplayServer.WindowSetPosition(new Vector2I(2000, 250));
            //                RemoveChild(timer);
            //            };
            //#endif


            _services = new ServiceCollection();
            TetriGroundsSetup.AddTetriGrounds(_services, c => c
                    .WithLingoGodotEngine(this)
#if DEBUG
                    .WithDirectorEngine()
                    .WithDirectorGodotEngine(this)
#endif
                    );
            var serviceProvider = _services.BuildServiceProvider();

            var movie = TetriGroundsSetup.SetupGame(serviceProvider);
            var game = TetriGroundsSetup.StartGame(serviceProvider);
#if DEBUG
            _controller = new LingoGodotPlayerControler(
                (Node2D)serviceProvider.GetRequiredService<LingoGodotRootNode>().RootNode,
                movie,
                serviceProvider.GetRequiredService<IDirectorEventMediator>());
#endif

        }
        catch (Exception ex)
        {
            GD.PrintErr(ex);
        }

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}

