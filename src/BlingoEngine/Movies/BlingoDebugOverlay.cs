using BlingoEngine.Core;
using BlingoEngine.FrameworkCommunication;

namespace BlingoEngine.Movies;

public class BlingoDebugOverlay : IBlingoClockListener
{
    private readonly IBlingoFrameworkDebugOverlay _framework;
    private bool _enabled = false;
    private float _accum;
    private int _frames;
    private float _fps;
    private float _engineAccum;
    private int _engineFrames;
    private float _engineFps;
    private readonly BlingoPlayer _player;
    private readonly BlingoClock _clock;

    public BlingoDebugOverlay(IBlingoFrameworkDebugOverlay framework, BlingoPlayer player)
    {
        _framework = framework;
        _player = player;
        _clock = (BlingoClock)player.Clock;
        _clock.Subscribe(this);
        _framework.PrepareLine(1, $"UI FPS: {_fps:F1}");
        _framework.PrepareLine(2, $"Engine FPS: {_engineFps:F1}");
        _framework.PrepareLine(3, $"Sprites:0");
        _framework.PrepareLine(4, $"Frame: 0");
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
        _engineAccum += deltaTime;
        if (_accum >= 1f)
        {
            _fps = _frames / _accum;
            _accum = 0f;
            _frames = 0;
        }
        if (_engineAccum >= 1f)
        {
            _engineFps = _engineFrames / _engineAccum;
            _engineAccum = 0f;
            _engineFrames = 0;
        }
    }

    public void Render()
    {
        if (!_enabled) return;
        var movie = _player.ActiveMovie as BlingoMovie;
        _framework.Begin();
        _framework.SetLineText(1, $"UI FPS: {_fps:F1}");
        _framework.SetLineText(2, $"Engine FPS: {_engineFps:F1}");
        _framework.SetLineText(3, $"Sprites: {movie?.SpriteTotalCount ?? 0}");
        _framework.SetLineText(4, $"Frame: {movie?.CurrentFrame ?? 0}");
        _framework.End();
    }

    public void OnTick()
    {
        _engineFrames++;
    }

    public void OnIdle(float deltaTime)
    {
        // Idle callbacks do not affect the debug overlay.
    }

    public void Prepare()
    {
        throw new NotImplementedException();
    }
}

