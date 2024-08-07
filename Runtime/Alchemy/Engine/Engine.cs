using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Shadowpaw.Alchemy {
  public static class Engine {
    public static GameObject RootObject => EngineEvents.Instance.gameObject;
    public static EngineEvents Events => EngineEvents.Instance;
    public static EngineConfig Config => EngineConfig.Instance;

    #region Getters

    /// <inheritdoc cref="Injector.TryProvide(Type, out object, object, bool)" />
    public static bool TryGet(Type type, out object instance, bool force = false)
      => Injector.TryProvide(type, out instance, RootObject, force);

    /// <inheritdoc cref="Injector.TryProvide{T}(out T, object, bool)" />
    public static bool TryGet<T>(out T instance, bool force = false)
      => Injector.TryProvide(out instance, RootObject, force);

    /// <inheritdoc cref="Injector.TryProvide(Type, out object, object, bool)" />
    public static object Get(Type type)
      => Injector.Provide(type, RootObject);

    /// <inheritdoc cref="Injector.TryProvide{T}(out T, object, bool)" />
    public static T Get<T>()
      => Injector.Provide<T>(RootObject);

    /// <summary>
    /// Gets an existing instance of the requested type, or creates a new one.
    /// </summary>
    public static object GetOrCreate(Type type, out object instance) {
      if (Injector.TryProvide(type, out instance, RootObject, force: true)) {
        if (instance is IService service) {
          Services.Register(type, service);
        }
      }
      return instance;
    }

    /// <summary>
    /// Gets an existing instance of the requested type, or creates a new one.
    /// </summary>
    public static T GetOrCreate<T>() {
      if (Injector.TryProvide(out T instance, RootObject, force: true)) {
        if (instance is IService service) {
          Services.Register(typeof(T), service);
        }
      }
      return instance;
    }

    #endregion

    #region Initialization

    private static TaskCompletionSource<bool> initTCS;
    public static bool IsInitializing => initTCS is not null && !initTCS.Task.IsCompleted;
    public static bool IsInitialized => initTCS is not null && initTCS.Task.IsCompleted;

    private static readonly List<Func<Task>> preInitTasks = new();
    private static readonly List<Func<Task>> postInitTasks = new();

    /// <summary>
    /// Adds a task to be executed before Services are initialized.
    /// </summary>
    /// <remarks>
    /// Services will be registered but not initialized at this point.
    /// </remarks>
    public static void AddPreInitTask(Func<Task> task) {
      if (initTCS is null) {
        preInitTasks.Add(task);
        return;
      }

      // If the engine is already initializing, execute the task immediately after.
      Debug.LogWarning("Attempting to add a pre-init task after Engine initialization has started.");
      initTCS.Task.Wait();
      task();
    }

    /// <inheritdoc cref="AddPreInitTask(Func{Task})" />
    public static void AddPreInitTask(Action task) {
      AddPreInitTask(() => {
        task();
        return Task.CompletedTask;
      });
    }

    /// <summary>
    /// Adds a task to be executed after Services are initialized.
    /// </summary>
    public static void AddPostInitTask(Func<Task> task) {
      if (initTCS is null) {
        postInitTasks.Add(task);
        return;
      }

      // If the engine is already initializing, execute the task immediately after.
      Debug.LogWarning("Attempting to add a post-init task after Engine initialization has started.");
      initTCS.Task.Wait();
      task();
    }

    /// <inheritdoc cref="AddPostInitTask(Func{Task})" />
    public static void AddPostInitTask(Action task) {
      AddPostInitTask(() => {
        task();
        return Task.CompletedTask;
      });
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Automatically initialize the Engine, according to the EngineConfig
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static async void AutoInitialize() {
      if (!Config.AutoInitialize) return;
      await Initialize();
    }

    /// <summary>
    /// Initializes the Alchemy Engine
    /// </summary>
    public static async Task Initialize() {
      Debug.Assert(
        Application.isPlaying,
        "Alchemy Engine cannot be initialized outside Play Mode."
      );

      // Prevent multiple initializations
      if (IsInitialized) return;
      if (IsInitializing) {
        await initTCS.Task;
        return;
      } else initTCS = new();

      // Cleanup when the application quits
      Application.quitting += Destroy;

      // Update the RootObject according to the Config
      RootObject.name = "Alchemy<Engine>";
      if (Config.OverrideObjectLayer) {
        RootObject.layer = Config.ObjectLayer;
      }
      if (Config.SceneIndependent) {
        UnityEngine.Object.DontDestroyOnLoad(RootObject);
      }

      // Keep track of the initialization process
      var progress = 0f;

      // Notify Listeners
      Events.OnEngineStart?.Invoke();
      Events.OnInitStarted?.Invoke();
      Events.OnInitProgress?.Invoke(0);

      // Initialize Services enabled in the EngineConfig
      InitConfigServices();

      // Run Pre-Initialization Tasks... (0.00f to 0.25f Range)
      for (int i = 0; i < preInitTasks.Count; i++) {
        // Stop if the Engine was destroyed
        if (!IsInitializing) return;
        await preInitTasks[i]();

        progress = (i + 1f) / preInitTasks.Count;
        Events.OnInitProgress?.Invoke(0.25f * progress);
      }

      // Initialize Services...(0.25f to 0.75f Range)
      var services = Services.GetAll().ToArray();
      for (int i = 0; i < services.Length; i++) {
        // Stop if the Engine was destroyed
        if (!IsInitializing) return;
        await services[i].InitializeService();

        progress = (i + 1f) / services.Length;
        Events.OnInitProgress?.Invoke(0.25f + 0.5f * progress);
      }

      // Run Post-Initialization Tasks... (0.75f to 1.00f Range)
      for (int i = 0; i < postInitTasks.Count; i++) {
        // Stop if the Engine was destroyed
        if (!IsInitializing) return;
        await postInitTasks[i]();

        progress = (i + 1f) / postInitTasks.Count;
        Events.OnInitProgress?.Invoke(0.75f + 0.25f * progress);
      }

      // Notify Listeners
      Events.OnInitProgress?.Invoke(1);
      Events.OnInitComplete?.Invoke();

      // Initialization Complete!
      initTCS.TrySetResult(true);
    }

    /// <summary>
    /// Ensures any services enabled in the EngineConfig are built and registered.
    /// </summary>
    private static void InitConfigServices() {
      var serviceTypes = Config.ServiceList.EnabledServiceTypes
        .OrderByDescending(type => type.GetCustomAttribute<ServiceInfoAttribute>()?.Priority ?? 0)
        .OrderByTopology(
          type => type.GetInjectionDependencies(),
          (a, b) => a.IsAssignableFrom(b)
        );

      foreach (var type in serviceTypes) {
        // Create a child GameObject for the service
        var serviceName = type.GetCustomAttribute<ServiceInfoAttribute>()?.DisplayName ?? type.Name;
        var gameObject = new GameObject(serviceName);
        gameObject.transform.SetParent(RootObject.transform);
        gameObject.layer = RootObject.layer;

        // Use Injector to provide the service instance
        if (Injector.TryProvide(type, out var obj, gameObject, force: true)) {
          if (obj is IService service) Services.Register(type, service);
        } else {
          throw new InvalidOperationException($"Failed to create service of type {type.Name}.");
        }
      }
    }

    /// <summary>
    /// Resets all Services in use by the Engine (excluding the types provided)
    /// </summary>
    /// <param name="excludedTypes">
    /// Services of these types will NOT be reset
    /// </param>
    public static void Reset(params Type[] excludedTypes) {
      // Cannot reset services that are not yet initialized
      if (!IsInitialized) return;

      var services = Services.GetAll().ToArray();
      foreach (var service in services) {
        if (!excludedTypes.Any(type => type.IsInstanceOfType(service))) continue;

        service?.ResetService();
      }
    }

    /// <summary>
    /// Destroys all Services in use by the Engine
    /// </summary>
    public static void Destroy() {
      // Cannot destroy services that are not yet initialized
      if (initTCS is null) return;
      initTCS = null;

      // Clear Application Listener
      Application.quitting -= Destroy;

      // Destroy and unregister all Services
      var services = Services.GetAll().ToArray();
      foreach (var service in services) {
        service?.DestroyService();
      }
      Services.Clear();

      // Destroy all children >:3
      foreach (Transform child in RootObject.transform) {
        if (child == null) continue;
        UnityEngine.Object.Destroy(child.gameObject);
      }

      // Notify Listener
      Events.OnEngineDestroy?.Invoke();
    }

    #endregion
  }
}