using Godot;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Movies;
using LingoEngine.LGodot.Stages;
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Movies;

internal partial class DirGodotStageWindow : BaseGodotWindow
{
    private readonly LingoGodotStageContainer _stageContainer;
    private readonly IDirectorEventMediator _mediator;
    private LingoMovie? _movie;
    private ILingoFrameworkStage? _stage;

    public DirGodotStageWindow(Node root, LingoGodotStageContainer stageContainer, IDirectorEventMediator directorEventMediator)
        : base("Stage")
    {
        _stageContainer = stageContainer;
        _mediator = directorEventMediator;
        
        Size = new Vector2(640 +10, 480+ TitleBarHeight);
        CustomMinimumSize = Size;
        var scrollContainer = new ScrollContainer();
        // Set anchors to stretch fully
        scrollContainer.AnchorLeft = 0;
        scrollContainer.AnchorTop = 0;
        scrollContainer.AnchorRight = 1;
        scrollContainer.AnchorBottom = 1;

        // Set offsets to 0
        scrollContainer.OffsetLeft = 0;
        scrollContainer.OffsetTop = 0;
        scrollContainer.OffsetRight = -10;
        scrollContainer.OffsetBottom = -5;
        root.AddChild(this);

        scrollContainer.Position = new Vector2(0, 20);
        scrollContainer.AddChild(stageContainer.Container);
        AddChild(scrollContainer);
        directorEventMediator.SubscribeToMenu(MenuCodes.StageWindow, () => Visible = !Visible);
    }
    protected override void OnResizing(Vector2 size)
    {
        base.OnResizing(size);
    }

    public void SetStage(ILingoFrameworkStage stage)
    {
        _stage = stage;
        if (stage is Node node)
        {
            if (node.GetParent() != this)
            {
                node.GetParent()?.RemoveChild(node);
                AddChild(node);
            }
            if (node is Node2D node2D)
                node2D.Position = new Vector2(0, TitleBarHeight);
        }
    }

    public void SetActiveMovie(LingoMovie? movie)
    {
        _stage?.SetActiveMovie(movie);
        _movie = movie;
    }

    

   
}
