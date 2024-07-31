using System;
using System.Linq;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Base class for providing injection dependancies.
  /// </summary>
  public abstract class InjectionProvider : IProvider {
    /// <summary>
    /// The priority of this provider. <br />
    /// Providers with higher priority will be checked first.
    /// </summary>
    public virtual int Priority { get; init; } = 0;

    /// <inheritdoc cref="IProvider.CanProvide"/>
    /// <param name="context">
    /// A context object used to determine the scope of injection. <br />
    /// - For creating a new instance, this should be the parent object. <br />
    /// - For an existing instance, this should be the target object. <br />
    /// - This may be null if no context is provided.
    /// </param>
    public abstract bool CanProvide(Type type, object context = null);

    /// <inheritdoc cref="IProvider.TryProvide"/>
    /// <param name="context">
    /// A context object used to determine the scope of injection. <br />
    /// - For creating a new instance, this should be the parent object. <br />
    /// - For an existing instance, this should be the target object. <br />
    /// - This may be null if no context is provided.
    /// </param>
    public abstract bool TryProvide(Type type, out object instance, object context = null);

    #region IProvider (Explicit Implementation)

    bool IProvider.CanProvide(Type type) => CanProvide(type, null);
    bool IProvider.TryProvide(Type type, out object instance) => TryProvide(type, out instance, null);

    #endregion
  }

  /// <inheritdoc cref="InjectionProvider"/>
  public abstract class InjectionProvider<TBase> : InjectionProvider, ISubtypeProvider<TBase> {
    /// <inheritdoc cref="ISubtypeProvider{T}.CanProvide"/>
    /// <param name="context">
    /// A context object used to determine the scope of injection. <br />
    /// - For creating a new instance, this should be the parent object. <br />
    /// - For an existing instance, this should be the target object. <br />
    /// - This may be null if no context is provided.
    /// </param>
    public abstract bool CanProvide<T>(object context = null) where T : TBase;

    /// <inheritdoc cref="ISubtypeProvider{T}.TryProvide"/>
    /// <param name="context">
    /// A context object used to determine the scope of injection. <br />
    /// - For creating a new instance, this should be the parent object. <br />
    /// - For an existing instance, this should be the target object. <br />
    /// - This may be null if no context is provided.
    /// </param>
    public abstract bool TryProvide<T>(out T instance, object context = null) where T : TBase;

    #region InjectionProvider (Abstract Methods)

    public override bool CanProvide(Type type, object context = null) {
      if (!typeof(TBase).IsAssignableFrom(type)) return false;

      // Use reflection to call CanProvide<T> with the correct type argument
      // This is annoying, but it avoids an ambiguous method call
      var methodInfo = GetType().GetMethods()
        .Where(m => m.Name == nameof(CanProvide))
        .Where(m => m.IsGenericMethod)
        .Where(m => m.GetParameters().Length == 1)
        .FirstOrDefault();
      var genericMethod = methodInfo.MakeGenericMethod(type);
      return (bool)genericMethod.Invoke(this, new object[] { context });
    }

    public override bool TryProvide(Type type, out object instance, object context = null) {
      if (!typeof(TBase).IsAssignableFrom(type)) {
        instance = null;
        return false;
      }

      // Use reflection to call TryProvide<T> with the correct type argument
      // This is annoying, but it avoids an ambiguous method call
      var methodInfo = GetType().GetMethods()
        .Where(methodInfo => methodInfo.Name == nameof(TryProvide))
        .Where(methodInfo => methodInfo.ContainsGenericParameters)
        .Where(methodInfo => methodInfo.GetParameters().Length == 2)
        .FirstOrDefault();
      var genericMethod = methodInfo.MakeGenericMethod(type);
      var parameters = new object[] { context, null };
      var success = (bool)genericMethod.Invoke(this, parameters);

      instance = parameters[1];
      return success;
    }

    #endregion

    #region ISubtypeProvider<TBase> (Explicit Implementation)

    bool ISubtypeProvider<TBase>.CanProvide<T>() => CanProvide<T>(null);
    bool ISubtypeProvider<TBase>.TryProvide<T>(out T instance) => TryProvide(out instance, null);

    #endregion
  }
}