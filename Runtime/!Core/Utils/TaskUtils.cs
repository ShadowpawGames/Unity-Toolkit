using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Shadowpaw.Alchemy {
  public static class TaskUtils {
    /// <summary>
    /// Used to fire and forget a task without causing an UnobservedTaskException.
    /// </summary>
    /// <remarks>
    /// Based on a tweet by Ben Adams: https://twitter.com/ben_a_adams/status/1045060828700037125
    /// </remarks>
    public static void Forget(this Task task) {
      static async Task ForgetAwaited(Task task) {
        try {
          await task;
        } catch (Exception ex) {
          Debug.LogError($"An error occurred while executing a task that was forgotten!");
          Debug.LogException(ex);
        }
      }

      if (!task.IsCompleted || task.IsFaulted) {
        // Use the "_" (Discard operation) to remove the warning IDE0058
        // Because this call is not awaited, execution continues before the call is completed
        _ = ForgetAwaited(task);
      }
    }
  }
}