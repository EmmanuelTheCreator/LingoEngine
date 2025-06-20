using Godot;
using LingoEngine.Movies;
using LingoEngine.Core;
using LingoEngine.Commands;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreLabelsBar : Control
{
    private LingoMovie? _movie;

    private readonly DirGodotScoreGfxValues _gfxValues;
    private readonly ILingoCommandManager _commandManager;
    private readonly LineEdit _editField = new();
    private string? _activeLabel;
    private int _activeFrame;
    private int _startFrame;
    private bool _dragging;

    public DirGodotScoreLabelsBar(DirGodotScoreGfxValues gfxValues, ILingoCommandManager commandManager)
    {
        _gfxValues = gfxValues;
        _commandManager = commandManager;
        AddChild(_editField);
        _editField.Visible = false;
        _editField.Size = new Vector2(60, 16);
        _editField.TextSubmitted += _ => CommitEdit();
    }

    public void SetMovie(LingoMovie? movie)
    {
        _movie = movie;
        Size = new Vector2(_gfxValues.LeftMargin + (_movie?.FrameCount ?? 0) * _gfxValues.FrameWidth, 20);
        _editField.Visible = false;
    }

    public override void _Draw()
    {
        if (_movie == null) return;
        int frameCount = _movie.FrameCount;
        var font = ThemeDB.FallbackFont;
        Size = new Vector2(_gfxValues.LeftMargin + (frameCount) * _gfxValues.FrameWidth, 20);
        DrawRect(new Rect2(0, 0, Size.X, 20), Colors.White);
        foreach (var kv in _movie.GetScoreLabels())
        {
            float x = _gfxValues.LeftMargin + (kv.Value - 1) * _gfxValues.FrameWidth;
            Vector2[] pts = { new Vector2(x, 5), new Vector2(x + 10, 5), new Vector2(x + 5, 15) };
            DrawPolygon(pts, new[] { Colors.Black });
            DrawString(font, new Vector2(x + 12, font.GetAscent() - 5), kv.Key,
                HorizontalAlignment.Left, -1, 10, Colors.Black);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (_movie == null) return;

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                Vector2 pos = GetLocalMousePosition();
                foreach (var kv in _movie.GetScoreLabels())
                {
                    var font = ThemeDB.FallbackFont;
                    float x = _gfxValues.LeftMargin + (kv.Value - 1) * _gfxValues.FrameWidth;
                    float width = font.GetStringSize(kv.Key).X + 20;
                    if (pos.X >= x && pos.X <= x + width)
                    {
                        _activeLabel = kv.Key;
                        _activeFrame = kv.Value;
                        _startFrame = kv.Value;
                        if (mb.DoubleClick)
                        {
                            // Open the edit field on double click
                            _dragging = false;
                            _editField.Text = kv.Key;
                            UpdateEditFieldPosition();
                            _editField.Visible = true;
                            _editField.GrabFocus();
                        }
                        else
                        {
                            // Start dragging to reposition the label
                            _dragging = true;
                        }
                        break;
                    }
                }
            }
            else if (_dragging)
            {
                CommitEdit();
                _dragging = false;
            }
        }
        else if (@event is InputEventMouseMotion && _dragging)
        {
            float frameF = (GetLocalMousePosition().X - _gfxValues.LeftMargin) / _gfxValues.FrameWidth;
            _activeFrame = Math.Clamp(Mathf.RoundToInt(frameF) + 1, 1, _movie.FrameCount);
            UpdateEditFieldPosition();
        }
    }

    private void UpdateEditFieldPosition()
    {
        float x = _gfxValues.LeftMargin + (_activeFrame - 1) * _gfxValues.FrameWidth + 12;
        _editField.Position = new Vector2(x, 2);
    }

    private void CommitEdit()
    {
        if (_activeLabel == null || _movie == null) return;
        if (_activeFrame != _startFrame || _editField.Text != _activeLabel)
            _commandManager.Handle(new UpdateFrameLabelCommand(_startFrame, _activeFrame, _editField.Text));
        _activeLabel = null;
        _editField.Visible = false;
        QueueRedraw();
    }
}
