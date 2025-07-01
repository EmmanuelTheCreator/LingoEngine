using Godot;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Core;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Sprites;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.UI;

namespace LingoEngine.Director.LGodot.Inspector;

public partial class DirGodotPropertyInspector : BaseGodotWindow, IDirFrameworkPropertyInspectorWindow
{
   
    private readonly DirectorPropertyInspectorWindow _inspectorWindow;
    private LingoGodotPanel _headerPanel;
    private LingoGfxWindow? _behaviorWindow;

    public DirGodotPropertyInspector(DirectorPropertyInspectorWindow inspectorWindow, ILingoPlayer player, IDirGodotWindowManager windowManager, IDirectorIconManager iconManager)
        : base(DirectorMenuCodes.PropertyInspector, "Property Inspector", windowManager)
    {
        _inspectorWindow = inspectorWindow;
        
        Size = new Vector2(260, 450);
        _inspectorWindow.Init(this, Size.X, Size.Y,TitleBarHeight);
        CustomMinimumSize = Size;

        _headerPanel = _inspectorWindow.HeaderPanel.Framework<LingoGodotPanel>();
        _headerPanel.Position = new Vector2(0, TitleBarHeight);
        _headerPanel.Width = Size.X - 10;
        AddChild(_headerPanel);


        var tabs = _inspectorWindow.Tabs.Framework<LingoGodotTabContainer>();
        tabs.Position = new Vector2(0, TitleBarHeight + DirectorPropertyInspectorWindow.HeaderHeight);
        tabs.Size = new Vector2(Size.X, Size.Y - 30 - DirectorPropertyInspectorWindow.HeaderHeight);
        AddChild(tabs);

        _inspectorWindow.BehaviorSelected += OnBehaviorSelected;

        //var behaviorPanel = _inspectorWindow.BehaviorPanel.Framework<LingoGodotPanel>();
        //behaviorPanel.Visibility = false;
        //behaviorPanel.Position = new Vector2(0, TitleBarHeight + DirectorPropertyInspectorWindow.HeaderHeight);
        //behaviorPanel.Size = new Vector2(Size.X - 10, 0);
        //AddChild(behaviorPanel);

    }
  
    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);

       _inspectorWindow.OnResizing(size.X, size.Y);

    }

    private void OnBehaviorSelected(LingoSpriteBehavior behavior)
    {
        if (_behaviorWindow != null && _behaviorWindow.Framework<ILingoFrameworkGfxWindow>().FrameworkNode is Node oldNode)
        {
            oldNode.QueueFree();
            _behaviorWindow = null;
        }

        var win = _inspectorWindow.BuildBehaviorPopup(behavior);
        if (win == null) return;
        if (win.Framework<ILingoFrameworkGfxWindow>().FrameworkNode is Node node)
            GetTree().Root.AddChild(node);
        
        win.PopupCentered();
        _behaviorWindow = win;
    }
 

    
}
