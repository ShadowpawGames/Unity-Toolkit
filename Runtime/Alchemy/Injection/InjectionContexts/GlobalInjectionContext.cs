using UnityEngine;

namespace Shadowpaw {
  public class GlobalInjectionContext : InjectionContext, ISingleton {
    public static GlobalInjectionContext Instance => Singletons.GetOrCreate(() => {
      // Search for an existing instance
      var instance = FindFirstObjectByType<GlobalInjectionContext>();
      if (instance != null) return instance;

      // Create a new instance
      var gameObject = new GameObject("InjectionContext<Global>");
      DontDestroyOnLoad(gameObject);
      return gameObject.AddComponent<GlobalInjectionContext>();
    });

    private void Awake() {
      // Ensure only one instance exists
      if (!Singletons.TrySet(this)) {
        Debug.LogWarning($"GlobalInjectionContext already exists. Destroying new instance.", this);
        Destroy(gameObject);
      }
    }
  }
}
