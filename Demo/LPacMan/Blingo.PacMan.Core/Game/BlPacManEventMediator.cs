namespace Blingo.PacMan.Core.Game;

/// <summary>
/// Represents a subscription to a Pac-Man event mediator. Call <see cref="Release"/> to
/// unsubscribe from future notifications.
/// </summary>
public sealed class BlPacManEventSubscription
{
    private bool _released;
    private readonly Action _release;

    internal BlPacManEventSubscription(Action release)
    {
        _release = release ?? throw new ArgumentNullException(nameof(release));
    }

    /// <summary>
    /// Stops receiving notifications for the associated subscription.
    /// </summary>
    public void Release()
    {
        if (_released)
            return;

        _released = true;
        _release();
    }
}

/// <summary>
/// Lightweight mediator that mimics Lingo-style broadcast handlers using explicit
/// subscriptions rather than .NET events.
/// </summary>
/// <typeparam name="T">Payload delivered to each subscriber.</typeparam>
internal sealed class BlPacManEventMediator<T>
{
    private readonly List<Action<T>> _handlers = new();

    /// <summary>
    /// Registers a listener for the mediator and returns a subscription object that can
    /// be released to unsubscribe.
    /// </summary>
    public BlPacManEventSubscription Subscribe(Action<T> handler)
    {
        _handlers.Add(handler);

        return new BlPacManEventSubscription(() =>
        {
            _handlers.Remove(handler);
        });
    }

    /// <summary>
    /// Notifies all subscribers with the provided payload.
    /// </summary>
    public void Publish(T value)
    {
        if (_handlers.Count == 0)
            return;

        var snapshot = _handlers.ToArray();
        foreach (var handler in snapshot)
            handler(value);
    }
}
