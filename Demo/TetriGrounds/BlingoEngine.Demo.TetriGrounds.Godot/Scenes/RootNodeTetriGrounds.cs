using Godot;
using BlingoEngine.Setup;
using BlingoEngine.LGodot;
#if DEBUG
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.LGodot;
#endif
using Microsoft.Extensions.DependencyInjection;
using System;

public partial class RootNodeTetriGrounds : Node2D
{
    private ServiceCollection _services = null!;

    //private BlingoGodotPlayerControler _controller;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        try
        {
            /*
 // TODO:  Apply Director UI theme from IoC
 var style = serviceProvider.GetRequiredService<BlingoGodotStyle>();
 this.Theme = style.Theme;
*/
#if DEBUG
            ProjectSettings.SetSetting("display/window/stretch/mode", "disabled");
            ProjectSettings.SetSetting("display/window/stretch/aspect", "ignore");
            DisplayServer.WindowSetSize(new Vector2I(1600, 970));
#else
            ProjectSettings.SetSetting("display/window/size/initial_position_type", "3");
            ProjectSettings.SetSetting("display/window/stretch/mode", "canvas_items");
            ProjectSettings.SetSetting("display/window/stretch/aspect", "keep");
            DisplayServer.WindowSetSize(new Vector2I(730, 500));
#endif
            //DisplayServer.WindowSetPosition((DisplayServer.ScreenGetSize() - DisplayServer.WindowGetSize()) / 2);

            var screenSize = DisplayServer.ScreenGetSize();
            var windowSize = DisplayServer.WindowGetSize();

            var centeredPos = (screenSize - windowSize) / 2;
            centeredPos.Y += 200; // Shift window down by 200px
            centeredPos.X -= 200;
            DisplayServer.WindowSetPosition(centeredPos);
            _services = new ServiceCollection();
            _services.RegisterBlingoEngine(c => c

#if DEBUG
                    .WithDirectorGodotEngine(this)
                     .AddBuildAction(b =>
                     {
                         var directorSettings = b.GetRequiredService<DirectorProjectSettings>();
                         directorSettings.CsProjFile = "BlingoEngine.Demo.TetriGrounds.Core\\BlingoEngine.Demo.TetriGrounds.Core.csproj";
                     })
#else
                    .WithBlingoGodotEngine(this)
#endif
                    .SetProjectFactory<BlingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildAndRunProject()
                    );
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


