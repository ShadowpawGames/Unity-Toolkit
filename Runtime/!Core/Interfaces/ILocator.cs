using System;

namespace Shadowpaw {
  /// <summary>
  /// A Locator that can locate instances of a given type.
  /// </summary>
  public interface ILocator : IProvider {
    /// <summary>
    /// Determines if this locator can locate an instance of the given type.
    /// </summary>
    bool CanLocate(Type type);

    /// <summary>
    /// Tries to locate an instance of the given type.
    /// </summary>
    bool TryLocate(Type type, out object instance);

    /// <summary>
    /// Locates an instance of the given type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the locator cannot locate an instance of the given type.
    /// </exception>
    object Locate(Type type) {
      if (TryLocate(type, out var instance)) return instance;
      throw new InvalidOperationException($"Cannot locate an instance of type '{type.FullName}'");
    }

    #region IProvider (Explicit Implementation)

    bool IProvider.CanProvide(Type type) => CanLocate(type);
    bool IProvider.TryProvide(Type type, out object instance) => TryLocate(type, out instance);

    #endregion
  }

  /// <inheritdoc cref="ILocator"/>
  public interface ILocator<T> : ILocator, IProvider<T> {
    /// <inheritdoc cref="ILocator.CanLocate(Type)"/>
    bool CanLocate() => true;

    /// <inheritdoc cref="ILocator.TryLocate(Type, out object)"/>
    bool TryLocate(out T instance);

    /// <inheritdoc cref="ILocator.Locate(Type)"/>
    T Locate() {
      if (TryLocate(out var instance)) return instance;
      throw new InvalidOperationException($"Cannot locate an instance of type '{typeof(T).FullName}'");
    }

    #region ILocator (Explicit Implementation)

    bool ILocator.CanLocate(Type type) => type == typeof(T);

    bool ILocator.TryLocate(Type type, out object instance) {
      if (CanLocate(type)) {
        var success = TryLocate(out var result);
        instance = result;
        return success && result != null;
      }

      instance = null;
      return false;
    }

    #endregion

    #region IProvider<T> (Explicit Implementation)

    bool IProvider<T>.CanProvide() => CanLocate();
    bool IProvider<T>.TryProvide(out T instance) => TryLocate(out instance);

    #endregion
  }

  /// <summary>
  /// A Locator that can locate instances of a given base type and its subtypes.
  /// </summary>
  public interface ISubtypeLocator<TBase> : ILocator<TBase> {
    bool CanLocate<T>() where T : TBase;
    bool TryLocate<T>(out T instance) where T : TBase;
    T Locate<T>() where T : TBase;

    #region ILocator (Explicit Implementation)

    bool ILocator.CanLocate(Type type) {
      if (!typeof(TBase).IsAssignableFrom(type)) return false;

      // Use reflection to call CanCreate<T>() with the correct type argument
      var method = GetType().GetMethod(nameof(CanLocate)).MakeGenericMethod(type);
      return (bool)method.Invoke(this, null);
    }

    bool ILocator.TryLocate(Type type, out object instance) {
      if (!typeof(TBase).IsAssignableFrom(type)) {
        instance = null;
        return false;
      }

      // Use reflection to call TryCreate<T>() with the correct type argument
      var method = GetType().GetMethod(nameof(TryLocate)).MakeGenericMethod(type);
      var success = (bool)method.Invoke(this, new object[] { null });
      instance = method.GetParameters()[0].DefaultValue;
      return success;
    }

    bool ILocator<TBase>.CanLocate() => CanLocate<TBase>();

    bool ILocator<TBase>.TryLocate(out TBase instance) => TryLocate(out instance);

    #endregion
  }
}