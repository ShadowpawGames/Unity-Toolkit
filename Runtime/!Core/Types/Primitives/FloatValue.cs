using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// Represents a float value as a serializable struct. <br />
  /// This allows Unity to serialize the value in the inspector.
  /// </summary>
  [Serializable, InlineProperty]
  public struct FloatValue : IPrimitiveValue<float> {
    [field: SerializeField, HideLabel]
    public float Value { get; set; }

    public readonly bool ToBool() => Value != 0;
    public readonly int ToInt() => Mathf.RoundToInt(Value);
    public readonly float ToFloat() => Value;
    public override readonly string ToString() => Value.ToString();

    #region IEquatable, et al.

    public override readonly int GetHashCode() {
      return Value.GetHashCode();
    }

    public override readonly bool Equals(object obj) {
      if (obj is float i) return Value == i;
      return base.Equals(obj);
    }

    public readonly bool Equals(IPrimitiveValue other) {
      return Value == other.ToFloat();
    }

    public readonly bool Equals(IPrimitiveValue<float> other) {
      return Value == other.Value;
    }

    public static bool operator ==(FloatValue a, IPrimitiveValue b) => a.Equals(b);
    public static bool operator !=(FloatValue a, IPrimitiveValue b) => !a.Equals(b);

    public static implicit operator FloatValue(float value) => new() { Value = value };
    public static implicit operator float(FloatValue value) => value.Value;

    #endregion
  }
}