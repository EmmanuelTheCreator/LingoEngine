using System;
using System.Collections.Generic;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;

internal sealed class BlPacManAssetContainer
{
    private readonly List<BlPacManGhostBehavior> _ghosts = new();
    private BlPacManActorBehavior? _pacMan;
    private BlPacManRoamingBonusBehavior? _bonus;
    private BlPacManPositionContext? _lastPacManPosition;

    public IReadOnlyList<BlPacManGhostBehavior> Ghosts => _ghosts;

    public BlPacManActorBehavior? PacMan => _pacMan;

    public BlPacManRoamingBonusBehavior? Bonus => _bonus;

    public BlPacManPositionContext? PacManPosition => _lastPacManPosition;

    public void AttachPacMan(BlPacManActorBehavior pacMan)
    {
        _pacMan = pacMan ?? throw new ArgumentNullException(nameof(pacMan));
    }

    public void DetachPacMan(BlPacManActorBehavior pacMan)
    {
        if (!ReferenceEquals(_pacMan, pacMan))
        {
            return;
        }

        _pacMan = null;
        _lastPacManPosition = null;
    }

    public void AddGhost(BlPacManGhostBehavior ghost)
    {
        if (ghost is null)
        {
            throw new ArgumentNullException(nameof(ghost));
        }

        if (!_ghosts.Contains(ghost))
        {
            _ghosts.Add(ghost);
        }
    }

    public void RemoveGhost(BlPacManGhostBehavior ghost)
    {
        if (ghost is null)
        {
            return;
        }

        _ghosts.Remove(ghost);
    }

    public void AttachBonus(BlPacManRoamingBonusBehavior bonus)
    {
        _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
    }

    public void DetachBonus(BlPacManRoamingBonusBehavior bonus)
    {
        if (ReferenceEquals(_bonus, bonus))
        {
            _bonus = null;
        }
    }

    public void UpdatePacManPosition(BlPacManPositionContext context)
    {
        _lastPacManPosition = context;
    }

    public void Reset()
    {
        _ghosts.Clear();
        _bonus = null;
        _pacMan = null;
        _lastPacManPosition = null;
    }
}
