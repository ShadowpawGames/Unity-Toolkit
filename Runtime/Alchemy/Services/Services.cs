using System;
using System.Collections.Generic;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Provides a static registry for Serviecs.
  /// </summary>
  public static class Services {
    private static readonly TypeRegistry<IService> _registry = new();
  }
}