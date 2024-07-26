using System.Collections;
using System.Collections.Generic;

namespace Shadowpaw
{
  /// <summary>
  /// A registry of items of a given type.
  /// </summary>
  /// <remarks>
  /// Items in the registry can be registered, unregistered, and enumerated.
  /// May be implemented as a list, set, dictionary, or other collection type.
  /// </remarks>
  public interface IRegistry<T> : IEnumerable<T>
  {
    /// <summary>
    /// The contents of the registry.
    /// </summary>
    IEnumerable<T> Entries { get; }

    /// <summary>
    /// Returns true if the given item is registered in the registry.
    /// </summary>
    public bool IsRegistered(T item);

    /// <summary>
    /// Adds the given item to the registry.
    /// </summary>
    /// <param name="overwrite">
    /// If true, the item will be added even if it overwrites an existing item.
    /// </param>
    public void Register(T item, bool overwrite = true);

    /// <summary>
    /// Removes the given item from the registry.
    /// </summary>
    public void Unregister(T item);

    /// <summary>
    /// Clears all entries from the registry.
    /// </summary>
    public void Clear();

    #region IEnumerable<T> (Explicit Implementation)

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => Entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();

    #endregion
  }
}