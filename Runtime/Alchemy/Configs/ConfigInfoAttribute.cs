using System;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Provides additional information about a <see cref="Config"/>  object
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class ConfigInfoAttribute : Attribute {
    /// <summary>
    /// If provided, this will be the display name used in the inspector.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// If provided, a help button will be displayed in the inspector that links to this URL.
    /// </summary>
    public string HelpURL { get; set; }

    /// <summary>
    /// If true, this config will hidden from Project Settings
    /// </summary>
    public bool Hidden { get; set; }

    public ConfigInfoAttribute(string displayName = null) {
      DisplayName = displayName;
    }
  }
}