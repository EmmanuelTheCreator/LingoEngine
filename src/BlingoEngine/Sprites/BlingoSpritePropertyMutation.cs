using System;

namespace BlingoEngine.Sprites;

public sealed record BlingoSpritePropertyMutation(
    Action Apply,
    Action Revert,
    bool RequiresStageRefresh,
    bool IsAnimationProperty,
    object? NewValue,
    object? OriginalValue);
