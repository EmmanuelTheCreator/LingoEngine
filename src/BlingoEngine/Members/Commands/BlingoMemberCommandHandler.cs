using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AbstUI.Commands;
using AbstUI.Primitives;
using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Movies;

namespace BlingoEngine.Members.Commands;

public class BlingoMemberCommandHandler : IAbstCommandHandler<BlingoUpdateMemberPropertiesCommand>
{
    private readonly BlingoPlayer _player;
    private readonly IHistoryManager? _historyManager;
    private IBlingoMember? _cachedMember;

    public BlingoMemberCommandHandler(BlingoPlayer player, IHistoryManager? historyManager = null)
    {
        _player = player;
        _historyManager = historyManager;
    }

    public bool CanExecute(BlingoUpdateMemberPropertiesCommand command)
    {
        if (command == null || command.Changes.Count == 0)
            return false;

        if (_player.ActiveMovie is not BlingoMovie)
            return false;

        _cachedMember = _player.GetMember(command.MemberReference);
        return _cachedMember != null;
    }

    public bool Handle(BlingoUpdateMemberPropertiesCommand command)
    {
        if (_player.ActiveMovie is not BlingoMovie)
            return false;

        var member = _cachedMember ?? _player.GetMember(command.MemberReference);
        _cachedMember = null;

        if (member == null)
            return false;

        var updates = CollectChanges(member, command.Changes);
        if (updates.Count == 0)
            return true;

        bool requiresCastRefresh = updates.Any(u => u.Mutation.RequiresCastRefresh);

        ApplyMutations(updates);
        NotifyMemberChanged(member, requiresCastRefresh);

        void Undo()
        {
            RevertMutations(updates);
            NotifyMemberChanged(member, requiresCastRefresh);
        }

        void Redo()
        {
            ApplyMutations(updates);
            NotifyMemberChanged(member, requiresCastRefresh);
        }

        TrackHistory(Undo, Redo);
        return true;
    }

    protected virtual void NotifyMemberChanged(IBlingoMember member, bool refreshCast)
    {
    }

    private void TrackHistory(Action undo, Action redo)
    {
        if (_historyManager == null)
            return;

        _historyManager.Push(undo, redo);
    }

    private static List<CollectedChange> CollectChanges(IBlingoMember member, IReadOnlyList<APropertyValue> changes)
    {
        var updates = new List<CollectedChange>();
        if (changes.Count == 0)
            return updates;

        foreach (var change in changes)
        {
            if (string.IsNullOrWhiteSpace(change.PropertyName))
                continue;

            var mutation = CreateMutation(member, change);
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

    private static BlingoMemberPropertyMutation? CreateMutation(IBlingoMember member, APropertyValue change)
    {
        var property = FindProperty(member.GetType(), change.PropertyName);
        if (property == null || !property.CanWrite)
            return null;

        var currentValue = property.GetValue(member);
        var newValue = change.Value;

        if (Equals(currentValue, newValue))
            return null;

        bool requiresCastRefresh = string.Equals(property.Name, nameof(BlingoMember.Name), StringComparison.Ordinal);
        return new BlingoMemberPropertyMutation(
            () => property.SetValue(member, newValue),
            () => property.SetValue(member, currentValue),
            requiresCastRefresh);
    }

    private static PropertyInfo? FindProperty(Type type, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return null;

        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        return type.GetProperty(propertyName, Flags)
            ?? type.GetProperty(propertyName, Flags | BindingFlags.IgnoreCase);
    }

    private sealed record CollectedChange(APropertyValue Request, BlingoMemberPropertyMutation Mutation);

    private readonly struct BlingoMemberPropertyMutation
    {
        public BlingoMemberPropertyMutation(Action apply, Action revert, bool requiresCastRefresh)
        {
            Apply = apply;
            Revert = revert;
            RequiresCastRefresh = requiresCastRefresh;
        }

        public Action Apply { get; }
        public Action Revert { get; }
        public bool RequiresCastRefresh { get; }
    }
}
