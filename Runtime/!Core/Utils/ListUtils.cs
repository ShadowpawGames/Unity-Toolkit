using System;
using System.Collections.Generic;
using System.Linq;

namespace Shadowpaw.Alchemy {
  public static class ListUtils {
    /// <summary>
    /// Reorders a collection such that no element depends on a previous element.
    /// </summary>
    /// <param name="source">The collection to be reordered.</param>
    /// <param name="getDependencies">
    /// A function used to get the dependencies of a given element.
    /// This will be called recursively, so make sure to handle items not in the source list.
    /// </param>
    /// <param name="equalsDependancy">
    /// A function used to determine if an item (arg1) is equal to a dependancy (arg2).
    /// If null, will use the default equality operator.
    /// </param>
    public static IList<T> OrderByTopology<T>(
      this IEnumerable<T> source,
      Func<T, IEnumerable<T>> getDependencies,
      Func<T, T, bool> equalsDependancy = null
    ) {
      var progressMap = new Dictionary<T, bool>();
      var sourceList = source.ToList();
      var sortedList = new List<T>();

      foreach (var item in source) Visit(item);
      return sortedList;

      bool TryGetVisited(T item, out bool inProgress) {
        // If we don't have an equality function, just check if it is in the map
        if (equalsDependancy == null) {
          return progressMap.TryGetValue(item, out inProgress);
        }

        inProgress = false;
        bool visited = false;

        // Check each key to see if it is 'equal' to the item
        foreach (var kvp in progressMap) {
          if (equalsDependancy(item, kvp.Key)) {
            visited = true;
            if (kvp.Value) {
              inProgress = true;
              break;
            }
          }
        }

        return visited;
      }

      void Visit(T item) {
        var alreadyVisited = TryGetVisited(item, out var inProgress);

        // Skip nodes we've already visited
        if (alreadyVisited) {
          if (inProgress)
            throw new Exception($"Cyclic dependency found while performing topological ordering of {typeof(T).Name}.");
          return;
        }

        // Visit each of this node's dependencies
        progressMap[item] = true;
        var dependencies = getDependencies(item);
        if (dependencies != null) {
          foreach (var dependency in dependencies) Visit(dependency);
        }
        progressMap[item] = false;

        // Make sure to only add items from the source list
        if (sourceList.Contains(item)) {
          sortedList.Add(item);
        }
      }
    }

    public static int FirstIndex<T>(this IEnumerable<T> list, Func<T, bool> predicate, int minIndex = -1) {
      for (int i = minIndex; i < list.Count(); i++) {
        if (predicate(list.ElementAt(i))) return i;
      }
      return -1;
    }

    public static T Random<T>(this T[] arr)
      => arr[UnityEngine.Random.Range(0, arr.Length)];

    public static T Random<T>(this IEnumerable<T> list)
      => list.ElementAt(UnityEngine.Random.Range(0, list.Count()));

    public static IEnumerable<T> Shuffle<T>(this T[] arr)
      => arr.OrderBy(n => Guid.NewGuid());

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
      => list.OrderBy(n => Guid.NewGuid());
  }
}