using Sirenix.OdinInspector;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Base class for Project Configs.
  /// These are ScriptableObjects that are used to store project-wide settings.
  /// </summary>
  /// <remarks>
  /// Use <see cref="ConfigInfoAttribute"/> to customize how the Config appears in the Editor.
  /// </remarks>
  [HideMonoScript]
  public abstract class Config<T> : SingletonObject<T> where T : Config<T> { }
}