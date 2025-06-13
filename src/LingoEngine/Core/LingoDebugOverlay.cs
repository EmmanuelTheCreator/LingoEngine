using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Core;

public class LingoDebugOverlay
{
    private readonly ILingoFrameworkDebugOverlay _framework;
    private bool _enabled;
    private float _accum;
    private int _frames;
    private float _fps;
    private LingoPlayer? _player;

    public LingoDebugOverlay(ILingoFrameworkDebugOverlay framework)
    {
        _framework = framework;
    }

    public void Toggle() => _enabled = !_enabled;

    public void Update(float deltaTime, LingoPlayer player)
    {
        if (!_enabled) return;
        _player = player;
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
        if (!_enabled || _player == null) return;
        var movie = _player.ActiveMovie as Movies.LingoMovie;
        _framework.Begin();
        _framework.RenderLine(5, 5, $"FPS: {_fps:F1}");
        _framework.RenderLine(5, 15, $"Sprites: {movie?.SpriteTotalCount ?? 0}");
        _framework.RenderLine(5, 25, $"Frame: {movie?.CurrentFrame ?? 0}");
        _framework.End();
    }
}
