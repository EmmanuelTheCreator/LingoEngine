using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LingoEngine.Primitives
{
    /// <summary>
    /// Represents a Lingo-style list with 1-based indexing and ordered values.
    /// </summary>
    public class LingoList<T> : IList<T>
    {
        private readonly List<T> _items = new();

        /// <summary>
        /// Gets or sets the element at the 1-based index.
        /// </summary>
        public T this[int index]
        {
            get => GetAt(index);
            set => SetAt(index, value);
        }

        /// <summary>
        /// Gets the number of items in the list.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Gets a value indicating whether the list is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an item to the end of the list.
        /// </summary>
        public void Add(T item) => _items.Add(item);

        /// <summary>
        /// Adds an item at the specified 1-based index.
        /// </summary>
        public void AddAt(int index, T item) => _items.Insert(index - 1, item);

        /// <summary>
        /// Removes the item at the specified 1-based index.
        /// </summary>
        public void DeleteAt(int index) => _items.RemoveAt(index - 1);

        /// <summary>
        /// Gets the item at the specified 1-based index.
        /// </summary>
        public T GetAt(int index) => _items[index - 1];

        /// <summary>
        /// Sets the item at the specified 1-based index.
        /// </summary>
        public void SetAt(int index, T value) => _items[index - 1] = value;

        /// <summary>
        /// Removes the first occurrence of the specified item.
        /// </summary>
        public bool Remove(T item) => _items.Remove(item);

        /// <summary>
        /// Removes all items from the list.
        /// </summary>
        public void Clear() => _items.Clear();

        /// <summary>
        /// Determines whether the list contains the specified item.
        /// </summary>
        public bool Contains(T item) => _items.Contains(item);

        /// <summary>
        /// Copies the elements to an array starting at the specified array index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        /// <summary>
        /// Gets the zero-based index of the item (for compatibility).
        /// </summary>
        public int IndexOf(T item) => _items.IndexOf(item);

        /// <summary>
        /// Inserts an item at the specified zero-based index (not Lingo style).
        /// </summary>
        public void Insert(int index, T item) => _items.Insert(index, item);

        /// <summary>
        /// Removes the item at the specified zero-based index.
        /// </summary>
        public void RemoveAt(int index) => _items.RemoveAt(index);

        /// <summary>
        /// Returns an enumerator for the list.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Creates a shallow copy of the list.
        /// </summary>
        public LingoList<T> Duplicate() => new LingoList<T>(_items);

        /// <summary>
        /// Creates a new list from a collection.
        /// </summary>
        public LingoList(IEnumerable<T> collection) => _items = new List<T>(collection);

        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public LingoList() { }

        /// <summary>
        /// Gets the first item in the list.
        /// </summary>
        public T GetOne() => _items[0];

        /// <summary>
        /// Gets the last item in the list.
        /// </summary>
        public T GetLast() => _items[^1];

        /// <summary>
        /// Gets a random item in the list.
        /// </summary>
        public T GetAValue() => _items[Random.Shared.Next(0, _items.Count)];

        /// <summary>
        /// Returns the Lingo type identifier.
        /// </summary>
        public LingoSymbol Ilk() => LingoSymbol.New(IlkName);
        public const string IlkName = "list";

        /// <summary>
        /// Returns 1 to indicate this is a list.
        /// </summary>
        public int ListP() => 1;

        /// <summary>
        /// Returns a list copy of the values.
        /// </summary>
        public List<T> ToList() => new(_items);

        /// <summary>
        /// Deletes the first occurrence of the specified value.
        /// </summary>
        public bool DeleteOne(T value) => Remove(value);

        /// <summary>
        /// Deletes all values from the list.
        /// </summary>
        public void DeleteAll() => Clear();

        /// <summary>
        /// Executes the action on each element.
        /// </summary>
        public void Repeat(Action<T> action)
        {
            foreach (var item in _items)
                action(item);
        }

        /// <summary>
        /// Executes the function on each element until false is returned.
        /// </summary>
        public void RepeatWith(Func<T, bool> func)
        {
            foreach (var item in _items)
                if (!func(item)) break;
        }

        /// <summary>
        /// Sorts the list using the default comparer.
        /// </summary>
        public void Sort() => _items.Sort();

        /// <summary>
        /// Sorts the list using the given comparer.
        /// </summary>
        public void Sort(IComparer<T> comparer) => _items.Sort(comparer);

        /// <summary>
        /// Returns the maximum value.
        /// </summary>
        public T? Max() => _items.Max();

        /// <summary>
        /// Returns the minimum value.
        /// </summary>
        public T? Min() => _items.Min();
    }
}
