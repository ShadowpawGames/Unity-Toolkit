using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shadowpaw {
  public static class InjectionContexts {
    /// <summary>
    /// The global injection context.
    /// </summary>
    public static GlobalInjectionContext Global => GlobalInjectionContext.Instance;

    /// <summary>
    /// Map of scene indexes to their respective injection contexts.
    /// </summary>
    private static readonly Dictionary<int, InjectionContext> sceneContexts = new();

    /// <summary>
    /// Gets the parent context of a given context. <br />
    /// - Cascades upwards through Local, Scene, and Global contexts. <br />
    /// - Returns null if the given context is the global context.
    /// </summary>
    public static InjectionContext GetParent(this InjectionContext context) {
      if (context is GlobalInjectionContext) return null;

      // Check if the context has a parent game object (not root)
      var parent = context.transform.parent;
      if (parent != null) return For(parent.gameObject);

      // Check if the context is a scene context
      var sceneContext = ForScene(context.gameObject.scene, false);
      if (sceneContext != null && sceneContext != context) {
        return sceneContext;
      }

      // Default to the global context
      return Global;
    }

    /// <summary>
    /// Gets the injection context for the given scene. <br />
    /// If the scene does not have a context and createIfMissing is true, a new one will be created. <br />
    /// Otherwise the Global context will be returned instead.
    /// </summary>
    /// <param name="createIfMissing">
    /// Whether or not to create a new Context if one does not exist.
    /// </param>
    public static InjectionContext ForScene(Scene scene, bool createIfMissing = true) {
      if (scene == null) return Global;

      // Check if the scene has an existing context
      if (sceneContexts.TryGetValue(scene.handle, out var existing)) {
        if (existing != null) return existing;
      }

      // Search root objects for an existing Context
      foreach (var root in scene.GetRootGameObjects()) {
        if (root.TryGetComponent<InjectionContext>(out var context)) {
          sceneContexts[scene.handle] = context;
          return context;
        }
      }

      // If no context was found, create a new one
      if (createIfMissing) {
        var gameObject = new GameObject($"InjectionContext<Scene>");
        SceneManager.MoveGameObjectToScene(gameObject, scene);
        return gameObject.AddComponent<InjectionContext>();
      }

      // Otherwise, return the global context
      return Global;
    }

    /// <inheritdoc cref="ForScene(Scene, bool)"/>
    public static InjectionContext ForScene(GameObject gameObject, bool createIfMissing = true)
      => gameObject == null ? Global : ForScene(gameObject.scene, createIfMissing);

    /// <inheritdoc cref="ForScene(Scene, bool)"/>
    public static InjectionContext ForScene(Component component, bool createIfMissing = true)
      => component == null ? Global : ForScene(component.gameObject, createIfMissing);

    /// <summary>
    /// Gets the injection context for a given object.
    /// </summary>
    public static InjectionContext For(GameObject gameObject) {
      if (gameObject == null) return Global;

      // Check if the gameObject has an existing context
      if (gameObject.TryGetComponent<InjectionContext>(out var context)) {
        return context;
      }

      // Check if the gameObject has parents with contexts
      var parent = gameObject.transform.parent;
      if (parent != null) return For(parent.gameObject);

      // If the gameObject is a root object, get the scene context
      return ForScene(gameObject.scene, false);
    }

    /// <inheritdoc cref="For(GameObject)"/>
    public static InjectionContext For(Component component)
      => component == null ? Global : For(component.gameObject);

    #region Loose Types

    /// <inheritdoc cref="ForScene(Scene, bool)"/>
    public static InjectionContext ForScene(object source) {
      if (source is null) return Global;
      return source switch {
        GameObject gameObject => ForScene(gameObject),
        Component component => ForScene(component),
        Scene scene => ForScene(scene),
        _ => Global
      };
    }

    /// <inheritdoc cref="For(GameObject)"/>
    public static InjectionContext For(object source) {
      if (source is null) return Global;
      return source switch {
        GameObject gameObject => For(gameObject),
        Component component => For(component),
        Scene scene => ForScene(scene),
        _ => Global
      };
    }

    #endregion
  }
}
