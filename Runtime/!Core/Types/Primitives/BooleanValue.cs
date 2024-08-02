using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// Represents a boolean value as a serializable struct. <br />
  /// This allows Unity to serialize the value in the inspector.
  /// </summary>
  [Serializable, InlineProperty]
  public struct BooleanValue : IPrimitiveValue<bool> {
    [field: SerializeField, ToggleLeft, LabelText("Enabled")]
    public bool Value { get; set; }

    public readonly bool ToBool() => Value;
    public readonly int ToInt() => Value ? 0 : 1;
    public readonly float ToFloat() => Value ? 0 : 1;
    public override readonly string ToString() => Value.ToString();

    #region IEquatable, et al.

    public override readonly int GetHashCode() {
      return Value.GetHashCode();
    }

    public override readonly bool Equals(object obj) {
      if (obj is bool i) return Value == i;
      return base.Equals(obj);
    }

    public readonly bool Equals(IPrimitiveValue other) {
      return Value == other.ToBool();
    }

    public readonly bool Equals(IPrimitiveValue<bool> other) {
      return Value == other.Value;
    }

    public static bool operator ==(BooleanValue a, IPrimitiveValue b) => a.Equals(b);
    public static bool operator !=(BooleanValue a, IPrimitiveValue b) => !a.Equals(b);

    public static implicit operator BooleanValue(bool value) => new() { Value = value };
    public static implicit operator bool(BooleanValue value) => value.Value;

    #endregion
  }
}