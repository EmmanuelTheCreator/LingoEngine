namespace LingoEngine.Primitives
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class LingoDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dict = new();
        private readonly List<TKey> _keyOrder = new();

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

        public TValue this[int index]
        {
            get => GetAt(index);
            set => SetAt(index, value);
        }

        public TValue GetAt(int index)
        {
            if (index < 1 || index > _keyOrder.Count)
                throw new IndexOutOfRangeException("LingoDictionary is 1-based.");
            return _dict[_keyOrder[index - 1]];
        }

        public void SetAt(int index, TValue value)
        {
            if (index < 1 || index > _keyOrder.Count)
                throw new IndexOutOfRangeException("LingoDictionary is 1-based.");
            _dict[_keyOrder[index - 1]] = value;
        }

        public void Add(TKey key, TValue value)
        {
            if (!_dict.ContainsKey(key))
                _keyOrder.Add(key);
            _dict.Add(key, value);
        }

        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (_dict.Remove(key))
            {
                _keyOrder.Remove(key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value!);

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            _dict.Clear();
            _keyOrder.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => _dict.ContainsKey(item.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            throw new NotSupportedException();

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ICollection<TKey> Keys => _dict.Keys;
        public ICollection<TValue> Values => _dict.Values;
        public int Count => _dict.Count;
        public bool IsReadOnly => false;

        public void AddAt(int index, TKey key, TValue value)
        {
            if (_dict.ContainsKey(key))
                throw new ArgumentException("Key already exists.");
            if (index < 1 || index > _keyOrder.Count + 1)
                throw new IndexOutOfRangeException("Index must be 1-based and within bounds.");

            _keyOrder.Insert(index - 1, key);
            _dict[key] = value;
        }

        public void DeleteAt(int index)
        {
            if (index < 1 || index > _keyOrder.Count)
                throw new IndexOutOfRangeException("Index out of range for DeleteAt.");

            var key = _keyOrder[index - 1];
            _keyOrder.RemoveAt(index - 1);
            _dict.Remove(key);
        }

        public TKey GetKeyAt(int index)
        {
            if (index < 1 || index > _keyOrder.Count)
                throw new IndexOutOfRangeException("Index out of range for GetKeyAt.");

            return _keyOrder[index - 1];
        }

        public int FindIndexOfKey(TKey key)
        {
            int index = _keyOrder.IndexOf(key);
            return index >= 0 ? index + 1 : 0; // return 0 if not found, 1-based if found
        }

        public void SortKeysAsc() => _keyOrder.Sort();

        public void SortKeysDesc() => _keyOrder.Sort((a, b) => Comparer<TKey>.Default.Compare(b, a));

        public void SortByValuesAsc() => _keyOrder.Sort((a, b) => Comparer<TValue>.Default.Compare(_dict[a], _dict[b]));

        public void SortByValuesDesc() => _keyOrder.Sort((a, b) => Comparer<TValue>.Default.Compare(_dict[b], _dict[a]));

        public void SortKeys(IComparer<TKey> comparer) => _keyOrder.Sort(comparer);

        public void SortByValues(IComparer<TValue> comparer) => _keyOrder.Sort((a, b) => comparer.Compare(_dict[a], _dict[b]));


        public LingoDictionary<TKey, TValue> Duplicate()
        {
            var copy = new LingoDictionary<TKey, TValue>();
            foreach (var key in _keyOrder)
                copy.Add(key, _dict[key]);
            return copy;

        }

        public List<TKey> GetPropList() => new List<TKey>(_keyOrder);
        public void SetProp(TKey key, TValue value)
        {
            if (!_dict.ContainsKey(key))
                _keyOrder.Add(key);
            _dict[key] = value;
        }
        public void DeleteProp(TKey key)
        {
            if (_dict.ContainsKey(key))
            {
                _dict.Remove(key);
                _keyOrder.Remove(key);
            }
        }
        public Dictionary<TKey, TValue> AsDictionary() => new(_dict);

        public TKey GetNthProp(int index) => _keyOrder[index - 1];
        public TValue GetNthValue(int index) => _dict[_keyOrder[index - 1]];

        public TValue GetOne() => _dict[_keyOrder[0]];
        public TValue GetLast() => _dict[_keyOrder[^1]];
        public TKey GetAProp()
        {
            int i = Random.Shared.Next(0, _keyOrder.Count);
            return _keyOrder[i];
        }

        public TValue GetAValue() => _dict[GetAProp()];
        public TValue GetValueAt(int index) => GetNthValue(index);

        public void SetAProp(TValue value)
        {
            var key = GetAProp();
            _dict[key] = value;
        }
        public List<TValue> ToList() => _keyOrder.Select(key => _dict[key]).ToList();

        public List<TValue> ToValueList() => ToList();
        public List<TKey> ToPropList() => GetPropList();
        public override string ToString() => string.Join(", ", _keyOrder.Select(k => $"{k}: {_dict[k]}"));
        public void Sort() => _keyOrder.Sort(); // Requires TKey : IComparable
        public void SortByValue() => _keyOrder.Sort((a, b) => Comparer<TValue>.Default.Compare(_dict[a], _dict[b]));

        public void Repeat(Action<TKey, TValue> action)
        {
            foreach (var key in _keyOrder)
                action(key, _dict[key]);
        }
        public void RepeatWith(Func<TKey, TValue, bool> callback)
        {
            foreach (var key in _keyOrder)
            {
                if (!callback(key, _dict[key]))
                    break;
            }
        }
        public void Merge(LingoDictionary<TKey, TValue> other)
        {
            foreach (var key in other._keyOrder)
            {
                if (!_dict.ContainsKey(key))
                    _keyOrder.Add(key);
                _dict[key] = other._dict[key];
            }
        }


    }

}
