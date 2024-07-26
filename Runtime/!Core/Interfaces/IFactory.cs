using System;

namespace Shadowpaw {
  /// <summary>
  /// A Factory that can create instances of a given type.
  /// </summary>
  public interface IFactory : IProvider {
    /// <summary>
    /// Determines if this factory can create an instance of the given type.
    /// </summary>
    bool CanCreate(Type type);

    /// <summary>
    /// Tries to create an instance of the given type.
    /// </summary>
    bool TryCreate(Type type, out object instance);

    /// <summary>
    /// Creates an instance of the given type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the factory cannot create an instance of the given type.
    /// </exception>
    object Create(Type type) {
      if (TryCreate(type, out var instance)) return instance;
      throw new InvalidOperationException($"Cannot create an instance of type '{type.FullName}'");
    }

    #region IProvider (Explicit Implementation)

    bool IProvider.CanProvide(Type type) => CanCreate(type);
    bool IProvider.TryProvide(Type type, out object instance) => TryCreate(type, out instance);

    #endregion
  }

  /// <inheritdoc cref="IFactory"/>
  public interface IFactory<T> : IFactory, IProvider<T> {
    /// <inheritdoc cref="IFactory.CanCreate(Type)"/>
    bool CanCreate() => true;

    /// <inheritdoc cref="IFactory.TryCreate(Type, out object)"/>
    bool TryCreate(out T instance);

    /// <inheritdoc cref="IFactory.Create(Type)"/>
    T Create() {
      if (TryCreate(out var instance)) return instance;
      throw new InvalidOperationException($"Cannot create an instance of type '{typeof(T).FullName}'");
    }

    #region IFactory (Explicit Implementation)

    bool IFactory.CanCreate(Type type) => type == typeof(T);
    bool IFactory.TryCreate(Type type, out object instance) {
      if (CanCreate(type)) {
        var success = TryCreate(out var result);
        instance = result;
        return success && result != null;
      }

      instance = null;
      return false;
    }

    #endregion

    #region IProvider<T> (Explicit Implementation)

    bool IProvider<T>.CanProvide() => CanCreate();
    bool IProvider<T>.TryProvide(out T instance) => TryCreate(out instance);

    #endregion
  }

  /// <summary>
  /// A Factory that can create instances of a given base type and its subtypes.
  /// </summary>
  public interface ISubtypeFactory<TBase> : IFactory<TBase> {
    bool CanCreate<T>() where T : TBase;
    bool TryCreate<T>(out T instance) where T : TBase;
    T Create<T>() where T : TBase;

    #region IFactory (Explicit Implementation)

    bool IFactory.CanCreate(Type type) {
      if (!typeof(TBase).IsAssignableFrom(type)) return false;

      // Use reflection to call CanCreate<T>() with the correct type argument
      var method = GetType().GetMethod(nameof(CanCreate)).MakeGenericMethod(type);
      return (bool)method.Invoke(this, null);
    }

    bool IFactory.TryCreate(Type type, out object instance) {
      if (!typeof(TBase).IsAssignableFrom(type)) {
        instance = null;
        return false;
      }

      // Use reflection to call TryCreate<T>() with the correct type argument
      var method = GetType().GetMethod(nameof(TryCreate)).MakeGenericMethod(type);
      var success = (bool)method.Invoke(this, new object[] { null });
      instance = method.GetParameters()[0].DefaultValue;
      return success;
    }

    bool IFactory<TBase>.CanCreate() => CanCreate<TBase>();

    bool IFactory<TBase>.TryCreate(out TBase instance) => TryCreate(out instance);

    #endregion
  }
}