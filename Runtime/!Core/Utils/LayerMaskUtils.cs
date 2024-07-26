using UnityEngine;

namespace Shadowpaw.Alchemy {
  public static class LayerMaskUtils {
    /// <returns>
    /// If the LayerMask contains the given layer index.
    /// </returns>
    public static bool Contains(this LayerMask mask, int layer)
      => mask == (mask | (1 << layer));

    /// <returns>
    /// If the LayerMask contains the given layer name.
    /// </returns>
    public static bool Contains(this LayerMask mask, string layerName)
      => mask.Contains(LayerMask.NameToLayer(layerName));

    /// <returns>
    /// If the LayerMask contains the given GameObject.
    /// </returns>
    public static bool Contains(this LayerMask mask, GameObject gameObject)
      => mask.Contains(gameObject.layer);

    /// <returns>
    /// If the LayerMask contains the given GameObject.
    /// </returns>
    public static bool Contains(this LayerMask mask, Component component)
      => mask.Contains(component.gameObject.layer);
  }
}