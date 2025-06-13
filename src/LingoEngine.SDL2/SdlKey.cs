using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.SDL2.SDLL;
using System.Collections.Generic;

namespace LingoEngine.SDL2;

internal class SdlKey : ILingoFrameworkKey
{
    private readonly HashSet<SDL.SDL_Keycode> _keys = new();
    private LingoKey? _lingoKey;
    private string _lastKey = string.Empty;
    private int _lastCode;

    internal void SetLingoKey(LingoKey key) => _lingoKey = key;

    public void ProcessEvent(SDL.SDL_Event e)
    {
        if (e.type == SDL.SDL_EventType.SDL_KEYDOWN && e.key.repeat == 0)
        {
            _keys.Add(e.key.keysym.sym);
            _lastCode = (int)e.key.keysym.sym;
            _lastKey = SDL.SDL_GetKeyName(e.key.keysym.sym);
            _lingoKey?.DoKeyDown();
        }
        else if (e.type == SDL.SDL_EventType.SDL_KEYUP)
        {
            _keys.Remove(e.key.keysym.sym);
            _lastCode = (int)e.key.keysym.sym;
            _lastKey = SDL.SDL_GetKeyName(e.key.keysym.sym);
            _lingoKey?.DoKeyUp();
        }
    }

    public bool CommandDown => (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_GUI) != 0;
    public bool ControlDown => (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_CTRL) != 0;
    public bool OptionDown => (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_ALT) != 0;
    public bool ShiftDown => (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_SHIFT) != 0;

    public bool KeyPressed(LingoKeyType key) => key switch
    {
        LingoKeyType.BACKSPACE => _keys.Contains(SDL.SDL_Keycode.SDLK_BACKSPACE),
        LingoKeyType.ENTER or LingoKeyType.RETURN => _keys.Contains(SDL.SDL_Keycode.SDLK_RETURN),
        LingoKeyType.QUOTE => _keys.Contains(SDL.SDL_Keycode.SDLK_QUOTE),
        LingoKeyType.SPACE => _keys.Contains(SDL.SDL_Keycode.SDLK_SPACE),
        LingoKeyType.TAB => _keys.Contains(SDL.SDL_Keycode.SDLK_TAB),
        _ => false
    };

    public bool KeyPressed(char key) => _keys.Contains((SDL.SDL_Keycode)char.ToUpperInvariant(key));

    public bool KeyPressed(int keyCode) => _keys.Contains((SDL.SDL_Keycode)keyCode);

    public string Key => _lastKey;
    public int KeyCode => _lastCode;
}
