using System;
using UnityEditor;

namespace Shadowpaw.Alchemy {
  public static class StringUtils {
    /// <returns>
    /// If these strings are equal using <see cref="StringComparison.Ordinal"/>
    /// </returns>
    public static bool Equals(this string source, string other)
      => string.Equals(source, other, StringComparison.Ordinal);

    /// <returns>
    /// If these strings are equal using <see cref="StringComparison.OrdinalIgnoreCase"/>
    /// </returns>
    public static bool EqualsIgnoreCase(this string source, string other)
      => string.Equals(source, other, StringComparison.OrdinalIgnoreCase);

    /// <returns>
    /// If this string is null or empty
    /// </returns>
    public static bool IsNullOrEmpty(this string source)
      => string.IsNullOrEmpty(source);

    /// <returns>
    /// If this string is null, empty, or whitespace
    /// </returns>
    public static bool IsNullOrWhiteSpace(this string source)
      => string.IsNullOrWhiteSpace(source);

    /// <returns>
    /// A nicely formatted version of this string for display in the editor.
    /// Uses Unity's Variable Name Nicifier.
    /// </returns>
    public static string Nicify(this string source)
      => ObjectNames.NicifyVariableName(source);

    /// <summary>
    /// Computes the FNV-1a hash of an input string.
    /// </summary>
    /// <remarks>
    /// This is a non-cryptographic hash function known for its speed and quality distribution.
    /// Useful for hashing dictionary keys for fast lookups.
    /// </remarks>
    public static int GetHashCode_FNV1a(this string source) {
      uint hash = 2166136261;
      foreach (char c in source) {
        hash = (hash ^ c) * 16777619;
      }
      return unchecked((int)hash);
    }
  }
}