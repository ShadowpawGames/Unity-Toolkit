using System.Collections;
using System.Collections.Generic;

namespace Shadowpaw {
  /// <summary>
  /// A registry of items of a given type.
  /// </summary>
  /// <remarks>
  /// Items in the registry can be registered, unregistered, and enumerated.
  /// May be implemented as a list, set, dictionary, or other collection type.
  /// </remarks>
  public interface IRegistry<T> : IEnumerable<T> {
    /// <summary>
    /// The contents of the registry.
    /// </summary>
    IEnumerable<T> Entries { get; }

    /// <summary>
    /// Returns true if the given item is in the registry.
    /// </summary>
    bool IsRegistered(T item);

    /// <summary>
    /// Adds the given item to the registry.
    /// </summary>
    /// <param name="overwrite">
    /// If true, the item will be added even if it overwrites an existing item.
    /// </param>
    /// <returns>
    /// True if the item was successfully registered, false if it was not.
    /// </returns>
    bool Register(T item, bool overwrite = true);

    /// <summary>
    /// Removes the given item from the registry.
    /// </summary>
    void Unregister(T item);

    /// <summary>
    /// Clears all entries from the registry.
    /// </summary>
    void Clear();

    #region IEnumerable<T> (Explicit Implementation)

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => Entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();

    #endregion
  }

  /// <summary>
  /// A registry of items of a given type, using a key-value pair.
  /// </summary>
  public interface IRegistry<TKey, TValue> : IRegistry<KeyValuePair<TKey, TValue>> {
    /// <summary>
    /// The keys in use by the registry.
    /// </summary>
    IEnumerable<TKey> Keys { get; }

    /// <summary>
    /// The values contained in the registry.
    /// </summary>
    IEnumerable<TValue> Values { get; }

    /// <summary>
    /// Returns true if the given key is in the registry.
    /// </summary>
    bool IsRegistered(TKey key);

    /// <summary>
    /// Adds the given item to the registry.
    /// </summary>
    /// <param name="overwrite">
    /// If true, the item will be added even if it overwrites an existing item.
    /// </param>
    /// <returns>
    /// True if the item was successfully registered, false if it was not.
    /// </returns>
    public bool Register(TKey key, TValue value, bool overwrite = true);

    /// <summary>
    /// Removes the given item from the registry.
    /// </summary>
    public void Unregister(TKey key);

    #region IRegistry<KeyValuePair<TKey, TValue>> (Explicit Implementation)

    bool IRegistry<KeyValuePair<TKey, TValue>>.IsRegistered(KeyValuePair<TKey, TValue> item)
      => IsRegistered(item.Key);

    bool IRegistry<KeyValuePair<TKey, TValue>>.Register(KeyValuePair<TKey, TValue> item, bool overwrite)
      => Register(item.Key, item.Value, overwrite);

    void IRegistry<KeyValuePair<TKey, TValue>>.Unregister(KeyValuePair<TKey, TValue> item)
      => Unregister(item.Key);

    #endregion
  }
}