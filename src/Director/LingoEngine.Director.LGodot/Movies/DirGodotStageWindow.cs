using Godot;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Movies;

namespace LingoEngine.Director.LGodot.Movies;

internal partial class DirGodotStageWindow : BaseGodotWindow, ILingoFrameworkStageWindow, ILingoFrameworkStage
{
    private bool _toggleKey;
    private LingoMovie? _movie;
    private ILingoFrameworkStage? _stage;

    public DirGodotStageWindow(Node root)
        : base("Stage")
    {
        Position = new Vector2(20, 20);
        Size = new Vector2(640, 480);
        CustomMinimumSize = Size;
        root.AddChild(this);
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

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.F5) && !_toggleKey)
            Visible = !Visible;
        _toggleKey = Input.IsKeyPressed(Key.F5);
    }
}
