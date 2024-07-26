using System;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Marks a Constructor, Field, Property, or Method for dependancy injection. <br />
  /// - Injection always occurs in the following order: Constructors Fields, Properties, Methods <br />
  /// - At most one constructor should be marked for injection. <br />
  /// - If no constructor is marked, there may be only one constructor. <br />
  /// - Constructor/Method parameters with default values are considered optional. <br />
  /// - If a Required injection fails, an exception will be thrown. <br />
  /// </summary>
  [AttributeUsage(
    AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = true
  )]
  public sealed class InjectAttribute : Attribute {
    /// <summary>
    /// If true, not exception will be thrown if the dependancy cannot be injected.
    /// </summary>
    public bool Required { get; init; }

    public InjectAttribute(bool required = true) {
      Required = required;
    }
  }
}