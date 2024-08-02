using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Singleton component used to access Engine events in the inspector.
  /// </summary>
  [AddComponentMenu("Alchemy/Engine Events")]
  public class EngineEvents : Singleton<EngineEvents> {
    #region Lifecycle

    /// <summary>
    /// Event called when the Engine is started.
    /// </summary>
    [TabGroup("Events", "Lifecycle")]
    public UnityEvent OnEngineStart = new();

    /// <summary>
    /// Event called when the Engine is reset.
    /// Passes an array of types that were reset.
    /// </summary>
    [TabGroup("Events", "Lifecycle")]
    public UnityEvent OnEngineReset = new();

    /// <summary>
    /// Event called when the Engine is destroyed.
    /// </summary>
    [TabGroup("Events", "Lifecycle")]
    public UnityEvent OnEngineDestroy = new();

    #endregion

    #region Initialization

    /// <summary>
    /// Event called when Engine initialization starts.
    /// </summary>
    [TabGroup("Events", "Initialization")]
    public UnityEvent OnInitStarted = new();

    /// <summary>
    /// Event called when Engine initialization finishes.
    /// </summary>
    [TabGroup("Events", "Initialization")]
    public UnityEvent OnInitComplete = new();

    /// <summary>
    /// Event called when the Engine initialization progress changes.
    /// </summary>
    [TabGroup("Events", "Initialization")]
    public UnityEvent<float> OnInitProgress = new();

    #endregion

#if UNITY_EDITOR

    [TitleGroup("Engine Controls")]
    [DisableIf("@Engine.IsInitialized")]
    [Button(ButtonSizes.Large), HideInEditorMode]
    protected async void StartEngine() => await Engine.Initialize();

    [TitleGroup("Engine Controls")]
    [EnableIf("@Engine.IsInitialized")]
    [Button(ButtonSizes.Large), HideInEditorMode]
    protected void ResetEngine() => Engine.Reset();

    [TitleGroup("Engine Controls")]
    [EnableIf("@Engine.IsInitialized")]
    [Button(ButtonSizes.Large), HideInEditorMode]
    protected void DestroyEngine() => Engine.Destroy();
#endif
  }
}