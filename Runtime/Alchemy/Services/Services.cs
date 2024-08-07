using System;
using System.Collections;
using System.Collections.Generic;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Provides a static registry for Serviecs.
  /// </summary>
  public static class Services {
    private static readonly TypeRegistry<IService> serviceRegistry = new();

    /// <inheritdoc cref="TypeRegistry{T}.IsRegistered(T)"/>
    public static bool IsRegistered(Type key)
      => serviceRegistry.IsRegistered(key);

    /// <inheritdoc cref="TypeRegistry{T}.IsRegistered{T}()"/>
    public static bool IsRegistered<T>() where T : IService
      => serviceRegistry.IsRegistered<T>();

    /// <inheritdoc cref="TypeRegistry{T}.Register(T, bool)"/>
    public static bool Register(Type key, IService value, bool overwrite = true)
      => serviceRegistry.Register(key, value, overwrite);

    /// <inheritdoc cref="TypeRegistry{T}.Register{T}(T, bool)"/>
    public static bool Register<T>(T value, bool overwrite = true) where T : IService
      => serviceRegistry.Register(value, overwrite);

    /// <inheritdoc cref="TypeRegistry{T}.Unregister(T)"/>
    public static void Unregister(Type key)
      => serviceRegistry.Unregister(key);

    /// <inheritdoc cref="TypeRegistry{T}.Register{T}(T, bool)"/>
    public static void Unregister<T>() where T : IService
      => serviceRegistry.Unregister<T>();

    /// <inheritdoc cref="TypeRegistry{T}.Unregister(T)"/>
    public static void Unregister<T>(T service) where T : IService
      => serviceRegistry.Unregister(service);

    /// <inheritdoc cref="TypeRegistry{T}.Clear"/>
    public static void Clear()
      => serviceRegistry.Clear();

    /// <inheritdoc cref="TypeRegistry{T}.TryGet(Type, out T, bool)"/>
    public static bool TryGet(Type type, out IService service)
      => serviceRegistry.TryGet(type, out service, true);

    /// <inheritdoc cref="TypeRegistry{T}.TryGet{T}(out T, bool)"/>
    public static bool TryGet<T>(out T service) where T : IService
      => serviceRegistry.TryGet(out service, true);

    /// <inheritdoc cref="TypeRegistry{T}.GetAll(Type)"/>
    public static IEnumerable GetAll(Type type)
      => serviceRegistry.GetAll(type);

    /// <inheritdoc cref="TypeRegistry{T}.GetAll{T}"/>
    public static IEnumerable<T> GetAll<T>() where T : IService
      => serviceRegistry.GetAll<T>();

    /// <summary>
    /// Gets all registered services.
    /// </summary>
    public static IEnumerable<IService> GetAll()
      => serviceRegistry.Values;
  }
}