using System;
using System.Reflection;

namespace Shadowpaw.Alchemy {
  public static class TypeUtils {
    public static bool HasAttribute<T>(this Type type, bool inherit = true) where T : Attribute
      => type.HasAttribute(typeof(T), inherit);

    public static bool HasAttribute(this Type type, Type attrType, bool inherit = true)
      => type.GetCustomAttribute(attrType, inherit) != null;

    public static bool IsAssignableTo<T>(this Type type)
      => type.IsAssignableTo(typeof(T));

    public static bool IsAssignableTo(this Type type, Type targetType)
      => targetType.IsAssignableFrom(type);

    public static bool IsConcrete(this Type type)
      => !type.IsAbstract && !type.IsInterface;

    public static bool IsConcretionOf<T>(this Type type, bool includeSelf = true)
      => type.IsConcretionOf(typeof(T), includeSelf);

    public static bool IsConcretionOf(this Type type, Type baseType, bool includeSelf = true)
      => type.IsConcrete() && type.IsAssignableTo(baseType) && (includeSelf || type != baseType);

    /// <summary>
    /// Determines whether the specified type is a subclass of an unbound generic type.
    /// </summary>
    public static bool IsSubclassOfRawGeneric(this Type type, Type generic) {
      while (type != null && type != typeof(object)) {
        var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        if (generic == current) return true;
        type = type.BaseType;
      }
      return false;
    }
  }
}