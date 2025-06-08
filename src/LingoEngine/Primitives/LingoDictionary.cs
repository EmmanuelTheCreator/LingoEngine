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
    }

}
