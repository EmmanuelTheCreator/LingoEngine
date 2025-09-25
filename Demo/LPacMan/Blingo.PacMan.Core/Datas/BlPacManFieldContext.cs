using System;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Datas;

/// <summary>
/// Payload used to broadcast map rebuild events to consumable field behaviours.
/// </summary>
internal sealed class BlPacManFieldContext
{
    public BlPacManFieldContext(Map map, BlPacManGameBehavior coordinator)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        Coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
    }

    public Map Map { get; }

    public BlPacManGameBehavior Coordinator { get; }
}
