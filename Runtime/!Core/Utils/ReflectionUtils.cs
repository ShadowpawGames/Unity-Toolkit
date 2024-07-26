using System;
using System.Collections.Generic;
using System.Linq;

namespace Shadowpaw.Alchemy {
  public static class ReflectionUtils {
    private static IEnumerable<Type> allTypes = null;

    /// <returns>
    /// All exported types from all assemblies in the current AppDomain.
    /// </returns>
    public static IEnumerable<Type> GetAllTypes() {
      // Cache the result, as this can be quite expensive
      allTypes ??= AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic)
        .SelectMany(a => a.GetExportedTypes())
        .Where(t => t != null);
      return allTypes;
    }

    public static IEnumerable<Type> WithAttribute<T>(this IEnumerable<Type> types, bool inherit = true) where T : Attribute
      => types.WithAttribute(typeof(T), inherit);

    public static IEnumerable<Type> WithAttribute(this IEnumerable<Type> types, Type attrType, bool inherit = true)
      => types.Where(t => t.HasAttribute(attrType, inherit));

    public static IEnumerable<Type> AssignableTo<T>(this IEnumerable<Type> types)
      => types.AssignableTo(typeof(T));

    public static IEnumerable<Type> AssignableTo(this IEnumerable<Type> types, Type targetType)
      => types.Where(t => t.IsAssignableTo(targetType));

    public static IEnumerable<Type> AssignableFrom<T>(this IEnumerable<Type> types)
      => types.AssignableFrom(typeof(T));

    public static IEnumerable<Type> AssignableFrom(this IEnumerable<Type> types, Type sourceType)
      => types.Where(t => t.IsAssignableFrom(sourceType));

    public static IEnumerable<Type> Concretions(this IEnumerable<Type> types)
      => types.Where(t => t.IsConcrete());

    public static IEnumerable<Type> ConcretionsOf<T>(this IEnumerable<Type> types, bool includeSelf = true)
      => types.ConcretionsOf(typeof(T), includeSelf);

    public static IEnumerable<Type> ConcretionsOf(this IEnumerable<Type> types, Type baseType, bool includeSelf = true)
      => types.Where(t => t.IsConcretionOf(baseType, includeSelf));
  }
}
