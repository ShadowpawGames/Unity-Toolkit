using System;

namespace Shadowpaw {
  /// <summary>
  /// A Provider that can provide instances of a given type.
  /// </summary>
  /// <remarks>
  /// Unlike an <see cref="IFactory"/> or an <see cref="ILocator"/>,
  /// a Provider does not have a prescribed method for providing instances.
  /// They may be Provided, created, cached, reused, or otherwise obtained.
  /// The specific behavior of a Provider is determined by the implementation.
  /// </remarks>
  public interface IProvider {
    /// <summary>
    /// Determines if this provider can provide an instance of the given type.
    /// </summary>
    bool CanProvide(Type type);

    /// <summary>
    /// Tries to provide an instance of the given type.
    /// </summary>
    bool TryProvide(Type type, out object instance);

    /// <summary>
    /// Provides an instance of the given type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the provider cannot provide an instance of the given type.
    /// </exception>
    object Provide(Type type) {
      if (TryProvide(type, out var instance)) return instance;
      throw new InvalidOperationException($"Cannot provide an instance of type '{type.FullName}'");
    }
  }

  /// <inheritdoc cref="IProvider"/>
  public interface IProvider<T> : IProvider {
    /// <inheritdoc cref="IProvider.CanProvide(Type)"/>
    bool CanProvide() => true;

    /// <inheritdoc cref="IProvider.TryProvide(Type, out object)"/>
    bool TryProvide(out T instance);

    /// <inheritdoc cref="IProvider.Provide(Type)"/>
    T Provide() {
      if (TryProvide(out var instance)) return instance;
      throw new InvalidOperationException($"Cannot provide an instance of type '{typeof(T).FullName}'");
    }

    #region IProvider (Explicit Implementation)

    bool IProvider.CanProvide(Type type) => type == typeof(T);

    bool IProvider.TryProvide(Type type, out object instance) {
      if (CanProvide(type)) {
        var success = TryProvide(out var result);
        instance = result;
        return success && result != null;
      }

      instance = null;
      return false;
    }

    #endregion
  }

  /// <summary>
  /// A Provider that can provide instances of a given base type and its subtypes.
  /// </summary>
  public interface ISubtypeProvider<TBase> : IProvider<TBase> {
    /// <inheritdoc cref="IProvider{T}.CanProvide"/>
    bool CanProvide<T>() where T : TBase;

    /// <inheritdoc cref="IProvider{T}.TryProvide"/>
    bool TryProvide<T>(out T instance) where T : TBase;

    /// <inheritdoc cref="IProvider{T}.Provide"/>
    T Provide<T>() where T : TBase {
      if (TryProvide<T>(out var instance)) return instance;
      throw new InvalidOperationException($"Cannot provide an instance of type '{typeof(T).FullName}'");
    }

    #region IProvider (Explicit Implementation)

    bool IProvider.CanProvide(Type type) {
      if (!typeof(TBase).IsAssignableFrom(type)) return false;

      // Use reflection to call CanCreate<T>() with the correct type argument
      var method = GetType().GetMethod(nameof(CanProvide)).MakeGenericMethod(type);
      return (bool)method.Invoke(this, null);
    }

    bool IProvider.TryProvide(Type type, out object instance) {
      if (!typeof(TBase).IsAssignableFrom(type)) {
        instance = null;
        return false;
      }

      // Use reflection to call TryCreate<T>() with the correct type argument
      var method = GetType().GetMethod(nameof(TryProvide)).MakeGenericMethod(type);
      var success = (bool)method.Invoke(this, new object[] { null });
      instance = method.GetParameters()[0].DefaultValue;
      return success;
    }

    bool IProvider<TBase>.CanProvide() => CanProvide<TBase>();

    bool IProvider<TBase>.TryProvide(out TBase instance) => TryProvide(out instance);

    #endregion
  }
}