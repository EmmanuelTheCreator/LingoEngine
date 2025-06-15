using Godot;
using LingoEngine.Movies;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Movies;

namespace LingoEngine.Director.LGodot.Movies;

internal partial class DirGodotStageWindow : BaseGodotWindow, ILingoFrameworkStageWindow, ILingoFrameworkStage
{
    private readonly Button _rewindButton = new Button();
    private readonly Button _playButton = new Button();
    private bool _toggleKey;
    private LingoMovie? _movie;
    private ILingoFrameworkStage? _stage;

    public DirGodotStageWindow(Node root)
        : base("Stage")
    {
        Position = new Vector2(20, 60);
        Size = new Vector2(640, 480);
        CustomMinimumSize = Size;
        root.AddChild(this);

        _rewindButton.Position = new Vector2(3, 2);
        _rewindButton.CustomMinimumSize = new Vector2(20, 16);
        _rewindButton.Text = "<<";
        AddChild(_rewindButton);

        _playButton.Position = new Vector2(26, 2);
        _playButton.CustomMinimumSize = new Vector2(40, 16);
        _playButton.Text = "Play";
        AddChild(_playButton);

        _rewindButton.Pressed += () => _movie?.GoTo(1);
        _playButton.Pressed += OnPlayPressed;
    }

    private void OnPlayPressed()
    {
        if (_movie == null) return;
        if (_movie.IsPlaying)
            _movie.Halt();
        else
            _movie.Play();
        UpdatePlayButton();
    }

    private void UpdatePlayButton()
    {
        if (_movie == null)
            _playButton.Text = "Play";
        else
            _playButton.Text = _movie.IsPlaying ? "Stop" : "Play";
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
                node2D.Position = new Vector2(0, 20);
        }
    }

    public void SetActiveMovie(LingoMovie? movie)
    {
        _stage?.SetActiveMovie(movie);
        _movie = movie;
        UpdatePlayButton();
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.F5) && !_toggleKey)
            Visible = !Visible;
        _toggleKey = Input.IsKeyPressed(Key.F5);
    }
}
