using System;
using Blingo.PacMan.Core.Sprites.Behaviors;

namespace Blingo.PacMan.Core.Game;

/// <summary>
/// Payload used to broadcast map rebuild events to consumable field behaviours.
/// </summary>
internal sealed class PacManFieldContext
{
    public PacManFieldContext(Map map, PacManGameBehavior coordinator)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
    }

    public Map Map { get; }

    public PacManGameBehavior Coordinator { get; }
}
