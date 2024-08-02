
using System;

namespace Shadowpaw {
  /// <summary>
  /// Interface for primitive value types.
  /// </summary>
  public interface IPrimitiveValue : IEquatable<IPrimitiveValue> {
    /// <returns>
    /// The value as a boolean.
    /// </returns>
    bool ToBool();

    /// <returns>
    /// The value as an integer.
    /// </returns>
    int ToInt();

    /// <returns>
    /// The value as a single precision decimal.
    /// </returns>
    float ToFloat();

    /// <returns>
    /// The value as a string.
    /// </returns>
    string ToString();
  }

  /// <summary>
  /// Interface for primitive value types of type T.
  /// </summary>
  public interface IPrimitiveValue<T> : IPrimitiveValue, IEquatable<IPrimitiveValue<T>> {
    T Value { get; set; }
  }
}