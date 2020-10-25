using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EmbeddedResourceBrowser
{
    /// <summary>A <see cref="IReadOnlyList{T}"/> containing elements that can be accessed by their name.</summary>
    /// <typeparam name="T">The type of element that is contained in the collection,</typeparam>
    /// <remarks>
    /// All items in the collection are sorted by their name and searched using case-insensitive comparison (<see cref="StringComparer.OrdinalIgnoreCase"/>).
    /// </remarks>
    public class NamedReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyDictionary<string, T> _items;

        internal NamedReadOnlyList(IEnumerable<T> items, Func<T, string> nameSelector)
        {
            var sortedItems = new SortedList<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
                sortedItems.Add(nameSelector(item), item);
            _items = sortedItems;
        }

        /// <summary>Gets the element with the provided <paramref name="name"/></summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c></exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown with an element with the given <paramref name="name"/> does not exist.</exception>
        public T this[string name]
            => _items[name];

        /// <summary>Checks whether an item with the given <paramref name="name"/> exists. </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>Returns <c>true</c> if an item with the given <paramref name="name"/> exists; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c></exception>
        public bool ContainsKey(string name)
            => _items.ContainsKey(name);

        /// <summary> Gets the item having the specified <paramref name="name"/>. </summary>
        /// <param name="name">The name of the item to retrieve.</param>
        /// <param name="value">The retrieved item, if found; otherwise <c>default(T)</c> is returned.</param>
        /// <returns>If an item with the provided <paramref name="name"/> exists, returns <c>true</c>; Otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c></exception>
        public bool TryGetValue(string name, out T value)
            => _items.TryGetValue(name, out value);

        /// <summary>Gets the element at the provided <paramref name="index"/></summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the provided <paramref name="index"/> is less than <c>0</c> or greater than or equal to the total number of items in the collection.
        /// </exception>
        public T this[int index]
        => _items.ElementAt(index).Value;

        /// <summary>Gets the number of items in the collection.</summary>
        public int Count
            => _items.Count;

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
            => _items.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}