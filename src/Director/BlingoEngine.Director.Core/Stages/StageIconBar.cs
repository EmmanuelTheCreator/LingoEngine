using System;
using AbstUI.Commands;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Commands;
using BlingoEngine.Core;
using AbstUI.Primitives;
using AbstUI.Components.Inputs;
using AbstUI.Components.Containers;
using AbstUI.Components.Buttons;

namespace BlingoEngine.Director.Core.Stages;

public class StageIconBar : IDisposable
{
    private readonly AbstButton _rewindButton;
    private readonly AbstButton _playButton;
    private readonly AbstButton _prevFrameButton;
    private readonly AbstButton _nextFrameButton;
    private readonly AbstInputSlider<float> _zoomSlider;
    private readonly AbstInputCombobox _zoomDropdown;
    private readonly AbstPanel _colorDisplay;
    private readonly AbstColorPicker _colorPicker;
    private readonly AbstStateButton _recordButton;
    private readonly IBlingoPlayer _player;
    private readonly IDirStageManager _stageManager;
    private BlingoMovie? _movie;
    private readonly IDirectorEventSubscription _stageChangedSub;
    private bool _updatingZoom;

    public AbstPanel Panel { get; }

    public event Action<float>? ZoomChanged;
    public event Action<AColor>? ColorChanged;

    public float MinZoom => _zoomSlider.MinValue;
    public float MaxZoom => _zoomSlider.MaxValue;

    public StageIconBar(IBlingoFrameworkFactory factory, IAbstCommandManager commandManager, IBlingoPlayer player, IDirectorEventMediator mediator, IDirStageManager stageManager)
    {
        const int iconBarHeight = 12;
        _player = player;
        _stageManager = stageManager;

        Panel = factory.CreatePanel("StageIconBar");
        Panel.Height = iconBarHeight;
        Panel.BackgroundColor = DirectorColors.BG_WhiteMenus;

        var container = factory.CreateWrapPanel(AOrientation.Horizontal, "StageIconBarContainer");
        container.Width = 600;
        Panel.AddItem(container);

        _rewindButton = factory.CreateButton("RewindButton", "|<");
        _rewindButton.Width = 20;
        _rewindButton.Height = iconBarHeight;
        _rewindButton.Pressed += () => commandManager.Handle(new MovieRewindCommand());
        container.AddItem(_rewindButton);

        _playButton = factory.CreateButton("PlayButton", "Play");
        _playButton.Width = 60;
        _playButton.Height = iconBarHeight;
        _playButton.Pressed += () => commandManager.Handle(new PlayMovieCommand());
        container.AddItem(_playButton);

        _prevFrameButton = factory.CreateButton("PrevFrameButton", "<");
        _prevFrameButton.Width = 20;
        _prevFrameButton.Height = iconBarHeight;
        _prevFrameButton.Pressed += () => commandManager.Handle(new StepFrameCommand(-1));
        container.AddItem(_prevFrameButton);

        _nextFrameButton = factory.CreateButton("NextFrameButton", ">");
        _nextFrameButton.Width = 20;
        _nextFrameButton.Height = iconBarHeight;
        _nextFrameButton.Pressed += () => commandManager.Handle(new StepFrameCommand(1));
        container.AddItem(_nextFrameButton);

        _zoomSlider = factory.CreateInputSliderFloat(AOrientation.Horizontal, "ZoomSlider", 0.5f, 1.5f, 0.1f);
        _zoomSlider.Value = 1f;
        _zoomSlider.Width = 150;
        _zoomSlider.Height = iconBarHeight;
        _zoomSlider.ValueChanged += () =>
        {
            if (_updatingZoom) return;
            ZoomChanged?.Invoke(_zoomSlider.Value);
            SetZoom(_zoomSlider.Value);
        };
        container.AddItem(_zoomSlider);

        _zoomDropdown = factory.CreateInputCombobox("ZoomDropdown", null);
        for (int i = 50; i <= 150; i += 10)
            _zoomDropdown.AddItem(i.ToString(), $"{i}%");
        _zoomDropdown.SelectedKey = "100";
        _zoomDropdown.Width = 60;
        _zoomDropdown.Height = iconBarHeight;
        _zoomDropdown.ValueChanged += () =>
        {
            if (_updatingZoom) return;
            if (_zoomDropdown.SelectedKey is string key && int.TryParse(key, out var p))
            {
                SetZoom(p / 100f);
                ZoomChanged?.Invoke(p / 100f);
            }
        };
        container.AddItem(_zoomDropdown);

        _colorDisplay = factory.CreatePanel("ColorDisplay");
        _colorDisplay.Width = iconBarHeight;
        _colorDisplay.Height = iconBarHeight;
        _colorDisplay.BackgroundColor = AColors.Black;
        container.AddItem(_colorDisplay);

        _colorPicker = factory.CreateColorPicker("ColorPicker", null);
        _colorPicker.Width = iconBarHeight;
        _colorPicker.Height = iconBarHeight;
        _colorPicker.Color = AColors.Black;
        _colorPicker.ValueChanged += () =>
        {
            var c = _colorPicker.Color;
            _colorDisplay.BackgroundColor = c;
            ColorChanged?.Invoke(c);
        };
        container.AddItem(_colorPicker);

        _recordButton = factory.CreateStateButton("RecordButton", null, "●");
        _recordButton.Width = iconBarHeight;
        _recordButton.Height = iconBarHeight;
        _recordButton.IsOn = _stageManager.RecordKeyframes;
        _recordButton.ValueChanged += () => { _stageManager.RecordKeyframes = _recordButton.IsOn; };
        container.AddItem(_recordButton);

        _stageChangedSub = mediator.Subscribe(DirectorEventType.StagePropertiesChanged, OnStagePropertyChanged);
    }

    private bool OnStagePropertyChanged()
    {
        SetColor(_player.Stage.BackgroundColor);
        return true;
    }

    private void OnPlayStateChanged(bool playing) => UpdatePlayButton();

    private void UpdatePlayButton()
    {
        _playButton.Text = _movie != null && _movie.IsPlaying ? "stop !" : "Play >";
    }

    public void SetActiveMovie(BlingoMovie? movie)
    {
        if (_movie != null)
            _movie.PlayStateChanged -= OnPlayStateChanged;
        _movie = movie;
        if (_movie != null)
            _movie.PlayStateChanged += OnPlayStateChanged;
        UpdatePlayButton();
    }

    public void SetZoom(float value)
    {
        _updatingZoom = true;
        _zoomSlider.Value = value;
        _zoomDropdown.SelectedKey = ((int)MathF.Round(value * 100)).ToString();
        _updatingZoom = false;
    }

    public void SetColor(AColor color)
    {
        _colorDisplay.BackgroundColor = color;
        _colorPicker.Color = color;
    }

    public void Dispose()
    {
        _stageChangedSub.Release();
        if (_movie != null)
            _movie.PlayStateChanged -= OnPlayStateChanged;
    }
}

