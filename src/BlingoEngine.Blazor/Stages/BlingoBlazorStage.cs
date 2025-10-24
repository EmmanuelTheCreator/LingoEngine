using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AbstUI.Bitmaps;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.Components.Containers;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Stages;
using BlingoEngine.Blazor.Movies;
using Microsoft.JSInterop;
using AbstUI.Blazor;
using AbstUI.Blazor.Bitmaps;
using AbstUI.Blazor.Inputs;

namespace BlingoEngine.Blazor.Stages;

/// <summary>
/// Blazor stage implementation that drives the engine clock, renders
/// the active movie to an off-screen canvas and supports transition
/// overlays.
/// </summary>
public class BlingoBlazorStage : IBlingoFrameworkStage, IDisposable
{
    private readonly BlingoClock _clock;
    private readonly IJSRuntime _js;
    private readonly AbstUIScriptResolver _scripts;
    private readonly BlingoBlazorRootPanel _root;
    private readonly BlingoDebugOverlay _overlay;
    private readonly BlingoPlayer _player;
    private bool _f1Down;
    private static readonly int F1Code = BlazorKeyCodeMap.ToBlingo("F1");
    private readonly HashSet<BlingoBlazorMovie> _movies = new();
    private BlingoBlazorMovie? _activeMovie;

    public event Action? Changed;
    private BlingoStage _stage = null!;

    private readonly CancellationTokenSource _loopCts = new();
    private readonly Task _loopTask;
    private Action<IAbstTexture2D>? _nextShot;
    private bool _isTransitioning;
    private AbstBlazorTexture2D? _transitionStart;

    private string _name = "BlazorStage";
    private bool _visibility = true;
    private float _width;
    private float _height;
    private AMargin _margin = AMargin.Zero;
    private int _zIndex;

    public BlingoStage BlingoStage => _stage;

    public float Scale { get; set; } = 1f;


    public float X { get; set; }
    public float Y { get; set; }

    public BlingoBlazorStage(BlingoPlayer player, IJSRuntime js, AbstUIScriptResolver scripts, BlingoBlazorRootPanel root, IAbstComponentFactory factory)
    {
        _player = player;
        _clock = (BlingoClock)player.Clock;
        _js = js;
        _scripts = scripts;
        _root = root;
        _overlay = new BlingoDebugOverlay(new BlingoBlazorDebugOverlay(root, factory), player);
        _root.Component.Changed += OnRootComponentChanged;
        _loopTask = Task.Run(RenderLoopAsync);
    }

    internal void Init(BlingoStage stage)
    {
        _stage = stage;
        _width = stage.Width;
        _height = stage.Height;
        _root.Component.Name = _name;
        _root.Component.Visibility = _visibility;
        _root.Component.Margin = _margin;
        _root.Component.ZIndex = _zIndex;
        _root.Component.Width = _width;
        _root.Component.Height = _height;
    }

    internal void ShowMovie(BlingoBlazorMovie movie)
    {
        if (_movies.Add(movie))
        {
            _root.Component.AddItem(movie.Panel);
            movie.UpdateDimensions(_width, _height);
        }
    }

    internal void HideMovie(BlingoBlazorMovie movie)
    {
        if (_movies.Remove(movie))
            _root.Component.RemoveItem(movie.Panel);
    }

    /// <inheritdoc />
    public void SetActiveMovie(BlingoMovie? blingoMovie)
    {
        _activeMovie?.Hide();
        if (blingoMovie == null)
        {
            _activeMovie = null;
            Changed?.Invoke();
            return;
        }
        var movie = blingoMovie.Framework<BlingoBlazorMovie>();
        _activeMovie = movie;
        movie.Show();
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public void ApplyPropertyChanges() { }

    public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
    {
        _nextShot = onCaptured;
    }

    public IAbstTexture2D GetScreenshot()
    {
        return new NullTexture((int)_stage.Width, (int)_stage.Height, $"StageShot_{_activeMovie?.CurrentFrame ?? 0}");
    }

    public void ShowTransition(IAbstTexture2D startTexture)
    {
        _transitionStart?.Dispose();
        _transitionStart = (AbstBlazorTexture2D)startTexture.Clone();
        _isTransitioning = true;
        // Transition drawing not implemented in DOM composition yet.
    }

    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
    {
        if (!_isTransitioning)
            return;
    }

    public void HideTransition()
    {
        _isTransitioning = false;
        _transitionStart?.Dispose();
        _transitionStart = null;
        _activeMovie?.UpdateStage();
        Changed?.Invoke();
    }

    public void Dispose()
    {
        _loopCts.Cancel();
        try { _loopTask.Wait(); } catch { }
        foreach (var m in _movies.ToArray())
            m.Dispose();
        _movies.Clear();
        _transitionStart?.Dispose();
        _root.Component.Changed -= OnRootComponentChanged;
    }

    private void OnRootComponentChanged() => Changed?.Invoke();

    private async Task RenderLoopAsync()
    {
        var sw = Stopwatch.StartNew();
        var last = sw.Elapsed;
        while (!_loopCts.IsCancellationRequested)
        {
            var now = sw.Elapsed;
            var delta = (float)(now - last).TotalSeconds;
            last = now;
            _clock.Tick(delta);
            _overlay.Update(delta);
            bool f1 = _player.Key.KeyPressed(F1Code);
            if (f1 && !_f1Down)
                _overlay.Toggle();
            _f1Down = f1;
            _overlay.Render();
            if (!_isTransitioning)
                _activeMovie?.UpdateStage();
            if (_nextShot != null)
            {
                var shot = GetScreenshot();
                var cb = _nextShot;
                _nextShot = null;
                cb(shot);
            }
            try
            {
                await Task.Delay(16, _loopCts.Token);
            }
            catch (TaskCanceledException) { }
        }
    }

    private sealed class NullTexture : AbstBaseTexture2D<object>
    {
        private byte[] _pixels;

        public NullTexture(int width, int height, string name) : base(name)
        {
            Width = width;
            Height = height;
            _pixels = new byte[width * height * 4];
        }

        public override int Width { get; }
        public override int Height { get; }

        protected override void DisposeTexture() { }

        public override byte[] GetPixels() => _pixels;
        public override void SetARGBPixels(byte[] argbPixels) => _pixels = argbPixels;
        public override void SetRGBAPixels(byte[] rgbaPixels) => _pixels = rgbaPixels;

        public override IAbstTexture2D Clone()
        {
            var clone = new NullTexture(Width, Height, Name);
            clone._pixels = (byte[])_pixels.Clone();
            return clone;
        }
    }

    internal BlingoBlazorMovie? ActiveMovie => _activeMovie;

    string IAbstFrameworkNode.Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;
            _name = value;
            _root.Component.Name = value;
            Changed?.Invoke();
        }
    }

    bool IAbstFrameworkNode.Visibility
    {
        get => _visibility;
        set
        {
            if (_visibility == value)
                return;
            _visibility = value;
            _root.Component.Visibility = value;
            Changed?.Invoke();
        }
    }

    float IAbstFrameworkNode.Width
    {
        get => _width;
        set
        {
            if (Math.Abs(_width - value) <= float.Epsilon)
                return;
            _width = value;
            _root.Component.Width = value;
            foreach (var movie in _movies)
                movie.UpdateDimensions(_width, _height);
            Changed?.Invoke();
        }
    }

    float IAbstFrameworkNode.Height
    {
        get => _height;
        set
        {
            if (Math.Abs(_height - value) <= float.Epsilon)
                return;
            _height = value;
            _root.Component.Height = value;
            foreach (var movie in _movies)
                movie.UpdateDimensions(_width, _height);
            Changed?.Invoke();
        }
    }

    AMargin IAbstFrameworkNode.Margin
    {
        get => _margin;
        set
        {
            if (_margin.Equals(value))
                return;
            _margin = value;
            _root.Component.Margin = value;
            Changed?.Invoke();
        }
    }

    int IAbstFrameworkNode.ZIndex
    {
        get => _zIndex;
        set
        {
            if (_zIndex == value)
                return;
            _zIndex = value;
            _root.Component.ZIndex = value;
            Changed?.Invoke();
        }
    }

    object IAbstFrameworkNode.FrameworkNode => _root.Component;

}

