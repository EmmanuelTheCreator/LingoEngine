using Godot;
using LingoEngine.Demo.TetriGrounds.Core;
using LingoEngineGodot;
using Microsoft.Extensions.DependencyInjection;
using System;

public partial class RootNodeTetriGrounds : Node2D
{
    private ServiceCollection _services;
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
            TetriGroundsSetup.RegisterServices(_services, c => c
                    .WithLingoGodotEngine(this));
                    //.WithDirectorGodotEngine(this));
            var serviePrider = _services.BuildServiceProvider();

            var movie = TetriGroundsSetup.SetupGame(serviePrider);
            //_controller = new LingoGodotPlayerControler((Node2D)serviePrider.GetRequiredService<LingoGodotRootNode>().RootNode, movie);
            TetriGroundsSetup.StartGame(serviePrider);
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

