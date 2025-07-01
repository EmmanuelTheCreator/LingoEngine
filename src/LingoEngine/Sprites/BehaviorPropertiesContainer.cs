using System.Collections;
using LingoEngine.Primitives;
using LingoEngine.Tools;
using static LingoEngine.Sprites.BehaviorPropertiesContainer;

namespace LingoEngine.Sprites;
/// <summary>
/// Container for behavior properties set by the user. Holds the values
/// and, optionally, the description list returned by
/// <c>getPropertyDescriptionList</c>.
/// </summary>
public class BehaviorPropertiesContainer : IEnumerable<LingoPropertyItem>
{
    /// <summary>A single property entry.</summary>
    public class LingoPropertyItem
    {
        public LingoSymbol Key { get; set; } = LingoSymbol.Empty;
        public object? Value { get; set; }
    }

    private readonly List<LingoPropertyItem> _items = new();

    public void Apply(BehaviorPropertyDescriptionList definitions)
    {
        foreach (var definition in definitions)
        {
            var newValue = this[definition.Key];
            if (newValue != null)
                definition.Value.ApplyValue(newValue);
        }
    }

    /// <summary>Gets or sets a property by key.</summary>
    public object? this[LingoSymbol key]
    {
        get => _items.FirstOrDefault(i => i.Key.Equals(key))?.Value;
        set
        {
            var idx = _items.FindIndex(i => i.Key.Equals(key));
            if (idx >= 0)
                _items[idx].Value = value;
            else
                _items.Add(new LingoPropertyItem { Key = key, Value = value });
        }
    }
    public BehaviorPropertiesContainer Add(LingoSymbol key, object? value)
    {
        _items.Add(new LingoPropertyItem { Key = key, Value = value });
        return this;
    }
    public BehaviorPropertiesContainer Remove(LingoSymbol key)
    {
        var idx = _items.FindIndex(i => i.Key.Equals(key));
        if (idx >= 0)
            _items.RemoveAt(idx);
        return this;
    }

    /// <summary>Enumerates all property keys.</summary>
    public IEnumerable<LingoSymbol> Keys => _items.Select(i => i.Key);

    /// <summary>The number of stored properties.</summary>
    public int Count => _items.Count;

    public IEnumerator<LingoPropertyItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
