using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// Represents an integer value as a serializable struct. <br />
  /// This allows Unity to serialize the value in the inspector.
  /// </summary>
  [Serializable, InlineProperty]
  public struct IntValue : IPrimitiveValue<int> {
    [field: SerializeField, HideLabel]
    public int Value { get; set; }

    public readonly bool ToBool() => Value != 0;
    public readonly int ToInt() => Value;
    public readonly float ToFloat() => Value;
    public override readonly string ToString() => Value.ToString();

    #region IEquatable, et al.

    public override readonly int GetHashCode() {
      return Value.GetHashCode();
    }

    public override readonly bool Equals(object obj) {
      if (obj is int i) return Value == i;
      return base.Equals(obj);
    }

    public readonly bool Equals(IPrimitiveValue other) {
      return Value == other.ToInt();
    }

    public readonly bool Equals(IPrimitiveValue<int> other) {
      return Value == other.Value;
    }

    public static bool operator ==(IntValue a, IPrimitiveValue b) => a.Equals(b);
    public static bool operator !=(IntValue a, IPrimitiveValue b) => !a.Equals(b);

    public static implicit operator IntValue(int value) => new() { Value = value };
    public static implicit operator int(IntValue value) => value.Value;

    #endregion
  }
}