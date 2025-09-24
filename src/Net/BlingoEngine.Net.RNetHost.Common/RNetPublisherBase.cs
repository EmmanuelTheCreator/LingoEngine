using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using AbstUI;
using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using BlingoEngine.Texts;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetHost.Common;

/// <summary>
/// Base implementation for <see cref="IRNetPublisher"/> implementations that coordinate
/// subscriptions against the running <see cref="IBlingoPlayer"/> and surface DTO updates.
/// </summary>
public abstract class RNetPublisherBase : IRNetPublisherEngineBridge
{
    private readonly ConcurrentDictionary<(int SpriteNum, int BeginFrame), SpriteDeltaDto> _spriteQueue = new();
    private readonly ConcurrentDictionary<(int CastLibNum, int MemberNum, string Prop), RNetMemberPropertyDto> _memberQueue = new();
    private readonly ConcurrentDictionary<string, RNetMoviePropertyDto> _movieQueue = new();
    private readonly ConcurrentDictionary<string, RNetStagePropertyDto> _stageQueue = new();
    private readonly Dictionary<IHasPropertyChanged, PropertyChangedEventHandler> _propSubs = new();
    private readonly List<Action> _managerUnsubs = new();

    private IBlingoPlayer? _player;
    private IBlingoMovie? _movie;

    /// <inheritdoc />
    public void TryPublishFrame(StageFrameDto frame)
        => TryPublishFrameCore(frame);

    /// <inheritdoc />
    public void TryPublishDelta(SpriteDeltaDto delta)
        => _spriteQueue[(delta.SpriteNum, delta.BeginFrame)] = delta;

    /// <inheritdoc />
    public void TryPublishKeyframe(KeyframeDto keyframe)
        => TryPublishKeyframeCore(keyframe);

    /// <inheritdoc />
    public void TryPublishFilmLoop(FilmLoopDto filmLoop)
        => TryPublishFilmLoopCore(filmLoop);

    /// <inheritdoc />
    public void TryPublishSound(SoundEventDto sound)
        => TryPublishSoundCore(sound);

    /// <inheritdoc />
    public void TryPublishTempo(TempoDto tempo)
        => TryPublishTempoCore(tempo);

    /// <inheritdoc />
    public void TryPublishColorPalette(ColorPaletteDto palette)
        => TryPublishColorPaletteCore(palette);

    /// <inheritdoc />
    public void TryPublishFrameScript(FrameScriptDto script)
        => TryPublishFrameScriptCore(script);

    /// <inheritdoc />
    public void TryPublishTransition(TransitionDto transition)
        => TryPublishTransitionCore(transition);

    /// <inheritdoc />
    public void TryPublishMemberProperty(RNetMemberPropertyDto property)
        => _memberQueue[(property.CastLibNum, property.MemberNum, property.Prop)] = property;

    /// <inheritdoc />
    public void TryPublishMovieProperty(RNetMoviePropertyDto property)
        => _movieQueue[property.Prop] = property;

    /// <inheritdoc />
    public void TryPublishStageProperty(RNetStagePropertyDto property)
        => _stageQueue[property.Prop] = property;

    /// <inheritdoc />
    public void TryPublishTextStyle(TextStyleDto style)
        => TryPublishTextStyleCore(style);

    /// <inheritdoc />
    public void TryPublishSpriteCollectionEvent(RNetSpriteCollectionEventDto evt)
        => TryPublishSpriteCollectionEventCore(evt);

    /// <inheritdoc />
    public void FlushQueuedProperties()
    {
        foreach (var kv in _spriteQueue)
        {
            if (TryPublishDeltaCore(kv.Value))
            {
                _spriteQueue.TryRemove(kv.Key, out _);
            }
        }

        foreach (var kv in _memberQueue)
        {
            if (TryPublishMemberPropertyCore(kv.Value))
            {
                _memberQueue.TryRemove(kv.Key, out _);
            }
        }

        foreach (var kv in _movieQueue)
        {
            if (TryPublishMoviePropertyCore(kv.Value))
            {
                _movieQueue.TryRemove(kv.Key, out _);
            }
        }

        foreach (var kv in _stageQueue)
        {
            if (TryPublishStagePropertyCore(kv.Value))
            {
                _stageQueue.TryRemove(kv.Key, out _);
            }
        }
    }

    /// <inheritdoc />
    public void Enable(IBlingoPlayer player)
    {
        if (_player is not null)
        {
            Disable();
        }

        _player = player;

        if (player.Stage is IHasPropertyChanged stage)
        {
            Subscribe(stage, OnStagePropertyChanged);
        }

        player.ActiveMovieChanged += OnActiveMovieChanged;

        foreach (var cast in player.CastLibs.GetAll())
        {
            cast.MemberAdded += OnMemberAdded;
            cast.MemberDeleted += OnMemberDeleted;
            foreach (var member in cast.GetAll())
            {
                SubscribeMember(member);
            }
        }

        if (player.ActiveMovie is IBlingoMovie movie)
        {
            HookMovie(movie);
        }
    }

    /// <inheritdoc />
    public virtual void Disable()
    {
        if (_player is null)
        {
            return;
        }

        foreach (var kv in _propSubs.ToArray())
        {
            kv.Key.PropertyChanged -= kv.Value;
            _propSubs.Remove(kv.Key);
        }

        _player.ActiveMovieChanged -= OnActiveMovieChanged;

        foreach (var cast in _player.CastLibs.GetAll())
        {
            cast.MemberAdded -= OnMemberAdded;
            cast.MemberDeleted -= OnMemberDeleted;
        }

        UnhookMovie();

        _player = null;
        _movie = null;
    }

    /// <inheritdoc />
    public abstract bool TryDrainCommands(Action<IRNetCommand> apply);

    /// <summary>Publishes a frame to the underlying transport.</summary>
    protected abstract bool TryPublishFrameCore(StageFrameDto frame);

    /// <summary>Publishes a sprite delta to the underlying transport.</summary>
    protected abstract bool TryPublishDeltaCore(SpriteDeltaDto delta);

    /// <summary>Publishes a keyframe to the underlying transport.</summary>
    protected abstract bool TryPublishKeyframeCore(KeyframeDto keyframe);

    /// <summary>Publishes a film loop to the underlying transport.</summary>
    protected abstract bool TryPublishFilmLoopCore(FilmLoopDto filmLoop);

    /// <summary>Publishes a sound event to the underlying transport.</summary>
    protected abstract bool TryPublishSoundCore(SoundEventDto sound);

    /// <summary>Publishes a tempo change to the underlying transport.</summary>
    protected abstract bool TryPublishTempoCore(TempoDto tempo);

    /// <summary>Publishes a color palette to the underlying transport.</summary>
    protected abstract bool TryPublishColorPaletteCore(ColorPaletteDto palette);

    /// <summary>Publishes a frame script to the underlying transport.</summary>
    protected abstract bool TryPublishFrameScriptCore(FrameScriptDto script);

    /// <summary>Publishes a transition to the underlying transport.</summary>
    protected abstract bool TryPublishTransitionCore(TransitionDto transition);

    /// <summary>Publishes a text style to the underlying transport.</summary>
    protected abstract bool TryPublishTextStyleCore(TextStyleDto style);

    /// <summary>Publishes a sprite collection event to the underlying transport.</summary>
    protected abstract bool TryPublishSpriteCollectionEventCore(RNetSpriteCollectionEventDto evt);

    /// <summary>Publishes a member property change to the underlying transport.</summary>
    protected abstract bool TryPublishMemberPropertyCore(RNetMemberPropertyDto property);

    /// <summary>Publishes a movie property change to the underlying transport.</summary>
    protected abstract bool TryPublishMoviePropertyCore(RNetMoviePropertyDto property);

    /// <summary>Publishes a stage property change to the underlying transport.</summary>
    protected abstract bool TryPublishStagePropertyCore(RNetStagePropertyDto property);

    private void OnActiveMovieChanged(IBlingoMovie? movie)
    {
        UnhookMovie();
        if (movie is not null)
        {
            HookMovie(movie);
        }
    }

    private void HookMovie(IBlingoMovie movie)
    {
        _movie = movie;
        Subscribe(movie, OnMoviePropertyChanged);
        if (movie is BlingoMovie lm)
        {
            lm.CurrentFrameChanged += OnFrameChanged;
            HookManager(lm.Sprite2DManager, "Sprite2D");
            HookManager(lm.Transitions, "Transition");
            HookManager(lm.Tempos, "Tempo");
            HookManager(lm.ColorPalettes, "ColorPalette");
            HookManager(lm.FrameScripts, "FrameScript");
            HookManager(lm.Audio, "Audio");
        }
    }

    private void UnhookMovie()
    {
        if (_movie is not null)
        {
            if (_movie is BlingoMovie lm)
            {
                lm.CurrentFrameChanged -= OnFrameChanged;
            }
            Unsubscribe(_movie);
        }

        foreach (var unsub in _managerUnsubs)
        {
            unsub();
        }
        _managerUnsubs.Clear();
    }

    private void HookManager<TSprite>(IBlingoSpriteManager<TSprite> manager, string name) where TSprite : BlingoSprite
    {
        void Added(TSprite sprite)
        {
            SubscribeSprite(sprite);
            RNetSpriteDto? dto = sprite is BlingoSprite2D s2d ? s2d.ToDto() : null;
            TryPublishSpriteCollectionEvent(new RNetSpriteCollectionEventDto(name, RNetSpriteCollectionEventType.Added, sprite.SpriteNum, sprite.BeginFrame, dto));
        }

        void Removed(TSprite sprite)
        {
            Unsubscribe(sprite);
            TryPublishSpriteCollectionEvent(new RNetSpriteCollectionEventDto(name, RNetSpriteCollectionEventType.Removed, sprite.SpriteNum, sprite.BeginFrame, null));
        }

        void Cleared()
            => TryPublishSpriteCollectionEvent(new RNetSpriteCollectionEventDto(name, RNetSpriteCollectionEventType.Cleared, 0, 0, null));

        manager.SpriteAdded += Added;
        manager.SpriteRemoved += Removed;
        manager.SpritesCleared += Cleared;

        foreach (var s in manager.GetAllSprites())
        {
            SubscribeSprite(s);
        }

        _managerUnsubs.Add(() =>
        {
            manager.SpriteAdded -= Added;
            manager.SpriteRemoved -= Removed;
            manager.SpritesCleared -= Cleared;
            foreach (var s in manager.GetAllSprites())
            {
                Unsubscribe(s);
            }
        });
    }

    private void OnFrameChanged(int _)
        => FlushQueuedProperties();

    private void OnMemberAdded(IBlingoMember member)
        => SubscribeMember(member);

    private void OnMemberDeleted(IBlingoMember member)
        => Unsubscribe(member);

    private void OnMemberPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is IBlingoMember member && e.PropertyName is not null)
        {
            if (member is IBlingoMemberTextBase text && e.PropertyName == nameof(IBlingoMemberTextBase.Text))
            {
                var md = text.GetTextMDString();
                TryPublishMemberProperty(new RNetMemberPropertyDto(member.CastLibNum, member.NumberInCast, "MarkDownText", md));
                return;
            }

            var value = member.GetType().GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(member)?.ToString() ?? string.Empty;
            TryPublishMemberProperty(new RNetMemberPropertyDto(member.CastLibNum, member.NumberInCast, e.PropertyName, value));
        }
    }

    private void OnSpitePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is BlingoSprite2D sprite2D && e.PropertyName is not null)
        {
            var value = sprite2D.ToDto();
            // todo
            //TryPublishSpriteProperty(value);
        }
    }

    private void OnMoviePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is IBlingoMovie movie && e.PropertyName is not null)
        {
            var value = movie.GetType().GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(movie)?.ToString() ?? string.Empty;
            TryPublishMovieProperty(new RNetMoviePropertyDto(e.PropertyName, value));
        }
    }

    private void OnStagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is IBlingoStage stage && e.PropertyName is not null)
        {
            var value = stage.GetType().GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(stage)?.ToString() ?? string.Empty;
            TryPublishStageProperty(new RNetStagePropertyDto(e.PropertyName, value));
        }
    }

    private void SubscribeMember(IBlingoMember member)
    {
        PropertyChangedEventHandler handler = OnMemberPropertyChanged;
        Subscribe(member, handler);
    }

    private void SubscribeSprite(BlingoSprite sprite)
    {
        PropertyChangedEventHandler handler = OnSpitePropertyChanged;
        // Currently no per-property sprite publishing.
        Subscribe(sprite, handler);
    }

   

    private void Subscribe(IHasPropertyChanged src, PropertyChangedEventHandler handler)
    {
        src.PropertyChanged += handler;
        _propSubs[src] = handler;
    }

    private void Unsubscribe(IHasPropertyChanged src)
    {
        if (_propSubs.Remove(src, out var h))
        {
            src.PropertyChanged -= h;
        }
    }
}

