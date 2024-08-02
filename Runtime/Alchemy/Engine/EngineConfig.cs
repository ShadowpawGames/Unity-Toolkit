using Sirenix.OdinInspector;
using UnityEngine;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Project Configuration for the Alchemy Engine.
  /// </summary>
  public partial class EngineConfig : Config<EngineConfig> {
    [Tooltip("If enabled, the Engine will initialize automatically at runtime.")]
    [ToggleLeft] public bool AutoInitialize = true;

    [Tooltip("If enabled: Runtime objects will be independant of any loaded scenes. (Recommended!)")]
    [ToggleLeft] public bool SceneIndependent = true;

    [HorizontalGroup("Layer", 147)]
    [Tooltip("If enabled: All runtime objects will be assigned to the given layer.")]
    [ToggleLeft] public bool OverrideObjectLayer = false;

    [HorizontalGroup("Layer")]
    [HideLabel, ShowIf("OverrideObjectLayer")]
    [Tooltip("Which layer to assign to runtime objects.")]
    [Layer] public int ObjectLayer = 0;

    [TitleGroup("Initialization"), Space]
    [Tooltip("Configures which Services are initialized automatically.")]
    [SerializeField] internal EngineServiceList ServiceList = new();
  }
}