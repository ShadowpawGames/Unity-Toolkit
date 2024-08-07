using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// Registry for items using their Type as their key.
  /// </summary>
  [Serializable]
  public class TypeRegistry<TBase> : IRegistry<Type, TBase> {
    [SerializeField] private Dictionary<Type, TBase> _entries = new();
    public IEnumerable<KeyValuePair<Type, TBase>> Entries => _entries;
    public IEnumerable<Type> Keys => _entries.Keys;
    public IEnumerable<TBase> Values => _entries.Values;

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
    public bool TryGet(Type type, out TBase value, bool matchSubtypes = false) {
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
      if (TryGet(typeof(T), out TBase obj, matchSubtypes) && obj is T castObject) {
        value = castObject;
        return value != null;
      }
      value = default;
      return false;
    }

    /// <summary>
    /// Gets all entries assignable to a specified type.
    /// </summary>
    public IEnumerable GetAll(Type type) {
      return Entries
        .Where(entry => type.IsAssignableFrom(entry.Key))
        .Select(entry => entry.Value);
    }

    /// <inheritdoc cref="GetAll(Type)"/>
    public IEnumerable<T> GetAll<T>() => GetAll(typeof(T)).OfType<T>();

    public bool IsRegistered(Type type) => IsRegistered(type, false);

    /// <inheritdoc cref="IsRegistered(Type)"/>
    /// <param name="matchSubtypes">
    /// If true, the method will return the first entry of the given type or a Subtype.
    /// </param>
    public bool IsRegistered(Type type, bool matchSubtypes) {
      var key = GetKey(type, matchSubtypes);
      return _entries.ContainsKey(key) && _entries[key] != null;
    }

    /// <inheritdoc cref="IsRegistered(Type, bool)"/>
    public bool IsRegistered<T>(bool matchSubtypes = false) where T : TBase
      => IsRegistered(typeof(T), matchSubtypes);

    public bool Register(Type type, TBase value, bool overwrite = true) {
      // Confirm TBase is a base type of the given type
      if (!typeof(TBase).IsAssignableFrom(type)) return false;

      if (TryGet(type, out TBase existing)) {
        if (existing.Equals(value)) return true;
        if (!overwrite) return false;
      }

      _entries[type] = value;
      return true;
    }

    /// <inheritdoc cref="Register(Type, TBase, bool)"/>
    public bool Register<T>(T value, bool overwrite = true) where T : TBase
      => Register(typeof(T), value, overwrite);

    public void Unregister(Type key) => _entries.Remove(key);

    public void Unregister(TBase value) {
      foreach (var (key, val) in _entries) {
        if (value.Equals(val)) _entries.Remove(key);
      }
    }

    /// <inheritdoc cref="Unregister(Type)"/>
    public void Unregister<T>() where T : TBase
      => Unregister(typeof(T));

    public void Clear() => _entries.Clear();

  }

  /// <inheritdoc cref="TypeRegistry{TBase}"/>
  [Serializable]
  public class TypeRegistry : TypeRegistry<object> { }
}