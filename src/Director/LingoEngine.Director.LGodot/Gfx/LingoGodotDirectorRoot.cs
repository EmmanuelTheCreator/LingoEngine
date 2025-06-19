using Godot;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Director.Core.Events;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.LGodot.Stages;
using LingoEngine.Core;
using LingoEngine.LGodot;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Inspector;
using LingoEngine.Director.LGodot.Movies;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.Core;

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
        private readonly LingoPlayer _player;
        private readonly DirGodotProjectSettingsWindow _projectSettingsWindow;
        private readonly DirectorProjectManager _projectManager;
        private readonly DirGodotBinaryViewerWindow _binaryViewer;


        public LingoGodotDirectorRoot(ILingoMovie lingoMovie, LingoPlayer player, IServiceProvider serviceProvider)
        {
            _mediator = serviceProvider.GetRequiredService<IDirectorEventMediator>();
            _lingoMovie = lingoMovie;
            _player = player;
            _projectSettingsWindow = serviceProvider.GetRequiredService<DirGodotProjectSettingsWindow>();
            _projectManager = serviceProvider.GetRequiredService<DirectorProjectManager>();
           

            // set up root
            var parent = (Node2D)serviceProvider.GetRequiredService<LingoGodotRootNode>().RootNode;
            parent.AddChild(_directorParent);

            // Apply Director UI theme from IoC
            var style = serviceProvider.GetRequiredService<DirectorStyle>();
            _directorParent.Theme = style.Theme;

            // Setup stage
            var stageContainer = (LingoGodotStageContainer)serviceProvider.GetRequiredService<ILingoFrameworkStageContainer>();
            var commandManager = serviceProvider.GetRequiredService<ILingoCommandManager>();
            _stageWindow = new DirGodotStageWindow(_directorParent, stageContainer,_mediator, commandManager, player);

            // project settings
            _projectSettingsWindow.Visible = false;
            _projectSettingsWindow.Position = new Vector2(100, 100);

            _dirGodotMainMenu = new DirGodotMainMenu(_mediator, lingoMovie);
            _castViewer = new DirGodotCastWindow(_mediator, lingoMovie, style);
            _scoreWindow = new DirGodotScoreWindow(_mediator);
            _inspector = new DirGodotObjectInspector(_mediator);
            _binaryViewer = new DirGodotBinaryViewerWindow(_mediator);
            _scoreWindow.SetMovie((LingoMovie)lingoMovie);
            _directorParent.AddChild(_dirGodotMainMenu);
            _directorParent.AddChild(_projectSettingsWindow);
            _directorParent.AddChild(_castViewer);
            _directorParent.AddChild(_scoreWindow);
            _directorParent.AddChild(_binaryViewer);


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
            _scoreWindow.Position = new Vector2(20, 560);
            _inspector.Position = new Vector2(1330, 25);
            _binaryViewer.Position = new Vector2(20, 120);

        }



        public void Dispose()
        {
            _stageWindow.Dispose();
            _scoreWindow.Dispose();
            _castViewer.Dispose();
            _inspector.Dispose();
            _binaryViewer.Dispose();
        }
    }
}
