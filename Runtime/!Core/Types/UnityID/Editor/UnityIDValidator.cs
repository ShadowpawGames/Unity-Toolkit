using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.OdinInspector;

[assembly: RegisterValidationRule(typeof(Shadowpaw.Editors.UnityIDValidator), Name = "Empty UnityID")]

namespace Shadowpaw.Editors {
  public class UnityIDValidator : ValueValidator<UnityID> {
    [EnumToggleButtons] public ValidatorSeverity Severity = ValidatorSeverity.Warning;

    protected override void Validate(ValidationResult result) {
      if (Value.IsEmpty) {
        result
          .Add(Severity, "UnityID is empty. Are you sure this is correct?")
          .WithFix(() => Value = UnityID.New());
      }
    }
  }
}