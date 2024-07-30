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
  public class Registry<TKey, TValue> : IRegistry<TKey, TValue> {
    [SerializeField] private Dictionary<TKey, TValue> _entries = new();

    public IEnumerable<KeyValuePair<TKey, TValue>> Entries => _entries;
    public IEnumerable<TKey> Keys => _entries.Keys;
    public IEnumerable<TValue> Values => _entries.Values;

    public bool IsRegistered(TKey key)
      => _entries.ContainsKey(key) && _entries[key] != null;

    public bool Register(TKey key, TValue value, bool overwrite = true) {
      if (!overwrite && IsRegistered(key)) return false;

      _entries[key] = value;
      return true;
    }

    public void Unregister(TKey key) => _entries.Remove(key);

    public void Clear() => _entries.Clear();

    /// <summary>
    /// Tries to get the value associated with the given key, if it exists and is non-null.
    /// </summary>
    public bool TryGet(TKey key, out TValue value)
      => _entries.TryGetValue(key, out value) && value != null;

    /// <summary>
    /// Gets the value associated with the given key, if it exists and is non-null.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the key/value is not found in the registry.
    /// </exception>
    public TValue Get(TKey key) {
      if (TryGet(key, out TValue value)) return value;
      throw new KeyNotFoundException($"Key '{key}' not found in registry.");
    }

    /// <summary>
    /// Indexer for the registry.
    /// </summary>
    public TValue this[TKey key] {
      get => _entries[key];
      set => _entries[key] = value;
    }
  }
}