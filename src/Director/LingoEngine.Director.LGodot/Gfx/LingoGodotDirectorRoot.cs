using Godot;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Director.Core.Events;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.LGodot.Stages;
using LingoEngine.LGodot;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Inspector;
using LingoEngine.Director.LGodot.Movies;
using System.Reflection.Emit;

namespace LingoEngine.Director.LGodot.Gfx
{
    public class LingoGodotDirectorRoot : IDisposable
    {
        private readonly ILingoMovie _lingoMovie;
        private readonly Control _directorParent = new();
        private readonly DirGodotCastWindow _castViewer;
        private readonly DirGodotScoreWindow _scoreWindow;
        private readonly DirGodotObjectInspector _inspector;
        private readonly IDirectorEventMediator _mediator;
        private readonly DirGodotStageWindow _stageWindow;
        private readonly DirGodotMainMenu _dirGodotMainMenu;


        public LingoGodotDirectorRoot(ILingoMovie lingoMovie, IServiceProvider serviceProvider)
        {
            _mediator = serviceProvider.GetRequiredService<IDirectorEventMediator>();
            _lingoMovie = lingoMovie;

            // set up root
            var parent = (Node2D)serviceProvider.GetRequiredService<LingoGodotRootNode>().RootNode;
            parent.AddChild(_directorParent);

            // Setup stage
            var stageContainer = (LingoGodotStageContainer)serviceProvider.GetRequiredService<ILingoFrameworkStageContainer>();
            _stageWindow = new DirGodotStageWindow(_directorParent, stageContainer,_mediator);


            _dirGodotMainMenu = new DirGodotMainMenu(_mediator, lingoMovie);
            _castViewer = new DirGodotCastWindow(_mediator, lingoMovie);
            _scoreWindow = new DirGodotScoreWindow(_mediator);
            _inspector = new DirGodotObjectInspector(_mediator);
            _scoreWindow.SetMovie((LingoMovie)lingoMovie);
            _directorParent.AddChild(_dirGodotMainMenu);
            _directorParent.AddChild(_castViewer);
            _directorParent.AddChild(_scoreWindow);


            //var hContainer = new HBoxContainer
            //{
            //    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            //};
            ////var spacer = new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            ////hContainer.AddChild(spacer);
            ////hContainer.AddChild(_inspector);
            ////_directorParent.AddChild(hContainer);
            ////_inspector.AnchorLeft = 1.0f;
            //_inspector.AnchorRight = 1.0f;
            //_inspector.OffsetLeft = -150; // Adjust to control width
            //_inspector.OffsetRight = 0;
            _directorParent.AddChild(_inspector);
            _stageWindow.Position = new Vector2(40, 25); 
            _castViewer.Position = new Vector2(830, 25);
            _scoreWindow.Position = new Vector2(20, 540);
            _inspector.Position = new Vector2(1330, 25);

        }



        public void Dispose()
        {
            _stageWindow.Dispose();
            _scoreWindow.Dispose();
            _castViewer.Dispose();
            _inspector.Dispose();
        }
    }
}
