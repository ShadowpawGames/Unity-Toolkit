using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shadowpaw {

  /// <summary>
  /// Registry for a Set of items of a given type.
  /// </summary>
  [Serializable]
  public class Registry<T> : IRegistry<T> {
    [SerializeField] private HashSet<T> _entries = new();
    public IEnumerable<T> Entries => _entries;

    public bool IsRegistered(T item) => _entries.Contains(item);

    public bool Register(T item, bool overwrite = true) {
      if (!overwrite && IsRegistered(item)) return false;
      return _entries.Add(item);
    }

    public void Unregister(T item) => _entries.Remove(item);

    public void Clear() => _entries.Clear();
  }

  /// <summary>
  /// Registry for a Dictionary of items of a given type.
  /// </summary>
  [Serializable]
  public class Registry<TKey, TValue> : IRegistry<KeyValuePair<TKey, TValue>> {
    [SerializeField] private Dictionary<TKey, TValue> _entries = new();
    public IEnumerable<KeyValuePair<TKey, TValue>> Entries => _entries;
    public IEnumerable<TKey> Keys => _entries.Keys;
    public IEnumerable<TValue> Values => _entries.Values;

    /// <summary>
    /// Tries to get the value associated with the given key, if it exists and is non-null.
    /// </summary>
    public bool TryGet(TKey key, out TValue value)
      => _entries.TryGetValue(key, out value) && value != null;

    /// <summary>
    /// Returns true if the given key is in the registry and non-null.
    /// </summary>
    public bool IsRegistered(TKey key)
      => _entries.ContainsKey(key) && _entries[key] != null;

    /// <summary>
    /// Registers the given key-value pair in the registry.
    /// </summary>
    /// <param name="overwrite">
    /// If true, the value will be added even if it overwrites an existing value.
    /// </param>
    public bool Register(TKey key, TValue value, bool overwrite = true) {
      if (!overwrite && IsRegistered(key)) return false;

      _entries[key] = value;
      return true;
    }

    /// <summary>
    /// Removes the given key from the registry.
    /// </summary>
    public void Unregister(TKey key) => _entries.Remove(key);

    public void Clear() => _entries.Clear();

    #region IRegistry<KeyValuePair<TKey, TValue>> (Explicit Implementation)

    /// <summary>
    /// Returns true if the given key-value pair is registered in the registry.
    /// </summary>
    /// <remarks>
    /// The value is only considered registered if it matches the value associated with the key.
    /// </remarks>
    /// <deprecated>Use <see cref="IsRegistered(TKey)"/> instead.</deprecated> 
    bool IRegistry<KeyValuePair<TKey, TValue>>.IsRegistered(KeyValuePair<TKey, TValue> item)
      => _entries.ContainsKey(item.Key) && _entries[item.Key].Equals(item.Value);

    /// <summary>
    /// Registers the given key-value pair in the registry.
    /// </summary>
    bool IRegistry<KeyValuePair<TKey, TValue>>.Register(KeyValuePair<TKey, TValue> item, bool overwrite)
      => Register(item.Key, item.Value, overwrite);

    /// <summary>
    /// Removes the given key-value pair from the registry.
    /// </summary>
    /// <remarks>
    /// The value is only removed if it matches the value associated with the key.
    /// </remarks>
    /// <deprecated>Use <see cref="Unregister(TKey)"/> instead.</deprecated>
    void IRegistry<KeyValuePair<TKey, TValue>>.Unregister(KeyValuePair<TKey, TValue> item) {
      if (_entries.ContainsKey(item.Key) && _entries[item.Key].Equals(item.Value))
        _entries.Remove(item.Key);
    }

    #endregion
  }
}