using Godot;
using LingoEngine.Demo.TetriGrounds.Core;
#if DEBUG
using LingoEngine.Director.LGodot;
using LingoEngine.Director.LGodot.Gfx;
#else
using LingoEngine.LGodot;
#endif
using Microsoft.Extensions.DependencyInjection;
using System;

public partial class RootNodeTetriGrounds : Node2D
{
    private ServiceCollection _services;
#if DEBUG
    private LingoGodotDirectorRoot _director;
#endif

    //private LingoGodotPlayerControler _controller;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        try
        {
#if DEBUG
            ProjectSettings.SetSetting("display/window/stretch/mode", "disabled");
            ProjectSettings.SetSetting("display/window/stretch/aspect", "ignore");
            DisplayServer.WindowSetSize(new Vector2I(1600, 1000));
#else
            ProjectSettings.SetSetting("display/window/size/initial_position_type","3");
            ProjectSettings.SetSetting("display/window/stretch/mode","canvas_items");
            ProjectSettings.SetSetting("display/window/stretch/aspect", "keep");
            DisplayServer.WindowSetSize(new Vector2I(730, 546));
#endif            
            //DisplayServer.WindowSetPosition((DisplayServer.ScreenGetSize() - DisplayServer.WindowGetSize()) / 2);

            var screenSize = DisplayServer.ScreenGetSize();
            var windowSize = DisplayServer.WindowGetSize();

            var centeredPos = (screenSize - windowSize) / 2;
            centeredPos.Y += 400; // Shift window down by 200px

            DisplayServer.WindowSetPosition(centeredPos);
            _services = new ServiceCollection();
            TetriGroundsSetup.AddTetriGrounds(_services, c => c
#if DEBUG
                    .WithDirectorGodotEngine(this)
#else
                    .WithLingoGodotEngine(this)
#endif
                    );
            var serviceProvider = _services.BuildServiceProvider();

#if DEBUG
            var movie = TetriGroundsSetup.SetupGame(serviceProvider);
            var game = TetriGroundsSetup.StartGame(serviceProvider);
            _director = new LingoGodotDirectorRoot(movie, game.LingoPlayer, serviceProvider);
#else
            TetriGroundsSetup.SetupGame(serviceProvider);
            TetriGroundsSetup.StartGame(serviceProvider);
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

