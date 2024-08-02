namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Injection Provider for Injection Contexts.
  /// </summary>
  public class ServiceInjectionProvider : InjectionProvider<IService> {
    public override bool CanProvide<T>(object context = null)
      => Services.TryGet<T>(out _);

    public override bool TryProvide<T>(out T instance, object context = null)
      => Services.TryGet(out instance);
  }
}
