using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Commands;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;

namespace BlingoEngine.Director.Core.Members.Commands;

public sealed class BlingoMemberCommandHandler : IAbstCommandHandler<BlingoUpdateMemberPropertiesCommand>
{
    private readonly BlingoPlayer _player;
    private readonly IHistoryManager _historyManager;
    private readonly IDirectorEventMediator _mediator;
    private IBlingoMember? _cachedMember;

    public BlingoMemberCommandHandler(BlingoPlayer player, IHistoryManager historyManager, IDirectorEventMediator mediator)
    {
        _player = player;
        _historyManager = historyManager;
        _mediator = mediator;
    }

    public bool CanExecute(BlingoUpdateMemberPropertiesCommand command)
    {
        if (command == null || command.Changes.Count == 0)
            return false;

        if (_player.ActiveMovie is not BlingoMovie movie)
            return false;

        _cachedMember = movie.GetMember(command.MemberReference);
        return _cachedMember != null;
    }

    public bool Handle(BlingoUpdateMemberPropertiesCommand command)
    {
        if (_player.ActiveMovie is not BlingoMovie movie)
            return false;

        var member = _cachedMember ?? movie.GetMember(command.MemberReference);
        _cachedMember = null;

        if (member == null)
            return false;

        var updates = CollectChanges(member, command.Changes);
        if (updates.Count == 0)
            return true;

        bool requiresCastRefresh = updates.Any(u => u.Mutation.RequiresCastRefresh);

        ApplyMutations(updates);
        Notify(member, requiresCastRefresh);

        void Undo()
        {
            RevertMutations(updates);
            Notify(member, requiresCastRefresh);
        }

        void Redo()
        {
            ApplyMutations(updates);
            Notify(member, requiresCastRefresh);
        }

        _historyManager.Push(Undo, Redo);
        return true;
    }

    private void Notify(IBlingoMember member, bool refreshCast)
    {
        if (refreshCast)
            _mediator.Raise(DirectorEventType.CastPropertiesChanged);

        _mediator.RaiseMemberSelected(member);
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

            var mutation = RetrievePropertyMutation(member, change);
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

    private static BlingoMemberPropertyMutation? RetrievePropertyMutation(IBlingoMember member, APropertyValue change)
    {
        var common = RetrieveCommonMutation(member, change);
        if (common != null)
            return common;

        return member switch
        {
            BlingoMemberSound sound => RetrieveSoundMutation(sound, change),
            BlingoMemberShape shape => RetrieveShapeMutation(shape, change),
            IBlingoMemberTextBase text => RetrieveTextMutation(text, change),
            BlingoMemberBitmap bitmap => RetrieveBitmapMutation(bitmap, change),
            _ => null,
        };
    }

    private static BlingoMemberPropertyMutation? RetrieveCommonMutation(IBlingoMember member, APropertyValue change)
    {
        switch (change.PropertyName)
        {
            case nameof(BlingoMember.Name):
            {
                var newName = change.Value?.ToString() ?? string.Empty;
                if (member.Name == newName)
                    return null;
                string oldValue = member.Name;
                return new BlingoMemberPropertyMutation(
                    () => member.Name = newName,
                    () => member.Name = oldValue,
                    requiresCastRefresh: true);
            }

            case nameof(BlingoMember.Comments):
            {
                var newComments = change.Value?.ToString() ?? string.Empty;
                if (member.Comments == newComments)
                    return null;
                string oldValue = member.Comments;
                return new BlingoMemberPropertyMutation(
                    () => member.Comments = newComments,
                    () => member.Comments = oldValue,
                    requiresCastRefresh: false);
            }

            case nameof(BlingoMember.FileName) when member is BlingoMember baseMember:
            {
                var newFileName = change.Value?.ToString() ?? string.Empty;
                if (baseMember.FileName == newFileName)
                    return null;
                string oldValue = baseMember.FileName;
                return new BlingoMemberPropertyMutation(
                    () => baseMember.FileName = newFileName,
                    () => baseMember.FileName = oldValue,
                    requiresCastRefresh: false);
            }

            case nameof(BlingoMember.RegPoint) when change.Value is APoint newPoint:
            {
                if (member.RegPoint == newPoint)
                    return null;
                var oldValue = member.RegPoint;
                return new BlingoMemberPropertyMutation(
                    () => member.RegPoint = newPoint,
                    () => member.RegPoint = oldValue,
                    requiresCastRefresh: false);
            }

            case nameof(BlingoMember.Width) when change.TryGetInt(out var newWidth):
            {
                if (member.Width == newWidth)
                    return null;
                int oldValue = member.Width;
                return new BlingoMemberPropertyMutation(
                    () => member.Width = newWidth,
                    () => member.Width = oldValue,
                    requiresCastRefresh: false);
            }

            case nameof(BlingoMember.Height) when change.TryGetInt(out var newHeight):
            {
                if (member.Height == newHeight)
                    return null;
                int oldValue = member.Height;
                return new BlingoMemberPropertyMutation(
                    () => member.Height = newHeight,
                    () => member.Height = oldValue,
                    requiresCastRefresh: false);
            }

            case nameof(BlingoMember.Hilite) when change.Value is bool newHilite && member is BlingoMember baseMember:
            {
                if (baseMember.Hilite == newHilite)
                    return null;
                bool oldValue = baseMember.Hilite;
                return new BlingoMemberPropertyMutation(
                    () => baseMember.SetHilite(newHilite),
                    () => baseMember.SetHilite(oldValue),
                    requiresCastRefresh: true);
            }
        }

        return null;
    }

    private static BlingoMemberPropertyMutation? RetrieveSoundMutation(BlingoMemberSound sound, APropertyValue change)
    {
        if (change.PropertyName == nameof(BlingoMemberSound.Loop) && change.Value is bool newLoop)
        {
            if (sound.Loop == newLoop)
                return null;
            bool oldValue = sound.Loop;
            return new BlingoMemberPropertyMutation(
                () => sound.Loop = newLoop,
                () => sound.Loop = oldValue,
                requiresCastRefresh: false);
        }

        return null;
    }

    private static BlingoMemberPropertyMutation? RetrieveShapeMutation(BlingoMemberShape shape, APropertyValue change)
    {
        switch (change.PropertyName)
        {
            case nameof(BlingoMemberShape.ShapeTypeInt) when change.TryGetInt(out var newType):
            {
                if (shape.ShapeTypeInt == newType)
                    return null;
                int oldValue = shape.ShapeTypeInt;
                return new BlingoMemberPropertyMutation(
                    () => shape.ShapeTypeInt = newType,
                    () => shape.ShapeTypeInt = oldValue,
                    requiresCastRefresh: false);
            }

            case nameof(BlingoMemberShape.Filled) when change.Value is bool newFilled:
            {
                if (shape.Filled == newFilled)
                    return null;
                bool oldValue = shape.Filled;
                return new BlingoMemberPropertyMutation(
                    () => shape.Filled = newFilled,
                    () => shape.Filled = oldValue,
                    requiresCastRefresh: false);
            }
        }

        return null;
    }

    private static BlingoMemberPropertyMutation? RetrieveTextMutation(IBlingoMemberTextBase text, APropertyValue change)
    {
        // Width and Height are handled in common mutation handling.
        return null;
    }

    private static BlingoMemberPropertyMutation? RetrieveBitmapMutation(BlingoMemberBitmap bitmap, APropertyValue change)
    {
        // Bitmap-specific properties are handled by the common branch for now.
        return null;
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
