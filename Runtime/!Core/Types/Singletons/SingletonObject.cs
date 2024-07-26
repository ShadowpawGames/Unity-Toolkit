using Sirenix.OdinInspector;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// A Singleton ScriptableObject
  /// </summary>
  /// <remarks>
  /// This is a base class for creating Singleton ScriptableObjects in Unity.
  /// Instances are lazily created or loaded from the Resources folder when accessed.
  /// </remarks>
  public abstract class SingletonObject<T> : RootObject, ISingleton where T : SingletonObject<T> {
    /// <summary>
    /// A Singleton Instance
    /// </summary>
    public static T Instance => Singletons.GetOrCreate(() => {
      // Search for an existing instance
      var resources = Resources.LoadAll<T>("");
      if (resources.Length > 1) {
        Debug.LogWarning($"Multiple instances of Singleton {typeof(T).Name} found in Resources folder.");
      }
      if (resources.Length > 0) return resources[0];

      // Unity gets grumpy about creating generic instances, so we can't do that...
      if (typeof(T).IsGenericType) return null;

      // Create a new non-generic instance
      return CreateInstance<T>();
    });

    protected virtual void Awake() {
      // Warn if multiple instances exist
      if (!Singletons.TrySet(this as T)) {
        Debug.LogWarning($"Multiple instances of SingletonObject {typeof(T).Name} found in project.", this);
      }
    }
  }
}