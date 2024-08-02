using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// Represents a string value as a serializable struct. <br />
  /// This allows Unity to serialize the value in the inspector.
  /// </summary>
  [Serializable, InlineProperty]
  public struct StringValue : IPrimitiveValue<string> {
    [field: SerializeField, HideLabel]
    public string Value { get; set; }

    public readonly bool ToBool() {
      try {
        return bool.Parse(Value);
      } catch (Exception) {
        return ToInt() != 0;
      }
    }

    public readonly int ToInt() {
      try {
        return int.Parse(Value);
      } catch (Exception) {
        return Mathf.RoundToInt(ToFloat());
      }
    }

    public readonly float ToFloat() {
      try {
        return float.Parse(Value);
      } catch (Exception) {
        return 0;
      }
    }

    public override readonly string ToString() => Value;

    #region IEquatable, et al.

    public override readonly int GetHashCode() {
      return Value.GetHashCode();
    }

    public override readonly bool Equals(object obj) {
      if (obj is string str) return Value.Equals(str);
      return base.Equals(obj);
    }

    public readonly bool Equals(IPrimitiveValue other) {
      return other.ToString().Equals(Value);
    }

    public readonly bool Equals(IPrimitiveValue<string> other) {
      return other.Value.Equals(Value);
    }

    public static bool operator ==(StringValue a, IPrimitiveValue b) => a.Equals(b);
    public static bool operator !=(StringValue a, IPrimitiveValue b) => !a.Equals(b);

    public static implicit operator StringValue(string value) => new() { Value = value };
    public static implicit operator string(StringValue value) => value.Value;

    #endregion
  }
}