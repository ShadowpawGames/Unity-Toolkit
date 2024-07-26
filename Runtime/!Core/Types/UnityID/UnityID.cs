using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// A globally unique identifier (GUID) for Unity.
  /// It is serializable and persists across scenes and runtime sessions.
  /// </summary>
  [Serializable, HideLabel, InlineProperty]
  public struct UnityID : IEquatable<UnityID> {
    [ShowInInspector, LabelText("@$property.Parent.NiceName")]
    [CustomContextMenu("Generate GUID", nameof(GenerateGUID))]
    [CustomContextMenu("Clear GUID", nameof(ClearGUID))]
    public readonly string HexString => ToString();

    [SerializeField, HideInInspector]
    private uint Part1, Part2, Part3, Part4;

    public static UnityID Empty => new(0, 0, 0, 0);

    public readonly bool IsEmpty => this == Empty;

    public UnityID(uint part1, uint part2, uint part3, uint part4) {
      Part1 = part1;
      Part2 = part2;
      Part3 = part3;
      Part4 = part4;
    }

    public UnityID(Guid guid) {
      byte[] bytes = guid.ToByteArray();
      Part1 = BitConverter.ToUInt32(bytes, 0);
      Part2 = BitConverter.ToUInt32(bytes, 4);
      Part3 = BitConverter.ToUInt32(bytes, 8);
      Part4 = BitConverter.ToUInt32(bytes, 12);
    }

    public override readonly string ToString() {
      return $"{Part1:X8}-{Part2:X8}-{Part3:X8}-{Part4:X8}";
    }

    public static UnityID Parse(string hexString) {
      // Remove hyphens and convert to lowercase
      hexString = hexString.Replace("-", "").ToLower();

      if (hexString.Length != 32) return Empty;

      return new UnityID(
        Convert.ToUInt32(hexString[0..8], 16),
        Convert.ToUInt32(hexString[8..16], 16),
        Convert.ToUInt32(hexString[16..24], 16),
        Convert.ToUInt32(hexString[24..32], 16)
      );
    }

    public readonly Guid ToGuid() {
      byte[] bytes = new byte[16];
      BitConverter.GetBytes(Part1).CopyTo(bytes, 0);
      BitConverter.GetBytes(Part2).CopyTo(bytes, 4);
      BitConverter.GetBytes(Part3).CopyTo(bytes, 8);
      BitConverter.GetBytes(Part4).CopyTo(bytes, 12);
      return new Guid(bytes);
    }

    public static UnityID New() => Guid.NewGuid();

    public static implicit operator UnityID(Guid guid) => new(guid);
    public static implicit operator Guid(UnityID unityId) => unityId.ToGuid();

    public static bool operator ==(UnityID left, UnityID right) => left.Equals(right);
    public static bool operator !=(UnityID left, UnityID right) => !left.Equals(right);

    public override readonly bool Equals(object obj) {
      return obj is UnityID unityId && Equals(unityId);
    }

    public readonly bool Equals(UnityID other) {
      return Part1 == other.Part1
        && Part2 == other.Part2
        && Part3 == other.Part3
        && Part4 == other.Part4;
    }

    public override readonly int GetHashCode() {
      return HashCode.Combine(Part1, Part2, Part3, Part4);
    }

    private void ClearGUID() => this = Empty;
    private void GenerateGUID() => this = New();
  }
}