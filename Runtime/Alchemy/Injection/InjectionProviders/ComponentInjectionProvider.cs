using System;
using UnityEngine;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Injection Provider for Unity Components.
  /// </summary>
  public class ComponentInjectionProvider : InjectionProvider {
    public override bool CanProvide(Type type, object context = null) {
      return context switch {
        GameObject gameObject => gameObject.GetComponent(type) != null,
        Component component => component.GetComponent(type) != null,
        _ => false
      };
    }

    public override bool TryProvide(Type type, out object instance, object context = null) {
      instance = context switch {
        GameObject gameObject => gameObject.GetComponent(type),
        Component component => component.GetComponent(type),
        _ => null
      };
      return instance != null;
    }
  }
}
