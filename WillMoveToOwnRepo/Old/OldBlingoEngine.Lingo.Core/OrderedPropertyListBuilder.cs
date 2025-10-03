using System.Collections.Generic;

namespace OldBlingoEngine.Lingo.Core;

internal sealed class OrderedPropertyListBuilder<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, int> _indices;
    private readonly List<TValue> _items = new();

    public OrderedPropertyListBuilder(IEqualityComparer<TKey>? comparer = null)
    {
        _indices = new Dictionary<TKey, int>(comparer ?? EqualityComparer<TKey>.Default);
    }

    public void AddOrUpdate(TKey key, TValue value)
    {
        if (_indices.TryGetValue(key, out var index))
        {
            _items[index] = value;
        }
        else
        {
            _indices[key] = _items.Count;
            _items.Add(value);
        }
    }

    public IReadOnlyList<TValue> Items => _items;

    public List<TValue> ToList() => new(_items);
}
