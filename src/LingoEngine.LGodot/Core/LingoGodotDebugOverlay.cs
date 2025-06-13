using Godot;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.LGodot.Core;

public partial class LingoGodotDebugOverlay : CanvasLayer, ILingoFrameworkDebugOverlay
{
    private readonly List<Label> _labels = new();
    private readonly Node _root;

    public void ShowDebugger() => _root.AddChild(this);
    public void HideDebugger() => _root.RemoveChild(this);

    public LingoGodotDebugOverlay(Node root)
    {
        Name = "DebugOverlay";
        _root = root;
    }

    public void Begin()
    {
        //_index = 0;
    }
    public int PrepareLine(int id, string text)
    {
        Label label = new Label();
        AddChild(label);
        _labels.Add(label);
        label.Position = new Vector2(10, id * 15);
        label.Text = text;
        return id;
    }
    public void SetLineText(int id, string text)
    {
        _labels[id-1].Text = text;
    }
 

    public void End()
    {
        //for (int i = _index; i < _labels.Count; i++)
        //    _labels[i].Visible = false;
    }

   

   
}
