using UnityEngine;

namespace Shadowpaw {
  /// <summary>
  /// Bootstrappers initialize and register systems or services 
  /// which need to be initialized before other components in the scene.
  /// </summary>
  /// <remarks>
  /// Bootstrappers are executed before any other MonoBehaviour in the scene.
  /// They generally should not have a lifecycle beyond Awake and Start.
  /// It is recommended to destroy the Bootstrapper after initialization.
  /// </remarks>
  [DefaultExecutionOrder(-9999)]
  public abstract class Bootstrapper : MonoBehaviour { }
}
