using Sirenix.OdinInspector;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// A Singleton MonoBehaviour
  /// </summary>
  /// <remarks>
  /// This class is a base class for creating Singletons in Unity.
  /// Instances are lazily created when accessed and are not destroyed
  /// when new scenes are loaded.
  /// </remarks>
  public abstract class Singleton<T> : RootBehaviour where T : Singleton<T> {
    /// <summary>
    /// The single instance of this Singleton.
    /// </summary>
    public static T Instance => Singletons.GetOrCreate(() => {
      // Search for an existing instance
      var instance = FindObjectOfType<T>();
      if (instance != null) {
        return instance;
      }

      // Create a new instance
      var gameObject = new GameObject(typeof(T).Name);
      DontDestroyOnLoad(gameObject);
      return gameObject.AddComponent<T>();
    });

    protected virtual void Awake() {
      // Ensure only one instance exists
      if (!Singletons.TrySet(this as T)) {
        Destroy(gameObject);
      }
    }
  }

  public static class SingletonExtensions {
    /// <summary>
    /// Gets the singleton instance of the specified type.
    /// </summary>
    public static T GetInstance<T>(this T _) where T : Singleton<T> => Singleton<T>.Instance;
  }
}