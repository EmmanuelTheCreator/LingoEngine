using Godot;
using LingoEngine.FrameworkCommunication;
using System.Collections.Generic;

namespace LingoEngine.LGodot.Core;

public partial class LingoGodotDebugOverlay : CanvasLayer, ILingoFrameworkDebugOverlay
{
    private readonly List<Label> _labels = new();
    private int _index;

    public LingoGodotDebugOverlay(Node root)
    {
        Name = "DebugOverlay";
        root.AddChild(this);
    }

    public void Begin()
    {
        _index = 0;
    }

    public void RenderLine(int x, int y, string text)
    {
        Label label;
        if (_labels.Count <= _index)
        {
            label = new Label();
            AddChild(label);
            _labels.Add(label);
        }
        else
        {
            label = _labels[_index];
            label.Visible = true;
        }
        label.Position = new Vector2(x, y);
        label.Text = text;
        _index++;
    }

    public void End()
    {
        for (int i = _index; i < _labels.Count; i++)
            _labels[i].Visible = false;
    }
}
