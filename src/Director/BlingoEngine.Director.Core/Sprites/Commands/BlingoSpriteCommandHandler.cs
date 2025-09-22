using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Commands;
using AbstUI.Primitives;
using BlingoEngine.Animations;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Sprites.Commands;

public class BlingoSpriteCommandHandler :
    IAbstCommandHandler<BlingoUpdateSpritePropertiesCommand>
{
    private readonly DirSpritesManager _spritesManager;
    private readonly BlingoPlayer _player;
    private readonly IHistoryManager _historyManager;
    private BlingoSprite? _cachedSprite;

    public BlingoSpriteCommandHandler(DirSpritesManager spritesManager, BlingoPlayer player, IHistoryManager historyManager)
    {
        _spritesManager = spritesManager;
        _player = player;
        _historyManager = historyManager;
    }

    public bool CanExecute(BlingoUpdateSpritePropertiesCommand command)
    {
        if (command == null || command.Changes.Count == 0)
            return false;

        if (_player.ActiveMovie is not BlingoMovie movie)
            return false;

        _cachedSprite = movie.GetSprite(command.SpriteReference);
        return _cachedSprite != null;
    }

    public bool Handle(BlingoUpdateSpritePropertiesCommand command)
    {
        if (_player.ActiveMovie is not BlingoMovie movie)
            return false;

        var sprite = _cachedSprite ?? movie.GetSprite(command.SpriteReference);
        _cachedSprite = null;

        if (sprite == null)
            return false;

        var updates = CollectChanges(movie, command.SpriteReference, sprite, command.Changes);
        if (updates.Count == 0)
            return true;

        bool requiresStageRefresh = updates.Any(u => u.Mutation.RequiresStageRefresh);
        AnimationUpdate? animationUpdate = null;

        if (sprite is BlingoSprite2D sprite2D)
        {
            animationUpdate = PrepareAnimationUpdate(sprite2D, updates, movie.CurrentFrame);
            if (animationUpdate != null)
                requiresStageRefresh = true;
        }

        ApplyMutations(updates);
        animationUpdate?.Apply();
        UpdateStageIfNeeded(sprite, requiresStageRefresh);

        void Undo()
        {
            RevertMutations(updates);
            animationUpdate?.Revert();
            UpdateStageIfNeeded(sprite, requiresStageRefresh);
            _spritesManager.ChannelChanged(sprite.SpriteNumWithChannel);
            _spritesManager.Mediator.RaiseSpriteSelected(sprite);
        }

        void Redo()
        {
            ApplyMutations(updates);
            animationUpdate?.Apply();
            UpdateStageIfNeeded(sprite, requiresStageRefresh);
            _spritesManager.ChannelChanged(sprite.SpriteNumWithChannel);
            _spritesManager.Mediator.RaiseSpriteSelected(sprite);
        }

        _historyManager.Push(Undo, Redo);
        _spritesManager.ChannelChanged(sprite.SpriteNumWithChannel);
        _spritesManager.Mediator.RaiseSpriteSelected(sprite);
        return true;
    }

    private sealed record CollectedChange(APropertyValue Request, BlingoSpritePropertyMutation Mutation);

    private readonly struct AnimationUpdate
    {
        public AnimationUpdate(Action apply, Action revert)
        {
            Apply = apply;
            Revert = revert;
        }

        public Action Apply { get; }
        public Action Revert { get; }
    }

    private static List<CollectedChange> CollectChanges(BlingoMovie movie, BlingoSpriteRef spriteRef, BlingoSprite sprite, IReadOnlyList<APropertyValue> changes)
    {
        var updates = new List<CollectedChange>();
        if (changes.Count == 0)
            return updates;

        var manager = movie.GetSpriteManager(spriteRef.SpriteType);
        if (manager == null)
            return updates;

        foreach (var change in changes)
        {
            if (string.IsNullOrWhiteSpace(change.PropertyName))
                continue;

            var mutation = manager.RetrievePropertyMutation(sprite, change);

            if (mutation != null)
                updates.Add(new CollectedChange(change, mutation));
        }

        return updates;
    }

    private static void ApplyMutations(IReadOnlyList<CollectedChange> changes)
    {
        foreach (var change in changes)
            change.Mutation.Apply();
    }

    private static void RevertMutations(IReadOnlyList<CollectedChange> changes)
    {
        for (int i = changes.Count - 1; i >= 0; i--)
            changes[i].Mutation.Revert();
    }

    private void UpdateStageIfNeeded(BlingoSprite sprite, bool shouldUpdateStage)
    {
        if (!shouldUpdateStage)
            return;

        if (sprite is BlingoSprite2D sprite2D)
            _player.Stage.UpdateKeyFrame(sprite2D);
    }

    private static AnimationUpdate? PrepareAnimationUpdate(BlingoSprite2D sprite, IReadOnlyList<CollectedChange> changes, int currentFrame)
    {
        var animationChanges = changes.Where(c => c.Mutation.IsAnimationProperty).ToList();
        if (animationChanges.Count == 0)
            return null;

        var keyframes = sprite.GetKeyframes();
        if (keyframes == null || keyframes.Count == 0)
            return null;

        int relativeFrame = Math.Max(0, currentFrame - sprite.BeginFrame);
        var targetKeyframe = sprite.GetKeyFrameForFrame(relativeFrame);
        if (targetKeyframe == null)
            return null;

        var original = targetKeyframe.Value;
        var updated = original;

        bool positionChanged = false;
        bool sizeChanged = false;

        float? locH = null;
        float? locV = null;
        float? width = null;
        float? height = null;
        float? rotation = null;
        float? skew = null;
        float? blend = null;
        AColor? foreColor = null;
        AColor? backColor = null;

        foreach (var change in animationChanges)
        {
            switch (change.Request.PropertyName)
            {
                case nameof(BlingoSprite2D.LocH):
                    if (APropertyValue.TryGetFloat(change.Mutation.NewValue, out var newLocH))
                    {
                        locH = newLocH;
                        positionChanged = true;
                    }
                    break;
                case nameof(BlingoSprite2D.LocV):
                    if (APropertyValue.TryGetFloat(change.Mutation.NewValue, out var newLocV))
                    {
                        locV = newLocV;
                        positionChanged = true;
                    }
                    break;
                case nameof(BlingoSprite2D.Width):
                    if (APropertyValue.TryGetFloat(change.Mutation.NewValue, out var newWidth))
                    {
                        width = newWidth;
                        sizeChanged = true;
                    }
                    break;
                case nameof(BlingoSprite2D.Height):
                    if (APropertyValue.TryGetFloat(change.Mutation.NewValue, out var newHeight))
                    {
                        height = newHeight;
                        sizeChanged = true;
                    }
                    break;
                case nameof(BlingoSprite2D.Rotation):
                    if (APropertyValue.TryGetFloat(change.Mutation.NewValue, out var newRotation))
                        rotation = newRotation;
                    break;
                case nameof(BlingoSprite2D.Skew):
                    if (APropertyValue.TryGetFloat(change.Mutation.NewValue, out var newSkew))
                        skew = newSkew;
                    break;
                case nameof(BlingoSprite2D.Blend):
                    if (APropertyValue.TryGetFloat(change.Mutation.NewValue, out var newBlend))
                        blend = newBlend;
                    break;
                case nameof(BlingoSprite2D.ForeColor):
                    if (change.Mutation.NewValue is AColor newForeColor)
                        foreColor = newForeColor;
                    break;
                case nameof(BlingoSprite2D.BackColor):
                    if (change.Mutation.NewValue is AColor newBackColor)
                        backColor = newBackColor;
                    break;
            }
        }

        if (positionChanged)
        {
            var basePosition = original.Position ?? new APoint(sprite.LocH, sprite.LocV);
            float x = locH ?? basePosition.X;
            float y = locV ?? basePosition.Y;
            updated.Position = new APoint(x, y);
        }

        if (sizeChanged)
        {
            var baseSize = original.Size ?? new APoint(sprite.Width, sprite.Height);
            float sizeX = width ?? baseSize.X;
            float sizeY = height ?? baseSize.Y;
            updated.Size = new APoint(sizeX, sizeY);
        }

        if (rotation.HasValue)
            updated.Rotation = rotation.Value;

        if (skew.HasValue)
            updated.Skew = skew.Value;

        if (blend.HasValue)
            updated.Blend = blend.Value;

        if (foreColor.HasValue)
            updated.ForeColor = foreColor.Value;

        if (backColor.HasValue)
            updated.BackColor = backColor.Value;

        return new AnimationUpdate(
            () => sprite.UpdateKeyframe(updated),
            () => sprite.UpdateKeyframe(original));
    }
}
