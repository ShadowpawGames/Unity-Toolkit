using System;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Injection Provider for Injection Contexts.
  /// </summary>
  public class ContextInjectionProvider : InjectionProvider<InjectionContext> {
    public override int Priority { get; init; } = 9999;

    public override bool CanProvide<T>(object context = null)
      => InjectionContexts.For(context).TryGet<T>(out _, true);

    public override bool TryProvide<T>(out T instance, object context = null)
      => InjectionContexts.For(context).TryGet(out instance, true);
  }
}
