using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Commands;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Core;

namespace BlingoEngine.Casts.Commands;

public class BlingoCastCommandHandler : IAbstCommandHandler<BlingoUpdateCastPropertiesCommand>
{
    private readonly BlingoPlayer _player;
    private readonly IHistoryManager? _historyManager;
    private BlingoCast? _cachedCast;

    public BlingoCastCommandHandler(BlingoPlayer player, IHistoryManager? historyManager = null)
    {
        _player = player;
        _historyManager = historyManager;
    }

    public bool CanExecute(BlingoUpdateCastPropertiesCommand command)
    {
        if (command == null || command.Changes.Count == 0)
            return false;

        _cachedCast = _player.GetCast(command.CastReference) as BlingoCast;
        return _cachedCast != null;
    }

    public bool Handle(BlingoUpdateCastPropertiesCommand command)
    {
        var cast = _cachedCast ?? _player.GetCast(command.CastReference) as BlingoCast;
        _cachedCast = null;

        if (cast == null)
            return false;

        var updates = CollectChanges(cast, command.Changes);
        if (updates.Count == 0)
            return true;

        ApplyMutations(updates);
        NotifyCastChanged();

        void Undo()
        {
            RevertMutations(updates);
            NotifyCastChanged();
        }

        void Redo()
        {
            ApplyMutations(updates);
            NotifyCastChanged();
        }

        TrackHistory(Undo, Redo);
        return true;
    }

    protected virtual void NotifyCastChanged()
    {
    }

    private void TrackHistory(Action undo, Action redo)
    {
        if (_historyManager == null)
            return;

        _historyManager.Push(undo, redo);
    }

    private static List<CollectedChange> CollectChanges(BlingoCast cast, IReadOnlyList<APropertyValue> changes)
    {
        var updates = new List<CollectedChange>();
        if (changes.Count == 0)
            return updates;

        foreach (var change in changes)
        {
            if (string.IsNullOrWhiteSpace(change.PropertyName))
                continue;

            var mutation = RetrievePropertyMutation(cast, change);
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

    private static BlingoCastPropertyMutation? RetrievePropertyMutation(BlingoCast cast, APropertyValue change)
    {
        switch (change.PropertyName)
        {
            case nameof(BlingoCast.Name):
            {
                var newName = change.Value?.ToString() ?? string.Empty;
                var current = cast.Name ?? string.Empty;
                if (current == newName)
                    return null;
                return new BlingoCastPropertyMutation(
                    () => cast.Name = newName,
                    () => cast.Name = current);
            }

            case nameof(BlingoCast.FileName):
            {
                var newValue = change.Value?.ToString() ?? string.Empty;
                var current = cast.FileName ?? string.Empty;
                if (current == newValue)
                    return null;
                return new BlingoCastPropertyMutation(
                    () => cast.FileName = newValue,
                    () => cast.FileName = current);
            }

            case nameof(BlingoCast.PreLoadMode) when change.Value is PreLoadModeType mode:
            {
                var current = cast.PreLoadMode;
                if (current == mode)
                    return null;
                return new BlingoCastPropertyMutation(
                    () => cast.PreLoadMode = mode,
                    () => cast.PreLoadMode = current);
            }
        }

        return null;
    }

    private sealed record CollectedChange(APropertyValue Request, BlingoCastPropertyMutation Mutation);

    private readonly struct BlingoCastPropertyMutation
    {
        public BlingoCastPropertyMutation(Action apply, Action revert)
        {
            Apply = apply;
            Revert = revert;
        }

        public Action Apply { get; }
        public Action Revert { get; }
    }
}
