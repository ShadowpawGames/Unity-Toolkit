using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// Registry for items using their Type as their key.
  /// </summary>
  [Serializable]
  public class TypeRegistry : IRegistry<KeyValuePair<Type, object>> {
    [SerializeField] private Dictionary<Type, object> _entries = new();
    public IEnumerable<KeyValuePair<Type, object>> Entries => _entries;
    public IEnumerable<Type> Types => _entries.Keys;
    public IEnumerable<object> Values => _entries.Values;

    /// <summary>
    /// Gets the appropriate key for the given Type.
    /// If an exact match is not found, the first entry of a subtype is returned.
    /// </summary>
    private Type GetKey(Type type, bool matchSubtypes) {
      // If we don't care about subtypes, or we have an exact match, return the key as-is
      if (!matchSubtypes || _entries.ContainsKey(type)) return type;

      // Find the first entry of the given type or a subtype, or return the original type
      return Entries.FirstOrDefault(entry => type.IsAssignableFrom(entry.Key)).Key ?? type;
    }

    /// <summary>
    /// Tries to get the value associated with the given key, if it exists and is non-null.
    /// </summary>
    /// <param name="matchSubtypes">
    /// If true, the method will return the first entry of the given type or a subtype.
    /// </param>
    public bool TryGet(Type type, out object value, bool matchSubtypes = false) {
      var key = GetKey(type, matchSubtypes);
      return _entries.TryGetValue(key, out value) && value != null;
    }

    /// <summary>
    /// Tries to get the value associated with the given key,
    /// if it exists, is of the right Type, and is non-null.
    /// </summary>
    /// <param name="matchSubtypes">
    /// If true, the method will return the first entry of the given type or a subtype.
    /// </param>
    public bool TryGet<T>(out T value, bool matchSubtypes = false) {
      if (TryGet(typeof(T), out object obj, matchSubtypes) && obj is T castObject) {
        value = castObject;
        return value != null;
      }
      value = default;
      return false;
    }

    /// <summary>
    /// Returns true if the given Type is in the registry and non-null.
    /// </summary>
    /// <param name="matchSubtypes">
    /// If true, the method will return the first entry of the given type or a subtype.
    /// </param>
    public bool IsRegistered(Type type, bool matchSubtypes = false) {
      var key = GetKey(type, matchSubtypes);
      return _entries.ContainsKey(key) && _entries[key] != null;
    }

    /// <inheritdoc cref="IsRegistered(Type, bool)"/>
    public bool IsRegistered<T>(bool matchSubtypes = false) => IsRegistered(typeof(T), matchSubtypes);

    /// <summary>
    /// Registers the given key-value pair in the registry.
    /// </summary>
    /// <param name="overwrite">
    /// If true, the value will be added even if it overwrites an existing value.
    /// </param>
    public bool Register(Type type, object value, bool overwrite = true) {
      if (!overwrite && IsRegistered(type)) return false;

      _entries[type] = value;
      return true;
    }

    /// <inheritdoc cref="Register(Type, object, bool)"/>
    public bool Register<T>(T value, bool overwrite = true)
      => Register(typeof(T), value, overwrite);

    /// <summary>
    /// Removes the given key from the registry.
    /// </summary>
    public void Unregister(Type type) => _entries.Remove(type);

    /// <inheritdoc cref="Unregister(Type)"/>
    public void Unregister<T>() => Unregister(typeof(T));

    public void Clear() => _entries.Clear();

    #region IRegistry<KeyValuePair<TKey, TValue>> (Explicit Implementation)

    /// <summary>
    /// Returns true if the given key-value pair is registered in the registry.
    /// </summary>
    /// <remarks>
    /// The value is only considered registered if it matches the value associated with the key.
    /// </remarks>
    /// <deprecated>Use <see cref="IsRegistered(TKey)"/> instead.</deprecated> 
    bool IRegistry<KeyValuePair<Type, object>>.IsRegistered(KeyValuePair<Type, object> item)
      => _entries.ContainsKey(item.Key) && _entries[item.Key].Equals(item.Value);

    /// <summary>
    /// Registers the given key-value pair in the registry.
    /// </summary>
    bool IRegistry<KeyValuePair<Type, object>>.Register(KeyValuePair<Type, object> item, bool overwrite)
      => Register(item.Key, item.Value, overwrite);

    /// <summary>
    /// Removes the given key-value pair from the registry.
    /// </summary>
    /// <remarks>
    /// The value is only removed if it matches the value associated with the key.
    /// </remarks>
    /// <deprecated>Use <see cref="Unregister(TKey)"/> instead.</deprecated>
    void IRegistry<KeyValuePair<Type, object>>.Unregister(KeyValuePair<Type, object> item) {
      if (_entries.ContainsKey(item.Key) && _entries[item.Key].Equals(item.Value))
        _entries.Remove(item.Key);
    }

    #endregion
  }
}