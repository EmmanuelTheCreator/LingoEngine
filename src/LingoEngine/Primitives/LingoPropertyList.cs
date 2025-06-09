namespace LingoEngine.Primitives
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A property list implementation that mimics Lingo's propList behavior, preserving insertion order.
    /// </summary>
    public class LingoPropertyList<TValue> : LingoPropertyList<LingoSymbol,TValue>
    {

    }
    /// <summary>
    /// A property list implementation that mimics Lingo's propList behavior, preserving insertion order.
    /// </summary>
    public class LingoPropertyList<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dict = new();
        private readonly List<TKey> _keyOrder = new();

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        public TValue this[TKey key]
        {
            get => _dict[key];
            set
            {
                if (!_dict.ContainsKey(key))
                    _keyOrder.Add(key);
                _dict[key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value at the specified 1-based index.
        /// </summary>
        public TValue this[int index]
        {
            get => GetAt(index);
            set => SetAt(index, value);
        }

        /// <summary>
        /// Gets the value at a 1-based index.
        /// </summary>
        public TValue? GetAt(int index)
        {
            if (index < 1)
                throw new IndexOutOfRangeException("LingoPropertyList is 1-based.");
            if (index > _keyOrder.Count)
                return default;
            return _dict[_keyOrder[index - 1]];
        }

        /// <summary>
        /// Sets the value at a 1-based index.
        /// </summary>
        public void SetAt(int index, TValue value)
        {
            if (index < 1 || index > _keyOrder.Count)
                throw new IndexOutOfRangeException("LingoPropertyList is 1-based.");
            _dict[_keyOrder[index - 1]] = value;
        }

        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
        {
            if (!_dict.ContainsKey(key))
                _keyOrder.Add(key);
            _dict.Add(key, value);
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

        /// <inheritdoc/>
        public bool Remove(TKey key)
        {
            if (_dict.Remove(key))
            {
                _keyOrder.Remove(key);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value!);

        /// <inheritdoc/>
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <inheritdoc/>
        public void Clear()
        {
            _dict.Clear();
            _keyOrder.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<TKey, TValue> item) => _dict.ContainsKey(item.Key);

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            throw new NotSupportedException();

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public ICollection<TKey> Keys => _dict.Keys;

        /// <inheritdoc/>
        public ICollection<TValue> Values => _dict.Values;

        /// <inheritdoc/>
        public int Count => _dict.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds a key-value pair at a specific 1-based index.
        /// </summary>
        public void AddAt(int index, TKey key, TValue value)
        {
            if (_dict.ContainsKey(key))
                throw new ArgumentException("Key already exists.");
            if (index < 1 || index > _keyOrder.Count + 1)
                throw new IndexOutOfRangeException("Index must be 1-based and within bounds.");

            _keyOrder.Insert(index - 1, key);
            _dict[key] = value;
        }

        /// <summary>
        /// Deletes an entry at a specific 1-based index.
        /// </summary>
        public void DeleteAt(int index)
        {
            if (index < 1 || index > _keyOrder.Count)
                throw new IndexOutOfRangeException("Index out of range for DeleteAt.");

            var key = _keyOrder[index - 1];
            _keyOrder.RemoveAt(index - 1);
            _dict.Remove(key);
        }

        /// <summary>
        /// Returns the key at a 1-based index.
        /// </summary>
        public TKey GetKeyAt(int index)
        {
            if (index < 1 || index > _keyOrder.Count)
                throw new IndexOutOfRangeException("Index out of range for GetKeyAt.");

            return _keyOrder[index - 1];
        }

        /// <summary>
        /// Finds the 1-based index of a key. Returns 0 if not found.
        /// </summary>
        public int FindIndexOfKey(TKey key)
        {
            int index = _keyOrder.IndexOf(key);
            return index >= 0 ? index + 1 : 0;
        }

        /// <summary>
        /// Finds the 1-based index of a value. Returns 0 if not found.
        /// </summary>
        public int FindPosValue(TValue value)
        {
            var key = _keyOrder.FirstOrDefault(k => EqualityComparer<TValue>.Default.Equals(_dict[k], value));
            if (key is null || !_dict.ContainsKey(key))
                return 0;

            return _keyOrder.IndexOf(key) + 1;
        }

        /// <summary>
        /// Sorts keys in ascending order.
        /// </summary>
        public void SortKeysAsc() => _keyOrder.Sort();

        /// <summary>
        /// Sorts keys in descending order.
        /// </summary>
        public void SortKeysDesc() => _keyOrder.Sort((a, b) => Comparer<TKey>.Default.Compare(b, a));

        /// <summary>
        /// Sorts key order based on their corresponding values in ascending order.
        /// </summary>
        public void SortByValuesAsc() => _keyOrder.Sort((a, b) => Comparer<TValue>.Default.Compare(_dict[a], _dict[b]));

        /// <summary>
        /// Sorts key order based on their corresponding values in descending order.
        /// </summary>
        public void SortByValuesDesc() => _keyOrder.Sort((a, b) => Comparer<TValue>.Default.Compare(_dict[b], _dict[a]));

        /// <summary>
        /// Sorts using a custom key comparer.
        /// </summary>
        public void SortKeys(IComparer<TKey> comparer) => _keyOrder.Sort(comparer);

        /// <summary>
        /// Sorts using a custom value comparer.
        /// </summary>
        public void SortByValues(IComparer<TValue> comparer) => _keyOrder.Sort((a, b) => comparer.Compare(_dict[a], _dict[b]));

        /// <summary>
        /// Duplicates the current property list.
        /// </summary>
        public LingoPropertyList<TKey, TValue> Duplicate()
        {
            var copy = new LingoPropertyList<TKey, TValue>();
            foreach (var key in _keyOrder)
                copy.Add(key, _dict[key]);
            return copy;
        }

        /// <summary>
        /// Returns a list of all keys in order.
        /// </summary>
        public List<TKey> GetPropList() => new List<TKey>(_keyOrder);

        /// <summary>
        /// Sets a property.
        /// </summary>
        public void SetProp(TKey key, TValue value)
        {
            if (!_dict.ContainsKey(key))
                _keyOrder.Add(key);
            _dict[key] = value;
        }

        /// <summary>
        /// Deletes a property.
        /// </summary>
        public void DeleteProp(TKey key)
        {
            if (_dict.ContainsKey(key))
            {
                _dict.Remove(key);
                _keyOrder.Remove(key);
            }
        }

        /// <summary>
        /// Returns the internal dictionary copy.
        /// </summary>
        public Dictionary<TKey, TValue> AsDictionary() => new(_dict);

        /// <summary>
        /// Gets the nth key (1-based).
        /// </summary>
        public TKey GetNthProp(int index) => _keyOrder[index - 1];

        /// <summary>
        /// Gets the nth value (1-based).
        /// </summary>
        public TValue GetNthValue(int index) => _dict[_keyOrder[index - 1]];

        /// <summary>
        /// Gets the first value.
        /// </summary>
        public TValue GetOne() => _dict[_keyOrder[0]];

        /// <summary>
        /// Gets the last value.
        /// </summary>
        public TValue GetLast() => _dict[_keyOrder[^1]];

        /// <summary>
        /// Gets a random key.
        /// </summary>
        public TKey GetAProp()
        {
            int i = Random.Shared.Next(0, _keyOrder.Count);
            return _keyOrder[i];
        }

        /// <summary>
        /// Gets a random value.
        /// </summary>
        public TValue GetAValue() => _dict[GetAProp()];

        /// <summary>
        /// Gets the value at 1-based index.
        /// </summary>
        public TValue GetValueAt(int index) => GetNthValue(index);

        /// <summary>
        /// Sets a random property.
        /// </summary>
        public void SetAProp(TValue value)
        {
            var key = GetAProp();
            _dict[key] = value;
        }

        /// <summary>
        /// Converts values to list.
        /// </summary>
        public List<TValue> ToList() => _keyOrder.Select(key => _dict[key]).ToList();

        /// <summary>
        /// Converts values to list (alias).
        /// </summary>
        public List<TValue> ToValueList() => ToList();

        /// <summary>
        /// Converts keys to list (alias).
        /// </summary>
        public List<TKey> ToPropList() => GetPropList();

        /// <inheritdoc/>
        public override string ToString() => string.Join(", ", _keyOrder.Select(k => $"{k}: {_dict[k]}"));

        /// <summary>
        /// Sorts keys ascending.
        /// </summary>
        public void Sort() => _keyOrder.Sort();

        /// <summary>
        /// Sorts values ascending.
        /// </summary>
        public void SortByValue() => _keyOrder.Sort((a, b) => Comparer<TValue>.Default.Compare(_dict[a], _dict[b]));

        /// <summary>
        /// Repeats an action over all key-value pairs.
        /// </summary>
        public void Repeat(Action<TKey, TValue> action)
        {
            foreach (var key in _keyOrder)
                action(key, _dict[key]);
        }

        /// <summary>
        /// Repeats a callback that can stop iteration early.
        /// </summary>
        public void RepeatWith(Func<TKey, TValue, bool> callback)
        {
            foreach (var key in _keyOrder)
            {
                if (!callback(key, _dict[key]))
                    break;
            }
        }

        /// <summary>
        /// Merges another property list into this one.
        /// </summary>
        public void Merge(LingoPropertyList<TKey, TValue> other)
        {
            foreach (var key in other._keyOrder)
            {
                if (!_dict.ContainsKey(key))
                    _keyOrder.Add(key);
                _dict[key] = other._dict[key];
            }
        }
        public const string IlkName = "propList";
        /// <summary>
        /// Returns Lingo type name.
        /// </summary>
        public LingoSymbol Ilk() => LingoSymbol.New(IlkName);

        /// <summary>
        /// Indicates list is a property list (always returns 1).
        /// </summary>
        public int ListP() => 1;

        /// <summary>
        /// Returns maximum value.
        /// </summary>
        public TValue? Max() => _dict.Values.Max();

        /// <summary>
        /// Returns minimum value.
        /// </summary>
        public TValue? Min() => _dict.Values.Min();

        /// <summary>
        /// Clears all entries.
        /// </summary>
        public void DeleteAll()
        {
            _dict.Clear();
            _keyOrder.Clear();
        }

        /// <summary>
        /// Removes first entry matching value.
        /// </summary>
        public bool DeleteOne(TValue value)
        {
            var key = _keyOrder.FirstOrDefault(k => EqualityComparer<TValue>.Default.Equals(_dict[k], value));
            if (key != null)
                return Remove(key);
            return false;
        }
    }
}
