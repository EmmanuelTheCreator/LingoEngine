using Godot;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using System.Collections.Generic;

namespace LingoEngine.Godot;

public partial class LingoGodotKey : Node, ILingoFrameworkKey
{
    private readonly List<Key> _pressed = new();
    private Lazy<LingoKey> _lingoKey;
    private string _lastKey = string.Empty;
    private int _lastCode;

    public LingoGodotKey(Node root, Lazy<LingoKey> key)
    {
        Name = "KeyConnector";
        _lingoKey = key;
        root.AddChild(this);
    }

    internal void SetLingoKey(LingoKey key)
    {
        _lingoKey = new Lazy<LingoKey>(() => key);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey k)
        {
            if (k.Pressed)
            {
                if (!_pressed.Contains(k.Keycode))
                    _pressed.Add(k.Keycode);
                _lastCode = (int)k.Keycode;
                _lastKey = k.KeyLabel;
                _lingoKey.Value.DoKeyDown();
            }
            else
            {
                _pressed.Remove(k.Keycode);
                _lastCode = (int)k.Keycode;
                _lastKey = k.KeyLabel;
                _lingoKey.Value.DoKeyUp();
            }
        }
    }

    public bool CommandDown => Input.IsKeyPressed((int)Key.Meta);
    public bool ControlDown => Input.IsKeyPressed((int)Key.Ctrl);
    public bool OptionDown => Input.IsKeyPressed((int)Key.Alt);
    public bool ShiftDown => Input.IsKeyPressed((int)Key.Shift);

    public bool KeyPressed(LingoKeyType key) => key switch
    {
        LingoKeyType.BACKSPACE => _pressed.Contains(Key.Backspace),
        LingoKeyType.ENTER or LingoKeyType.RETURN => _pressed.Contains(Key.Enter),
        LingoKeyType.QUOTE => _pressed.Contains(Key.Quoteleft),
        LingoKeyType.SPACE => _pressed.Contains(Key.Space),
        LingoKeyType.TAB => _pressed.Contains(Key.Tab),
        _ => false
    };

    public bool KeyPressed(char key)
        => _pressed.Contains((Key)char.ToUpperInvariant(key));

    public bool KeyPressed(int keyCode)
        => _pressed.Contains((Key)keyCode);

    public string Key => _lastKey;
    public int KeyCode => _lastCode;
}
