using Godot;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Scores;

/// <summary>
/// Container for the sound channel header and grid.
/// </summary>
internal partial class DirGodotSoundBar : Control
{
    private readonly DirGodotSoundHeader _header;
    private readonly DirGodotSoundGrid _grid;

    public DirGodotSoundBar(DirGodotScoreGfxValues gfxValues)
    {
        _header = new DirGodotSoundHeader(gfxValues);
        _grid = new DirGodotSoundGrid(gfxValues) { Position = new Vector2(gfxValues.ChannelInfoWidth, 0) };
        AddChild(_header);
        AddChild(_grid);
    }

    public bool Collapsed
    {
        get => _grid.Collapsed;
        set
        {
            _header.Collapsed = value;
            _grid.Collapsed = value;
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
    }

    protected override void OnResized()
    {
        base.OnResized();
        _grid.Position = new Vector2(_header.Size.X, 0);
    }
}
