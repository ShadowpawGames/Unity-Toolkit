using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shadowpaw {
  [DisallowMultipleComponent]
  public class InjectionContext : RootBehaviour, IRegistry<Object> {
    [field: Tooltip("These objects will override injections of their given type.")]
    [field: SerializeField] private HashSet<Object> context = new();
    public IEnumerable<Object> Entries => context;

    #region IRegistry<Object>

    public bool IsRegistered(Object item) => context.Contains(item);
    public bool Register(Object item, bool overwrite = true) => context.Add(item);
    public void Unregister(Object item) => context.Remove(item);
    public void Clear() => context.Clear();

    #endregion

    private bool TryGetComponent(Type type, out object instance) {
      // If the type is not a Component, return false
      if (!typeof(Component).IsAssignableFrom(type)) {
        instance = null;
        return false;
      }

      // Search for the component in GameObjects
      foreach (var obj in context) {
        if (obj is GameObject gameObject) {
          var component = gameObject.GetComponent(type);
          if (component != null) {
            instance = component;
            return true;
          }
        }
      }

      instance = null;
      return false;
    }

    /// <summary>
    /// Tries to get an instance of the given type. <br />
    /// If the type is a Component, GameObjects will be searched for the that component.
    /// </summary>
    /// <param name="matchSubtypes">
    /// Whether or not to include subtypes of the given type.
    /// </param>
    public bool TryGet(Type type, out object instance, bool matchSubtypes = false) {
      // Search for a directl match first
      foreach (var obj in context) {
        if (type.IsInstanceOfType(obj)) {
          instance = obj;
          return true;
        }
      }

      // If no direct match is found, search for subtypes
      if (matchSubtypes) {
        foreach (var obj in context) {
          if (matchSubtypes && type.IsAssignableFrom(obj.GetType())) {
            instance = obj;
            return true;
          }
        }
      }

      // Search for Components
      if (TryGetComponent(type, out instance)) {
        return true;
      }

      // if it still hasn't been found, check if a parent context can find it
      var parent = this.GetParent();
      if (parent != null && parent.TryGet(type, out instance, matchSubtypes)) {
        return true;
      }

      // If the instance is not found, return false
      instance = null;
      return false;
    }

    /// <inheritdoc cref="TryGet(Type, out Object, bool)"/>
    public bool TryGet<T>(out T instance, bool matchSubtypes = false) {
      if (TryGet(typeof(T), out var obj, matchSubtypes)) {
        if (obj is T castObj) {
          instance = castObj;
          return instance != null;
        }
      }
      instance = default;
      return false;
    }
  }
}
