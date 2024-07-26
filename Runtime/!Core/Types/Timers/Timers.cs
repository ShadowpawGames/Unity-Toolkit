using System.Collections.Generic;
using System.Linq;

namespace Shadowpaw {
  /// <summary>
  /// Singleton class that manages all Timers,
  /// updating them automatically every frame.
  /// </summary>
  public class Timers : Singleton<Timers> {
    private readonly HashSet<Timer> timers = new();

    /// <summary>
    /// Register a timer to be updated automatically
    /// </summary>
    public bool Register(Timer timer) => timers.Add(timer);

    /// <summary>
    /// Unregister a timer from being updated automatically
    /// </summary>
    public bool Unregister(Timer timer) => timers.Remove(timer);

    /// <summary>
    /// Update all registered timers
    /// </summary>
    private void Update() {
      //! Uses ToArray() to avoid modifying the collection while iterating
      foreach (var timer in timers.ToArray()) {
        timer.Tick();
      }
    }

    private void OnDestroy() {
      // Stop all timers when the object is destroyed
      foreach (var timer in timers.ToArray()) {
        timer.Stop();
      }
    }
  }
}