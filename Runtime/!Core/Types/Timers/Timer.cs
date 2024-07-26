using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Shadowpaw {
  [Serializable]
  public class Timer : ISerializationCallbackReceiver {
    /// <summary>
    /// How the Timer be updated
    /// </summary>
    public enum TickType {
      /// <summary>
      /// The timer will tick using Time.deltaTime
      /// </summary>
      ScaledTime,

      /// <summary>
      /// The timer will tick using Time.unscaledDeltaTime
      /// </summary>
      UnscaledTime,

      /// <summary>
      /// The timer will only tick when manually updated
      /// </summary>
      Manual
    }

    /// <summary>
    /// The name of the timer, for debug purposes
    /// </summary>
    [TabGroup("Settings")]
    public string Name = "Timer";

    /// <summary>
    /// How long the timer should run during each loop.
    /// If set to 0, the timer will run indefinitely.
    /// </summary>
    [TabGroup("Settings"), Unit(Units.Second)]
    public float Duration = 1f;

    /// <summary>
    /// How many loops the timer should run before stopping.
    /// If set to 0, the timer will loop indefinitely.
    /// </summary>
    [TabGroup("Settings")]
    public int LoopCount = 1;

    /// <inheritdoc cref="TickType"/>
    [TabGroup("Settings"), LabelText("Tick Type")]
    public TickType Type = TickType.ScaledTime;

    /// <summary>
    /// Invoked when the timer is ticked.
    /// The float parameter is the percentage of time elapsed in the current loop.
    /// </summary>
    [TabGroup("Events")] public UnityEvent<float> OnUpdate;

    /// <summary>
    /// Invoked when the timer completes a loop
    /// </summary>
    [TabGroup("Events")] public UnityEvent OnComplete;

    /// <summary>
    /// Invoked when the timer starts
    /// </summary>
    [TabGroup("Events")] public UnityEvent OnStart;

    /// <summary>
    /// Invoked when the timer stops
    /// </summary>
    [TabGroup("Events")] public UnityEvent OnStop;

    /// <summary>
    /// If the Timer is currently running
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// The number of loops that have elapsed...
    /// </summary>
    public int LoopsElapsed { get; private set; }

    /// <summary>
    /// The amount of time that has elapsed...
    /// </summary>
    public float TimeElapsed { get; private set; }

    /// <summary>
    /// The percentage of time that has elapsed in the current loop.
    /// </summary>
    public float Percentage => Duration > 0 ? TimeElapsed / Duration : 1f;

    /// <summary>
    /// Starts the timer
    /// </summary>
    public void Start() {
      Singletons.Get<Timers>().Register(this);
      IsRunning = true;
      OnStart.Invoke();
    }

    /// <summary>
    /// Stops the timer
    /// </summary>
    public void Stop() {
      Singletons.Get<Timers>().Unregister(this);
      IsRunning = false;
      OnStop.Invoke();
    }

    /// <summary>
    /// Pauses the timer
    /// </summary>
    public void Pause() {
      IsRunning = false;
    }

    /// <summary>
    /// Resumes the timer
    /// </summary>
    public void Resume() {
      IsRunning = true;
    }

    /// <summary>
    /// Resets the timer to its initial state
    /// </summary>
    /// <param name="restart">
    /// Whether or not to automatically restart the timer
    /// </param>
    public void Reset(bool restart = true) {
      TimeElapsed = 0;
      LoopsElapsed = 0;

      if (restart) Start();
    }

    /// <summary>
    /// Ticks the timer based on the timer type,
    /// provided that the timer is running
    /// </summary>
    public void Tick() {
      if (!IsRunning) return;

      switch (Type) {
        case TickType.ScaledTime:
          Tick(Time.deltaTime);
          break;
        case TickType.UnscaledTime:
          Tick(Time.unscaledDeltaTime);
          break;
      }
    }

    /// <summary>
    /// Ticks the timer by the specified amount of time
    /// </summary>
    public void Tick(float deltaTime) {
      TimeElapsed += deltaTime;

      // Check if the timer has completed a loop
      if (Duration > 0 && TimeElapsed >= Duration) {
        TimeElapsed = 0;
        LoopsElapsed++;
        OnComplete.Invoke();
      }

      // Invoke the OnUpdate event
      OnUpdate.Invoke(Percentage);

      // Check if the timer has completed all loops
      if (LoopCount > 0 && LoopsElapsed > LoopCount) {
        Stop();
      }
    }

    public void OnBeforeSerialize() {
      // Ensure that the timer settings are valid
      if (Duration < 0) Duration = 0;
      if (LoopCount < 0) LoopCount = 0;
    }

    public void OnAfterDeserialize() { }
  }
}