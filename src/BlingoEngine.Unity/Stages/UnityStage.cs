using System;
using System.Collections;
using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.Components;
using AbstUI.LUnity.Bitmaps;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Stages;
using BlingoEngine.Unity.Movies;
using UnityEngine;

namespace BlingoEngine.Unity.Stages;

/// <summary>
/// Unity implementation of <see cref="IBlingoFrameworkStage"/>.
/// Wraps a <see cref="GameObject"/> and drives the engine clock
/// via Unity's update loop.
/// </summary>
public class UnityStage : MonoBehaviour, IBlingoFrameworkStage, IDisposable
{
    private BlingoStage _stage = null!;
    private BlingoClock _clock = null!;
    private readonly HashSet<UnityMovie> _movies = new();
    private UnityMovie? _activeMovie;
    private float _width;
    private float _height;
    private AMargin _margin = AMargin.Zero;
    private int _zIndex;

    public BlingoStage BlingoStage => _stage;

    public float Scale
    {
        get => transform.localScale.x;
        set => transform.localScale = new Vector3(value, value, value);
    }


    public float X { get; set; }
    public float Y { get; set; }
    internal void Configure(BlingoClock clock) => _clock = clock;

    internal void Init(BlingoStage stage)
    {
        _stage = stage;
        _width = stage.Width;
        _height = stage.Height;
    }

    private void Update()
    {
        _clock?.Tick(Time.deltaTime);
        _activeMovie?.UpdateStage();
    }

    internal void ShowMovie(UnityMovie movie) => _movies.Add(movie);

    internal void HideMovie(UnityMovie movie) => _movies.Remove(movie);

    public void SetActiveMovie(BlingoMovie? blingoMovie)
    {
        _activeMovie?.Hide();
        if (blingoMovie == null)
        {
            _activeMovie = null;
            return;
        }
        var movie = blingoMovie.Framework<UnityMovie>();
        _activeMovie = movie;
        movie.Show();
    }

    public void ApplyPropertyChanges()
    {
    }

    public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
    {
        StartCoroutine(CaptureNextFrame(onCaptured));
    }

    private IEnumerator CaptureNextFrame(Action<IAbstTexture2D> onCaptured)
    {
        yield return null; // wait until the next frame renders
        onCaptured(GetScreenshot());
    }

    public IAbstTexture2D GetScreenshot()
    {
        var tex = new Texture2D((int)_stage.Width, (int)_stage.Height);
        return new UnityTexture2D(tex, $"StageShot_{_activeMovie?.CurrentFrame ?? 0}");
    }

    public void ShowTransition(IAbstTexture2D startTexture) { }

    public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect) { }

    public void HideTransition() { }

    public void Dispose()
    {
        foreach (var m in _movies)
            m.Dispose();
        _movies.Clear();
    }

    string IAbstFrameworkNode.Name
    {
        get => gameObject.name;
        set => gameObject.name = value;
    }

    bool IAbstFrameworkNode.Visibility
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    float IAbstFrameworkNode.Width
    {
        get => _width;
        set => _width = value;
    }

    float IAbstFrameworkNode.Height
    {
        get => _height;
        set => _height = value;
    }

    AMargin IAbstFrameworkNode.Margin
    {
        get => _margin;
        set => _margin = value;
    }

    int IAbstFrameworkNode.ZIndex
    {
        get => _zIndex;
        set => _zIndex = value;
    }

    object IAbstFrameworkNode.FrameworkNode => gameObject;

   
}


