using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Shadowpaw.InputSystem {
  [AddComponentMenu("Input/InputAction Events")]
  public class InputActionEvents : MonoBehaviour {
    [SerializeField, Required, HideLabel]
    private InputActionReference inputAction;

    [ShowIf("inputAction")]
    [TabGroup("Events", "Performed")]
    public UnityEvent OnInputActionPerformed = new();

    [ShowIf("inputAction")]
    [TabGroup("Events", "Started")]
    public UnityEvent OnInputActionStarted = new();

    [ShowIf("inputAction")]
    [TabGroup("Events", "Canceled")]
    public UnityEvent OnInputActionCanceled = new();

    private void OnEnable() {
      inputAction.action.Enable();
      inputAction.action.performed += InputActionPerformed;
      inputAction.action.started += InputActionStarted;
      inputAction.action.canceled += InputActionCanceled;
    }

    private void OnDisable() {
      inputAction.action.Disable();
      inputAction.action.started -= InputActionStarted;
      inputAction.action.performed -= InputActionPerformed;
      inputAction.action.canceled -= InputActionCanceled;
    }

    private void InputActionStarted(InputAction.CallbackContext context)
      => OnInputActionStarted?.Invoke();

    private void InputActionPerformed(InputAction.CallbackContext context)
      => OnInputActionPerformed?.Invoke();

    private void InputActionCanceled(InputAction.CallbackContext context)
      => OnInputActionCanceled?.Invoke();
  }
}