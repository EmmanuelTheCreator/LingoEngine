using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Core;

public class LingoDebugOverlay
{
    private readonly ILingoFrameworkDebugOverlay _framework;
    private bool _enabled;
    private float _accum;
    private int _frames;
    private float _fps;
    private LingoPlayer _player;

    public LingoDebugOverlay(ILingoFrameworkDebugOverlay framework, LingoPlayer player)
    {
        _framework = framework;
        _player = player;
        _framework.PrepareLine(1, $"FPS: {_fps:F1}");
        _framework.PrepareLine(2, $"Sprites:0");
        _framework.PrepareLine(3, $"Frame: 0");
    }

    public void Toggle()
    {
        _enabled = !_enabled;
        if (_enabled) 
            _framework.ShowDebugger();
        else 
            _framework.HideDebugger();
    }

    public void Update(float deltaTime)
    {
        if (!_enabled) return;
        
        _accum += deltaTime;
        _frames++;
        if (_accum >= 1f)
        {
            _fps = _frames / _accum;
            _accum = 0f;
            _frames = 0;
        }
    }

    public void Render()
    {
        if (!_enabled) return;
        var movie = _player.ActiveMovie as Movies.LingoMovie;
        _framework.Begin();
        _framework.SetLineText(1, $"FPS: {_fps:F1}");
        _framework.SetLineText(2, $"Sprites: {movie?.SpriteTotalCount ?? 0}");
        _framework.SetLineText(3, $"Frame: {movie?.CurrentFrame ?? 0}");
        _framework.End();
    }

    public void Prepare()
    {
        throw new NotImplementedException();
    }
}
