using System;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Attribute to provide information about Services. <br />
  /// Used to control how Services are displayed and initialized.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class ServiceInfoAttribute : Attribute {
    /// <summary>
    /// The display name of the Service. <br />
    /// If not provided, the name of the class will be used.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// If true, the Service will be required to be initialized. <br />
    /// This prevents the Service from being disabled in the inspector.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// The initialization priority of the Service. <br />
    /// Services with higher priority will be initialized first. <br />
    /// Dependencies will always be initialized before dependents.
    /// </summary>
    public int Priority { get; init; }

    public ServiceInfoAttribute(string displayName = "") {
      DisplayName = displayName;
    }
  }
}