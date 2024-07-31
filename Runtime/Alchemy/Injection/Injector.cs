using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Provides static utilities for Dependency Injection. <br />
  /// Dependencies are provided by impemtations of <see cref="InjectionProvider"/>.
  /// </summary>
  /// <remarks>
  /// There is no means of detecting if injection has already been performed on an object. <br />
  /// Thus any object can be injected multiple times, which may lead to unexpected behavior. <br />
  /// It is up to the developer to ensure that injection is only performed once on an object.
  /// </remarks>
  public static class Injector {
    /// <summary>
    /// Binding flags used to search for injectable members.
    /// </summary>
    private const BindingFlags bindingFlags = BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.FlattenHierarchy;

    /// <summary>
    /// The list of all available InjectionProviders.
    /// </summary>
    public static IEnumerable<InjectionProvider> Providers {
      get {
        // If the providers have not been initialized, do so now
        // If not in play mode, assume the list is stale and reinitialize
        if (_providers == null || !Application.isPlaying) {
          // Use reflection to find all InjectionProviders
          _providers = ReflectionUtils
            .GetAllTypes()
            .ConcretionsOf<InjectionProvider>()
            .Select(Activator.CreateInstance)
            .Cast<InjectionProvider>()
            .OrderByDescending(provider => provider.Priority)
            .ToArray();
        }
        return _providers;
      }
    }
    private static InjectionProvider[] _providers;

    #region Provide

    /// <summary>
    /// Provides all instances of the requested type.
    /// </summary>
    public static IEnumerable<object> ProvideAll(Type type, object context = null) {
      return Providers
        .Where(provider => provider.CanProvide(type, context))
        .Select(provider => provider.TryProvide(type, out var result, context) ? result : null)
        .NotNull();
    }

    /// <inheritdoc cref="ProvideAll(Type, object)"/>
    public static IEnumerable<T> ProvideAll<T>(object context = null)
      => ProvideAll(typeof(T), context).Cast<T>();

    /// <summary>
    /// Tries to provide an instance of the requested type. <br />
    /// Uses the first provider that can provide the requested type.
    /// </summary>
    /// <param name="context">
    /// A context object used to determine the scope of injection. <br />
    /// - For creating a new instance, this should be the parent object. <br />
    /// - For an existing instance, this should be the target object. <br />
    /// - This may be null if no context is provided.
    /// </param>
    /// <param name="force">
    /// If true a new instance will be created directly if no provider is found. <br />
    /// This can be a deeply recursive operation, so use with caution.
    /// </param>
    public static bool TryProvide(
      Type type, out object instance, object context = null, bool force = false
    ) {
      // Try to provide an instance using a provider
      var providers = Providers.Where(provider => provider.CanProvide(type, context));
      foreach (var provider in providers) {
        if (provider.TryProvide(type, out instance, context)) return true;
      }

      // If no provider was found, try to create a new instance
      if (force && TryCreate(type, out instance, context, force)) return true;

      instance = null;
      return false;
    }

    /// <inheritdoc cref="TryProvide(Type, out object, object, bool)"/>
    public static bool TryProvide<T>(out T instance, object context = null, bool force = false) {
      if (TryProvide(typeof(T), out var obj, context, force)) {
        instance = (T)obj;
        return instance != null;
      }
      instance = default;
      return false;
    }

    /// <inheritdoc cref="TryProvide(Type, out object, object, bool)"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the requested type cannot be provided.
    /// </exception>
    public static object Provide(Type type, object context = null, bool force = false) {
      if (TryProvide(type, out var instance, context, force)) return instance;
      throw new InvalidOperationException($"Failed to provide an instance of {type}");
    }

    /// <inheritdoc cref="Provide(Type, object, bool)"/>
    public static T Provide<T>(object context = null, bool force = false)
      => (T)Provide(typeof(T), context, force);

    #endregion

    #region Create

    /// <inheritdoc cref="TryProvide(Type, out object, object, bool)"/>
    /// <summary>
    /// Tries to create an instance of the requested type.
    /// </summary>
    public static bool TryCreate(
      Type type, out object instance, object context = null, bool force = false
    ) {
      instance = CreateInstance(type, context, force);
      return TryInject(ref instance, context ?? instance, force);
    }

    /// <inheritdoc cref="TryCreate(Type, out object, object, bool)"/>
    public static bool TryCreate<T>(out T instance, object context = null, bool force = false) {
      if (TryCreate(typeof(T), out var obj, context, force)) {
        instance = (T)obj;
        return instance != null;
      }
      instance = default;
      return false;
    }

    /// <inheritdoc cref="TryCreate(Type, out object, object, bool)"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the requested type cannot be created.
    /// </exception>
    public static object Create(Type type, object context = null, bool force = false) {
      if (TryCreate(type, out var instance, context, force)) return instance;
      throw new InvalidOperationException($"Failed to create an instance of {type}");
    }

    /// <inheritdoc cref="Create(Type, object, bool)"/>
    public static T Create<T>(object context = null, bool force = false)
      => (T)Create(typeof(T), context, force);

    /// <summary>
    /// Creates a new instance of the requested type.
    /// </summary>
    private static object CreateInstance(Type type, object context, bool force) {
      // Non-concrete types cannot be created
      if (!type.IsConcrete()) return null;

      // Create a new ScriptableObject...
      if (typeof(ScriptableObject).IsAssignableFrom(type)) {
        return ScriptableObject.CreateInstance(type);
      }

      // Create a new GameObject...
      if (typeof(GameObject).IsAssignableFrom(type)) {
        var gameObject = new GameObject(type.Name);
        if (context is GameObject parent) {
          // Set the parent of the new GameObject
          gameObject.transform.SetParent(parent.transform);
        }
        return gameObject;
      }

      // Create a new Component...
      if (typeof(Component).IsAssignableFrom(type)) {
        // Get the GameObject to attach the Component to
        GameObject gameObject;
        if (context is GameObject parent) {
          gameObject = parent;
        } else if (context is Component component) {
          gameObject = component.gameObject;
        } else {
          gameObject = new GameObject(type.Name);
        }

        // Prefer existing components over new ones
        // This safeguards against Transform and other default components
        var newComponent = gameObject.GetComponent(type);
        if (newComponent == null) {
          newComponent = gameObject.AddComponent(type);
        }
        return newComponent;
      }

      // Create a new Object via constructor...
      var constructor = GetInjectionConstructor(type);
      var parameters = constructor.GetParameters();
      var arguments = new object[parameters.Length];
      for (var i = 0; i < parameters.Length; i++) {
        if (TryProvide(parameters[i].ParameterType, out var argument, context, force)) {
          // If the provider was successful, use the provided instance
          arguments[i] = argument;
        } else if (parameters[i].IsOptional) {
          // If the parameter is optional, use the default value
          arguments[i] = parameters[i].DefaultValue;
        } else {
          Debug.LogError($"Failed to provide {parameters[i].ParameterType} to constructor of {type.Name}"); return null;
        }
      }

      return Activator.CreateInstance(type, arguments);
    }

    #endregion

    #region Inject

    /// <summary>
    /// Injects dependencies into all components in the given scene.
    /// </summary>
    /// <remarks>
    /// There is no means of detecting if injection has already been performed on an object. <br />
    /// Thus any object can be injected multiple times, which may lead to unexpected behavior. <br />
    /// It is up to the developer to ensure that injection is only performed once on an object.
    /// </remarks>
    /// <param name="scene">
    /// The scene to inject dependencies into.
    /// </param>
    /// <param name="force">
    /// If true a new instance will be created directly if no provider is found. <br />
    /// This can be a deeply recursive operation, so use with caution.
    /// </param>
    /// <param name="includeInactive">
    /// If true, inactive objects will be included in the injection process.
    /// </param>
    /// <returns>
    /// True if all dependencies were successfully injected, false otherwise.
    /// </returns>
    public static bool InjectAll(Scene scene, bool force = false, bool includeInactive = true) {
      var components = scene
        .GetRootGameObjects()
        .SelectMany(gameObject => gameObject.GetComponentsInChildren<Component>(includeInactive))
        .ToArray();

      var success = true;
      for (int i = 0; i < components.Length; i++) {
        if (!TryInject(ref components[i], components[i].gameObject, force)) {
          Debug.LogWarning($"Failed to inject dependencies into {components[i].name}", components[i]);
          success = false;
        }
      }
      return success;
    }

    /// <inheritdoc cref="TryInject{T}(ref T, object, bool)"/>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the dependencies cannot be injected into the target object.
    /// </exception>
    public static void Inject<T>(ref T target, object context = null, bool force = false) {
      if (TryInject(ref target, context, force)) return;
      throw new InvalidOperationException($"Failed to inject dependencies into {target.GetType()}");
    }

    /// <inheritdoc cref="TryProvide(Type, out object, object, bool)"/>
    /// <summary>
    /// Tries to inject dependencies into the target object.
    /// </summary>
    public static bool TryInject<T>(ref T target, object context = null, bool force = false) {
      return target != null
        && TryInjectFields(ref target, context, force)
        && TryInjectProperties(ref target, context, force)
        && TryInjectMethods(ref target, context, force);
    }

    private static bool TryInjectFields<T>(ref T target, object context, bool force) {
      var fields = target.GetType().GetFields(bindingFlags);
      foreach (var field in fields) {
        var attr = field.GetCustomAttribute<InjectAttribute>();
        if (attr == null) continue;

        if (!TryProvide(field.FieldType, out var instance, context, force) && attr.Required) {
          Debug.LogError($"Failed to inject {field.FieldType} into {target.GetType()}.{field.Name}");
          return false;
        }

        field.SetValue(target, instance);
      }
      return true;
    }

    private static bool TryInjectProperties<T>(ref T target, object context, bool force) {
      var props = target.GetType().GetProperties(bindingFlags);
      foreach (var prop in props) {
        var attr = prop.GetCustomAttribute<InjectAttribute>();
        if (attr == null) continue;

        if (!prop.CanWrite) {
          Debug.LogError($"Failed to inject {target.GetType()}.{prop.Name} because it is read-only");
          return false;
        }

        if (!TryProvide(prop.PropertyType, out var instance, context, force) && attr.Required) {
          Debug.LogError($"Failed to inject {prop.PropertyType} into {target.GetType()}.{prop.Name}");
          return false;
        }

        prop.SetValue(target, instance);
      }
      return true;
    }

    private static bool TryInjectMethods<T>(ref T target, object context, bool force) {
      var methods = target.GetType().GetMethods(bindingFlags);
      foreach (var method in methods) {
        var attr = method.GetCustomAttribute<InjectAttribute>();
        if (attr == null) continue;

        // Flag: Indicates if the method can be safely invoked
        var safeToInvoke = true;

        var parameters = method.GetParameters();
        var arguments = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++) {
          if (TryProvide(parameters[i].ParameterType, out var instance, context, force)) {
            // If the provider was successful, use the provided instance
            arguments[i] = instance;
          } else if (parameters[i].IsOptional) {
            // If the parameter is optional, use the default value
            arguments[i] = parameters[i].DefaultValue;
          } else {
            safeToInvoke = false;
            if (!attr.Required) continue;

            Debug.LogError($"Failed to inject {parameters[i].ParameterType} {parameters[i].Name} into {target.GetType()}.{method.Name}");
          }
        }

        // A non-optional method failed to be invoked
        if (!safeToInvoke && attr.Required) return false;
        if (safeToInvoke) method.Invoke(target, arguments);
      }
      return true;
    }

    #endregion

    #region Reflection

    /// <summary>
    /// Gets the constructor that should be used for injection. <br />
    /// - If a constructor has an [Inject] attribute, it will be used. <br />
    /// - If multiple constructors have an [Inject] attribute, an error will be logged. <br />
    /// - If no constructor has an [Inject] attribute, the first constructor found will be used. <br />
    /// - If no constructors are found, the default constructor will be used.
    /// </summary>
    public static ConstructorInfo GetInjectionConstructor(this Type type) {
      var allConstrcutors = type.GetConstructors(bindingFlags);
      var injectConstructors = allConstrcutors
        .Where(ctor => ctor.GetCustomAttribute<InjectAttribute>() != null);

      Debug.Assert(
        injectConstructors.Count() <= 1,
        $"Cannot construct {type.Name}. Expected exactly one constructor with an [Inject] attribute."
      );

      Debug.Assert(
        injectConstructors.Any() || allConstrcutors.Length <= 1,
        $"Cannot construct {type.Name}. Multiple constructors found, but none have an [Inject] attribute."
      );

      // Return the first constructor with an [Inject] attribute, or the first constructor found
      return injectConstructors.FirstOrDefault() ?? allConstrcutors.FirstOrDefault();
    }

    /// <summary>
    /// Gets all Types that the given Type depends on for injection.
    /// </summary>
    /// <param name="includeOptional">
    /// If true, optional dependencies will be included in the result.
    /// </param>
    public static IEnumerable<Type> GetInjectionDependencies(this Type type, bool includeOptional) {
      HashSet<Type> dependancies = new();

      // Add Constructor dependencies...
      var constructor = GetInjectionConstructor(type);
      if (constructor != null) {
        var parameters = constructor.GetParameters();
        foreach (var parameter in parameters) {
          if (includeOptional || !parameter.IsOptional) {
            dependancies.Add(parameter.ParameterType);
          }
        }
      }

      // Add Field dependencies...
      var fields = type.GetFields(bindingFlags);
      foreach (var field in fields) {
        var attr = field.GetCustomAttribute<InjectAttribute>();
        if (attr == null) continue;

        if (includeOptional || attr.Required) {
          dependancies.Add(field.FieldType);
        }
      }

      // Add Property dependencies...
      var properties = type.GetProperties(bindingFlags);
      foreach (var property in properties) {
        var attr = property.GetCustomAttribute<InjectAttribute>();
        if (attr == null) continue;

        if (includeOptional || attr.Required) {
          dependancies.Add(property.PropertyType);
        }
      }

      // Add Method dependencies...
      var methods = type.GetMethods(bindingFlags);
      foreach (var method in methods) {
        var attr = method.GetCustomAttribute<InjectAttribute>();
        if (attr == null) continue;

        // Skip optional methods if not required
        if (!includeOptional && !attr.Required) continue;

        var parameters = method.GetParameters();
        foreach (var parameter in parameters) {
          if (includeOptional || !parameter.IsOptional) {
            dependancies.Add(parameter.ParameterType);
          }
        }
      }

      return dependancies;
    }

    #endregion
  }
}