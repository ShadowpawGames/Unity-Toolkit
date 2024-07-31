using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// A marker interface for Singletons.
  /// </summary>
  /// <remarks>
  /// This saves the trouble of using reflection on a generic base class.
  /// </remarks>
  interface ISingleton { }

  /// <summary>
  /// A Singleton MonoBehaviour
  /// </summary>
  /// <remarks>
  /// This class is a base class for creating Singletons in Unity.
  /// Instances are lazily created when accessed and are not destroyed
  /// when new scenes are loaded.
  /// </remarks>
  public abstract class Singleton<T> : RootBehaviour, ISingleton where T : Singleton<T> {
    /// <summary>
    /// The single instance of this Singleton.
    /// </summary>
    public static T Instance => Singletons.GetOrCreate(() => {
      // Search for an existing instance
      var instance = FindObjectOfType<T>();
      if (instance != null) return instance;

      // Create a new instance
      var gameObject = new GameObject(typeof(T).Name);
      DontDestroyOnLoad(gameObject);
      return gameObject.AddComponent<T>();
    });

    protected virtual void Awake() {
      // Ensure only one instance exists
      if (!Singletons.TrySet(this as T)) {
        Debug.LogWarning($"Singleton of type {typeof(T)} already exists. Destroying new instance.", this);
        Destroy(gameObject);
      }
    }
  }
}