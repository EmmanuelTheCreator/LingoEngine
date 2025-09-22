using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Commands;
using AbstUI.Primitives;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Movies;
using BlingoEngine.Stages;

namespace BlingoEngine.Director.Core.Movies.Commands;

public sealed class BlingoMovieCommandHandler : IAbstCommandHandler<BlingoUpdateMoviePropertiesCommand>
{
    private readonly BlingoPlayer _player;
    private readonly IHistoryManager _historyManager;
    private readonly IDirectorEventMediator _mediator;
    private IBlingoMovie? _cachedMovie;

    public BlingoMovieCommandHandler(BlingoPlayer player, IHistoryManager historyManager, IDirectorEventMediator mediator)
    {
        _player = player;
        _historyManager = historyManager;
        _mediator = mediator;
    }

    public bool CanExecute(BlingoUpdateMoviePropertiesCommand command)
    {
        if (command == null || command.Changes.Count == 0)
            return false;

        _cachedMovie = _player.GetMovie(command.MovieReference);
        return _cachedMovie != null;
    }

    public bool Handle(BlingoUpdateMoviePropertiesCommand command)
    {
        var movie = _cachedMovie ?? _player.GetMovie(command.MovieReference);
        _cachedMovie = null;

        if (movie == null)
            return false;

        var stage = _player.Stage;
        var updates = CollectChanges(movie, stage, command.Changes);
        if (updates.Count == 0)
            return true;

        bool notifyStage = updates.Any(u => u.Mutation.NotifyStage);
        bool notifyCast = updates.Any(u => u.Mutation.NotifyCast);

        ApplyMutations(updates);
        Notify(notifyStage, notifyCast);

        void Undo()
        {
            RevertMutations(updates);
            Notify(notifyStage, notifyCast);
        }

        void Redo()
        {
            ApplyMutations(updates);
            Notify(notifyStage, notifyCast);
        }

        _historyManager.Push(Undo, Redo);
        return true;
    }

    private void Notify(bool notifyStage, bool notifyCast)
    {
        if (notifyStage)
            _mediator.Raise(DirectorEventType.StagePropertiesChanged);
        if (notifyCast)
            _mediator.Raise(DirectorEventType.CastPropertiesChanged);
    }

    private static List<CollectedChange> CollectChanges(IBlingoMovie movie, IBlingoStage stage, IReadOnlyList<APropertyValue> changes)
    {
        var updates = new List<CollectedChange>();
        if (changes.Count == 0)
            return updates;

        foreach (var change in changes)
        {
            if (string.IsNullOrWhiteSpace(change.PropertyName))
                continue;

            var mutation = RetrievePropertyMutation(movie, stage, change);
            if (mutation != null)
                updates.Add(new CollectedChange(change, mutation.Value));
        }

        return updates;
    }

    private static void ApplyMutations(IEnumerable<CollectedChange> changes)
    {
        foreach (var change in changes)
            change.Mutation.Apply();
    }

    private static void RevertMutations(IReadOnlyList<CollectedChange> changes)
    {
        for (int i = changes.Count - 1; i >= 0; i--)
            changes[i].Mutation.Revert();
    }

    private static BlingoMoviePropertyMutation? RetrievePropertyMutation(IBlingoMovie movie, IBlingoStage stage, APropertyValue change)
    {
        switch (change.PropertyName)
        {
            case nameof(BlingoStage.Width) when change.TryGetFloat(out var newWidth):
            {
                var current = movie.Width;
                if (Math.Abs(current - newWidth) <= float.Epsilon)
                    return null;
                return new BlingoMoviePropertyMutation(
                    () => movie.Width = newWidth,
                    () => movie.Width = current,
                    notifyStage: true,
                    notifyCast: false);
            }

            case nameof(BlingoStage.Height) when change.TryGetFloat(out var newHeight):
            {
                var current = movie.Height;
                if (Math.Abs(current - newHeight) <= float.Epsilon)
                    return null;
                return new BlingoMoviePropertyMutation(
                    () => movie.Height = newHeight,
                    () => movie.Height = current,
                    notifyStage: true,
                    notifyCast: false);
            }

            case nameof(BlingoStage.BackgroundColor) when change.Value is AColor newColor:
            {
                var current = stage.BackgroundColor;
                if (current.Equals(newColor))
                    return null;
                return new BlingoMoviePropertyMutation(
                    () => stage.BackgroundColor = newColor,
                    () => stage.BackgroundColor = current,
                    notifyStage: true,
                    notifyCast: false);
            }

            case nameof(BlingoMovie.MaxSpriteChannelCount) when change.TryGetInt(out var newChannelCount):
            {
                var current = movie.MaxSpriteChannelCount;
                if (current == newChannelCount)
                    return null;
                return new BlingoMoviePropertyMutation(
                    () => movie.MaxSpriteChannelCount = newChannelCount,
                    () => movie.MaxSpriteChannelCount = current,
                    notifyStage: true,
                    notifyCast: true);
            }

            case nameof(BlingoMovie.About):
            {
                var newValue = change.Value?.ToString() ?? string.Empty;
                var current = movie.About ?? string.Empty;
                if (current == newValue)
                    return null;
                return new BlingoMoviePropertyMutation(
                    () => movie.About = newValue,
                    () => movie.About = current,
                    notifyStage: false,
                    notifyCast: false);
            }

            case nameof(BlingoMovie.Copyright):
            {
                var newValue = change.Value?.ToString() ?? string.Empty;
                var current = movie.Copyright ?? string.Empty;
                if (current == newValue)
                    return null;
                return new BlingoMoviePropertyMutation(
                    () => movie.Copyright = newValue,
                    () => movie.Copyright = current,
                    notifyStage: false,
                    notifyCast: false);
            }

            case nameof(BlingoMovie.UserName):
            {
                var newValue = change.Value?.ToString() ?? string.Empty;
                var current = movie.UserName ?? string.Empty;
                if (current == newValue)
                    return null;
                return new BlingoMoviePropertyMutation(
                    () => movie.UserName = newValue,
                    () => movie.UserName = current,
                    notifyStage: false,
                    notifyCast: false);
            }

            case nameof(BlingoMovie.CompanyName):
            {
                var newValue = change.Value?.ToString() ?? string.Empty;
                var current = movie.CompanyName ?? string.Empty;
                if (current == newValue)
                    return null;
                return new BlingoMoviePropertyMutation(
                    () => movie.CompanyName = newValue,
                    () => movie.CompanyName = current,
                    notifyStage: false,
                    notifyCast: false);
            }
        }

        return null;
    }

    private sealed record CollectedChange(APropertyValue Request, BlingoMoviePropertyMutation Mutation);

    private readonly struct BlingoMoviePropertyMutation
    {
        public BlingoMoviePropertyMutation(Action apply, Action revert, bool notifyStage, bool notifyCast)
        {
            Apply = apply;
            Revert = revert;
            NotifyStage = notifyStage;
            NotifyCast = notifyCast;
        }

        public Action Apply { get; }
        public Action Revert { get; }
        public bool NotifyStage { get; }
        public bool NotifyCast { get; }
    }
}
