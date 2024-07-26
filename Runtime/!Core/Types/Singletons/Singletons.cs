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
    /// Tries to create a singleton for a specified type.
    /// </summary>
    /// <remarks>
    /// This is a bit of a hack to get around the fact that there is 
    /// no way to add static properties to an interface, and we can't
    /// call static methods or properties on subclasses. Great job C#.
    /// </remarks>
    private static bool TryCreate<T>(out T singleton) where T : class {
      // Prevent recursive calls to TryCreate.
      if (isCreating) {
        singleton = null;
        return false;
      }
      isCreating = true;

      Type singletonType = typeof(T);

      try {
        // Check for Singleton<T> types...
        var monoType = typeof(Singleton<>).MakeGenericType(typeof(T));
        if (typeof(T).IsSubclassOf(monoType)) {
          singletonType = monoType;
        }
      } catch (ArgumentException) { }

      try {
        // Check for SingletonObject<T> types...
        var objectType = typeof(SingletonObject<>).MakeGenericType(typeof(T));
        if (typeof(T).IsSubclassOf(objectType)) {
          singletonType = objectType;
        }
      } catch (ArgumentException) { }

      // Check for a public static Instance property on the type.
      // This allows for essentially any class to be used as a Singleton. (Duck Typing)
      var flags = BindingFlags.Public | BindingFlags.Static;
      var instanceProperty = singletonType?.GetProperty("Instance", flags);
      singleton = instanceProperty?.GetValue(null) as T;

      // Clear the flag to allow for future calls to TryCreate.
      isCreating = false;
      return singleton != null;
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
    public static T GetOrCreate<T>(Func<T> factory) where T : class {
      if (TryGet(out T singleton)) return singleton;
      return Set(factory());
    }

    /// <summary>
    /// Tries to get the singleton for a specified type.
    /// </summary>
    /// <returns></returns>
    public static bool TryGet<T>(out T singleton) where T : class {
      if (registry.TryGet(out singleton)) return true;
      return TryCreate(out singleton);
    }

    /// <summary>
    /// Gets the singleton for a specified type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a singleton of the specified type does not exist.
    /// </exception>
    public static T Get<T>() where T : class {
      if (TryGet(out T singleton)) return singleton;
      throw new InvalidOperationException($"Singleton of type {typeof(T)} does not exist.");
    }

    /// <summary>
    /// Tries to set the singleton for a specified type.
    /// </summary>
    /// <returns>
    /// True if the singleton was successfully set, false otherwise.
    /// </returns>
    public static bool TrySet<T>(T singleton) where T : class {
      // If the singleton cannot be registered, return false.
      if (!registry.Register(singleton, false)) return false;

      // Ensure the singleton is not destroyed when a new scene is loaded.
      if (singleton is Component component) {
        var gameObject = component.gameObject;
        if (gameObject.scene.buildIndex != -1) {
          // Clear the parent to avoid weird parenting issues.
          gameObject.transform.SetParent(null);
          UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }
      }

      return true;
    }

    /// <summary>
    /// Sets the singleton for a specified type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a singleton of the same type already exists.
    /// </exception>
    public static T Set<T>(T singleton) where T : class {
      if (TrySet(singleton)) return singleton;
      throw new InvalidOperationException($"Singleton of type {typeof(T)} already exists.");
    }
  }
}
