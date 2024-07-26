using System;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Injection Provider for Singletons.
  /// </summary>
  public class SingletonInjectionProvider : InjectionProvider {
    public override bool CanProvide(Type type, object context = null)
      => Singletons.Contains(type);

    public override bool TryProvide(Type type, out object instance, object context = null)
      => Singletons.TryGet(type, out instance);
  }
}
