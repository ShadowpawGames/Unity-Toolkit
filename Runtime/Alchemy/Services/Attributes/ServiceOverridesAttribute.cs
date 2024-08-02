using System;
using UnityEngine;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Attribute to provide information about Services. <br />
  /// Allows a Service to override other existing Services.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class ServiceOverridesAttribute : Attribute {
    /// <summary>
    /// The service types that this service overrides.
    /// </summary>
    public Type[] ServiceTypes { get; init; }

    public ServiceOverridesAttribute(params Type[] serviceTypes) {
      ServiceTypes = serviceTypes;

#if UNITY_EDITOR
      // Assert that all service types are valid.
      foreach (var type in serviceTypes) {
        Debug.Assert(
          type != null,
          "ServiceOverrides: Type cannot be null."
        );

        Debug.Assert(
          !type.IsConcrete(),
          $"ServiceOverrides: Type must be concrete. ({type.Name})"
        );

        Debug.Assert(
          typeof(IService).IsAssignableFrom(type),
          "ServiceOverrides: Type must be an implementation of IService."
        );
      }
#endif
    }
  }
}