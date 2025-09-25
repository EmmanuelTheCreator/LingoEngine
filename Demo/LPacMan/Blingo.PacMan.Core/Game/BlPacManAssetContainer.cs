using System;
using System.Collections.Generic;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;

/// <summary>
/// Central registry that tracks active gameplay actors and mediates cross-behaviour
/// communication (Pac-Man position, consumable lifecycle, etc.).
/// </summary>
internal sealed class BlPacManAssetContainer
{
    private readonly List<BlPacManGhostBehavior> _ghosts = new();
    private readonly List<BlPacManConsumableComponent> _consumables = new();
    private readonly BlPacManEventMediator<BlPacManPositionContext> _pacManPositionChanged = new();
    private readonly BlPacManEventMediator<BlPacManConsumableComponent> _consumableRegistered = new();
    private readonly BlPacManEventMediator<BlPacManConsumableComponent> _consumableEaten = new();

    private BlPacManActorBehavior? _pacMan;
    private BlPacManRoamingBonusBehavior? _bonus;
    private BlPacManEventSubscription? _pacManPositionSubscription;
    private BlPacManPositionContext? _lastPacManPosition;

    /// <summary>
    /// Gets the list of active ghosts.
    /// </summary>
    public IReadOnlyList<BlPacManGhostBehavior> Ghosts => _ghosts;

    /// <summary>
    /// Gets the currently registered Pac-Man behaviour.
    /// </summary>
    public BlPacManActorBehavior? PacMan => _pacMan;

    /// <summary>
    /// Gets the active roaming bonus behaviour, if any.
    /// </summary>
    public BlPacManRoamingBonusBehavior? Bonus => _bonus;

    /// <summary>
    /// Gets the number of consumables that are currently placed on the map.
    /// </summary>
    public int ActiveConsumableCount => _consumables.Count;

    /// <summary>
    /// Registers Pac-Man and begins relaying his position to interested listeners.
    /// </summary>
    public void AttachPacMan(BlPacManActorBehavior pacMan)
    {
        _pacMan = pacMan ?? throw new ArgumentNullException(nameof(pacMan));
        _pacManPositionSubscription?.Release();
        _pacManPositionSubscription = pacMan.SubscribePacManPosition(OnPacManPositionChanged);
        BroadcastPacManPosition();
    }

    /// <summary>
    /// Removes Pac-Man from the registry and stops the position feed.
    /// </summary>
    public void DetachPacMan(BlPacManActorBehavior pacMan)
    {
        if (!ReferenceEquals(_pacMan, pacMan))
        {
            return;
        }

        _pacManPositionSubscription?.Release();
        _pacManPositionSubscription = null;
        _pacMan = null;
        _lastPacManPosition = null;
    }

    /// <summary>
    /// Registers a ghost so the gameplay coordinator can iterate over the collection.
    /// </summary>
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

    /// <summary>
    /// Removes the specified ghost from the registry.
    /// </summary>
    public void RemoveGhost(BlPacManGhostBehavior ghost)
    {
        if (ghost is null)
        {
            return;
        }

        _ghosts.Remove(ghost);
    }

    /// <summary>
    /// Registers the roaming bonus behaviour.
    /// </summary>
    public void AttachBonus(BlPacManRoamingBonusBehavior bonus)
    {
        _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
    }

    /// <summary>
    /// Clears the roaming bonus reference if it matches the provided instance.
    /// </summary>
    public void DetachBonus(BlPacManRoamingBonusBehavior bonus)
    {
        if (ReferenceEquals(_bonus, bonus))
        {
            _bonus = null;
        }
    }

    /// <summary>
    /// Tracks a newly spawned consumable component.
    /// </summary>
    public void RegisterConsumable(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            throw new ArgumentNullException(nameof(consumable));
        }

        _consumables.Add(consumable);
        _consumableRegistered.Publish(consumable);
    }

    /// <summary>
    /// Removes a consumable without raising an eaten notification (e.g. level reset).
    /// </summary>
    public void UnregisterConsumable(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            return;
        }

        _consumables.Remove(consumable);
    }

    /// <summary>
    /// Notifies listeners that a consumable has been eaten and removes it from the registry.
    /// </summary>
    public void NotifyConsumableEaten(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            return;
        }

        if (_consumables.Remove(consumable))
        {
            _consumableEaten.Publish(consumable);
        }
    }

    /// <summary>
    /// Subscribes to the stream of Pac-Man position updates.
    /// </summary>
    public BlPacManEventSubscription SubscribePacManPosition(Action<BlPacManPositionContext> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var subscription = _pacManPositionChanged.Subscribe(handler);
        if (_lastPacManPosition is { } snapshot)
        {
            handler(snapshot);
        }

        return subscription;
    }

    /// <summary>
    /// Subscribes to notifications when new consumables are created.
    /// </summary>
    public BlPacManEventSubscription SubscribeConsumableRegistered(Action<BlPacManConsumableComponent> handler)
    {
        return _consumableRegistered.Subscribe(handler);
    }

    /// <summary>
    /// Subscribes to notifications when Pac-Man eats a consumable.
    /// </summary>
    public BlPacManEventSubscription SubscribeConsumableEaten(Action<BlPacManConsumableComponent> handler)
    {
        return _consumableEaten.Subscribe(handler);
    }

    /// <summary>
    /// Broadcasts Pac-Man's current position to all subscribers.
    /// </summary>
    public void BroadcastPacManPosition()
    {
        if (_pacMan is null)
        {
            return;
        }

        var character = _pacMan.Character;
        var sprite = _pacMan.GetSprite();
        var snapshot = new BlPacManPositionContext(sprite.LocH, sprite.LocV, character.GetTile(), character.Direction);
        OnPacManPositionChanged(snapshot);
    }

    /// <summary>
    /// Clears runtime registrations while preserving event subscriptions.
    /// </summary>
    public void Reset()
    {
        _ghosts.Clear();
        _consumables.Clear();
        _bonus = null;
        _pacMan = null;
        _pacManPositionSubscription?.Release();
        _pacManPositionSubscription = null;
        _lastPacManPosition = null;
    }

    private void OnPacManPositionChanged(BlPacManPositionContext context)
    {
        _lastPacManPosition = context;
        _pacManPositionChanged.Publish(context);
    }
}
