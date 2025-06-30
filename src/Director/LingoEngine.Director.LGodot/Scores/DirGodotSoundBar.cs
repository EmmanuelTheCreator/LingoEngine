using Godot;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Scores;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Container for the sound channel header and grid.
/// </summary>
internal partial class DirGodotSoundBar : Control
{
    private readonly DirGodotSoundHeader _header;
    private readonly DirGodotSoundGrid _grid;
    private readonly Control _gridClipper = new();

    public DirGodotSoundBar(DirScoreGfxValues gfxValues, ILingoFrameworkFactory factory)
    {
        _header = new DirGodotSoundHeader(gfxValues);
        _grid = new DirGodotSoundGrid(gfxValues, factory);
        _gridClipper.ClipContents = true;
        _gridClipper.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddChild(_header);
        AddChild(_gridClipper);
        _gridClipper.AddChild(_grid);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
    }

    public bool Collapsed
    {
        get => _grid.Collapsed;
        set
        {
            _header.Collapsed = value;
            _grid.Collapsed = value;
            UpdateGridPosition();
        }
    }

    public float ScrollX
    {
        get => _grid.ScrollX;
        set => _grid.ScrollX = value;
    }

    public void SetMovie(LingoMovie? movie)
    {
        _header.SetMovie(movie);
        _grid.SetMovie(movie);
        UpdateGridPosition();
    }

    public override void _Ready()
    {
        base._Ready();
        UpdateGridPosition();
        Resized += UpdateGridPosition;
    }

    private void UpdateGridPosition()
    {
        _gridClipper.Position = new Vector2(_header.Size.X, 0);
        _gridClipper.Size = new Vector2(Size.X - _header.Size.X, _grid.CustomMinimumSize.Y);
        _grid.Position = Vector2.Zero;
    }
}
