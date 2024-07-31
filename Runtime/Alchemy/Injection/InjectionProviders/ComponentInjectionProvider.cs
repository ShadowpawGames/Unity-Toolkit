using System;
using UnityEngine;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Injection Provider for Unity Components.
  /// </summary>
  public class ComponentInjectionProvider : InjectionProvider<Component> {
    public override bool CanProvide<T>(object context = null) {
      return context switch {
        GameObject gameObject => gameObject.GetComponent<T>() != null,
        Component component => component.GetComponent<T>() != null,
        _ => false
      };
    }

    public override bool TryProvide<T>(out T instance, object context = null) {
      instance = context switch {
        GameObject gameObject => gameObject.GetComponent<T>(),
        Component component => component.GetComponent<T>(),
        _ => null
      };
      return instance != null;
    }
  }
}
