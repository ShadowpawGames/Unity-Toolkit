using System;
using System.Reflection;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// Provides a static registry for managing Singletons.
  /// </summary>
  /// <remarks>
  /// This allows for Singleton creation and retrieval 
  /// without the need to hook into the Unity lifecycle.
  /// </remarks>
  public static class Singletons {
    private static readonly TypeRegistry registry = new();
    private static bool isCreating = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnBeforeSceneLoad() {
      // Load any Singletons from the Resources folder
      var resources = Resources.LoadAll("");
      foreach (var resource in resources) {
        if (resource is ISingleton) {
          if (!registry.Register(resource.GetType(), resource, false)) {
            Debug.LogWarning($"Singleton of type {resource.GetType()} already exists.", resource);
          }
        }
      }
    }

    /// <summary>
    /// Checks if a singleton exists for a specified type.
    /// </summary>
    /// <remarks>
    /// This method will NOT create a new Singleton if one does not exist.
    /// </remarks>
    public static bool Contains(Type type, bool matchSubtypes = true) {
      return registry.IsRegistered(type, matchSubtypes);
    }

    /// <inheritdoc cref="Contains(Type, bool)"/>
    public static bool Contains<T>(bool matchSubtypes = true) where T : class
      => Contains(typeof(T), matchSubtypes);

    /// <summary>
    /// Tries to create a singleton for a specified type.
    /// </summary>
    /// <remarks>
    /// This is a bit of a hack to get around the fact that there is 
    /// no way to add static properties to an interface, and we can't
    /// call static methods or properties on subclasses. Great job C#.
    /// </remarks>
    private static bool TryCreate(Type type, out object instance) {
      // Prevent recursive calls to TryCreate.
      if (isCreating) {
        instance = null;
        return false;
      }
      isCreating = true;

      try {
        // Check for Singleton<T> types...
        var monoType = typeof(Singleton<>).MakeGenericType(type);
        if (type.IsSubclassOf(monoType)) {
          type = monoType;
        }
      } catch (ArgumentException) { }

      try {
        // Check for SingletonObject<T> types...
        var objectType = typeof(SingletonObject<>).MakeGenericType(type);
        if (type.IsSubclassOf(objectType)) {
          type = objectType;
        }
      } catch (ArgumentException) { }

      // Check for a public static Instance property on the type.
      // This allows for essentially any class to be used as a Singleton. (Duck Typing)
      var flags = BindingFlags.Public | BindingFlags.Static;
      var instanceProperty = type?.GetProperty("Instance", flags);
      instance = instanceProperty?.GetValue(null);

      // Clear the flag to allow for future calls to TryCreate.
      isCreating = false;
      return instance != null;
    }

    /// <inheritdoc cref="TryCreate(Type, out object)"/>
    private static bool TryCreate<T>(out T instance) where T : class {
      if (TryCreate(typeof(T), out object obj)) {
        instance = obj as T;
        return instance != null;
      }
      instance = null;
      return false;
    }

    /// <summary>
    /// Gets an existing singleton or creates a new one if it does not exist.
    /// </summary>
    /// <param name="factory">
    /// The factory method to create a new singleton.
    /// </param>
    /// <returns>
    /// The singleton instance for the specified type.
    /// </returns>
    public static object GetOrCreate(Type type, Func<object> factory, bool matchSubtypes = true) {
      if (TryGet(type, out object instance, matchSubtypes)) return instance;
      return Set(factory());
    }

    /// <inheritdoc cref="GetOrCreate(Type, Func{object}, bool)"/>
    public static T GetOrCreate<T>(Func<T> factory, bool matchSubtypes = true) where T : class
      => GetOrCreate(typeof(T), () => factory(), matchSubtypes) as T;

    /// <summary>
    /// Tries to get the singleton for a specified type.
    /// </summary>
    /// <remarks>
    /// This method will attempt to create a new Singleton if one does not exist.
    /// </remarks>
    public static bool TryGet(Type type, out object instance, bool matchSubtypes = true) {
      if (registry.TryGet(type, out instance, matchSubtypes)) return true;
      return TryCreate(out instance);
    }

    /// <inheritdoc cref="TryGet(Type, out object, bool)"/>
    public static bool TryGet<T>(out T instance, bool matchSubtypes = true) where T : class {
      if (TryGet(typeof(T), out object obj, matchSubtypes)) {
        instance = obj as T;
        return instance != null;
      }
      instance = null;
      return false;
    }

    /// <summary>
    /// Gets the singleton for a specified type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a singleton of the specified type does not exist.
    /// </exception>
    public static object Get(Type type) {
      if (TryGet(type, out object instance)) return instance;
      throw new InvalidOperationException($"Singleton of type {type} does not exist.");
    }

    /// <inheritdoc cref="Get(Type)"/>
    public static T Get<T>() where T : class
      => Get(typeof(T)) as T;

    /// <summary>
    /// Tries to set the singleton for a specified type.
    /// </summary>
    /// <returns>
    /// True if the singleton was successfully set, false otherwise.
    /// </returns>
    public static bool TrySet(Type type, object instance) {
      // If the singleton cannot be registered, return false.
      if (!registry.Register(type, instance, false)) return false;

      // Ensure the singleton is not destroyed when a new scene is loaded.
      if (instance is Component component) {
        var gameObject = component.gameObject;
        if (gameObject.scene.buildIndex != -1) {
          // Clear the parent to avoid weird parenting issues.
          gameObject.transform.SetParent(null);
          UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }
      }

      return true;
    }

    /// <inheritdoc cref="TrySet(Type, object)"/>
    public static bool TrySet<T>(T instance) where T : class
      => TrySet(typeof(T), instance);

    /// <summary>
    /// Sets the singleton for a specified type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a singleton of the same type already exists.
    /// </exception>
    public static object Set(Type type, object instance) {
      if (TrySet(type, instance)) return instance;
      throw new InvalidOperationException($"Singleton of type {type} already exists.");
    }

    /// <inheritdoc cref="Set(Type, object)"/>
    public static T Set<T>(T singleton) where T : class
      => Set(typeof(T), singleton) as T;
  }
}
