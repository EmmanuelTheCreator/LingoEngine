using Godot;
using LingoEngine.Director.LGodot.Scores;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Core;
using LingoEngine.LGodot;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Inspector;
using LingoEngine.Director.LGodot.Movies;
using LingoEngine.Director.LGodot.Pictures;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot.Styles;

namespace LingoEngine.Director.LGodot.UI
{
    public class LingoGodotDirectorRoot : IDisposable
    {
        private readonly Control _directorParent = new();
        private readonly DirGodotCastWindow _castViewer;
        private readonly DirGodotScoreWindow _scoreWindow;
        private readonly DirGodotPropertyInspector _propertyInspector;
        private readonly DirGodotToolsWindow _toolsWindow;
        private readonly DirGodotStageWindow _stageWindow;
        private readonly DirGodotMainMenu _dirGodotMainMenu;
        private readonly DirGodotProjectSettingsWindow _projectSettingsWindow;
        private readonly DirGodotBinaryViewerWindow _binaryViewer;
        private readonly DirGodotImportExportWindow _importExportWindow;
        private readonly DirGodotTextableMemberWindow _textWindow;
        private readonly DirGodotPictureMemberEditorWindow _picture;

        public LingoGodotDirectorRoot(LingoPlayer player, IServiceProvider serviceProvider)
        {
            _projectSettingsWindow = serviceProvider.GetRequiredService<DirGodotProjectSettingsWindow>();
            _directorParent.Name = "DirectorRoot";
            // set up root
            var parent = (Node2D)serviceProvider.GetRequiredService<LingoGodotRootNode>().RootNode;
            parent.AddChild(_directorParent);

            // Apply Director UI theme from IoC
            var style = serviceProvider.GetRequiredService<DirectorGodotStyle>();
            _directorParent.Theme = style.Theme;

            // Create windows
            _dirGodotMainMenu = serviceProvider.GetRequiredService<DirGodotMainMenu>();
            _stageWindow = serviceProvider.GetRequiredService<DirGodotStageWindow>();
            _castViewer = serviceProvider.GetRequiredService<DirGodotCastWindow>();
            _scoreWindow = serviceProvider.GetRequiredService<DirGodotScoreWindow>();
            _propertyInspector = serviceProvider.GetRequiredService<DirGodotPropertyInspector>();
            _toolsWindow = serviceProvider.GetRequiredService<DirGodotToolsWindow>();
            _binaryViewer = serviceProvider.GetRequiredService<DirGodotBinaryViewerWindow>();
            _importExportWindow = serviceProvider.GetRequiredService<DirGodotImportExportWindow>();
            _textWindow = serviceProvider.GetRequiredService<DirGodotTextableMemberWindow>();
            _picture = serviceProvider.GetRequiredService<DirGodotPictureMemberEditorWindow>();

            _directorParent.AddChild(_dirGodotMainMenu);
            _directorParent.AddChild(_stageWindow);
            _directorParent.AddChild(_castViewer);
            _directorParent.AddChild(_projectSettingsWindow);
            _directorParent.AddChild(_scoreWindow);
            _directorParent.AddChild(_picture);
            _directorParent.AddChild(_toolsWindow);
            _directorParent.AddChild(_importExportWindow);
            _directorParent.AddChild(_textWindow);
            _directorParent.AddChild(_propertyInspector);
            _directorParent.AddChild(_binaryViewer);

            // Set all positions
            SetDefaultPositions();

            // close some windows
            _projectSettingsWindow.CloseWindow();
            _binaryViewer.CloseWindow();
            _importExportWindow.CloseWindow();
            _textWindow.CloseWindow();
            _picture.CloseWindow();

        }

        private void SetDefaultPositions()
        {
            _stageWindow.Position = new Vector2(100, 25);
            _castViewer.Position = new Vector2(830, 25);
            _scoreWindow.Position = new Vector2(20, 560);
            _propertyInspector.Position = new Vector2(1330, 25);
            _toolsWindow.Position = new Vector2(10, 25);
            _binaryViewer.Position = new Vector2(20, 120);
            _importExportWindow.Position = new Vector2(120, 120);
            _projectSettingsWindow.Position = new Vector2(100, 100);
            _textWindow.Position = new Vector2(20, 120);
            _picture.Position = new Vector2(20, 120);
        }

        public void Dispose()
        {
            _picture.Dispose();
            _textWindow.Dispose();
            _stageWindow.Dispose();
            _scoreWindow.Dispose();
            _castViewer.Dispose();
            _propertyInspector.Dispose();
            _toolsWindow.Dispose();
            _binaryViewer.Dispose();
            _importExportWindow.Dispose();
        }
    }
}
