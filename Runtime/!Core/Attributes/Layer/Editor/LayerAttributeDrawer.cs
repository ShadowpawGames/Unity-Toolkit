using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Shadowpaw.Editors {
  public class LayerAttributeDrawer : OdinAttributeDrawer<LayerAttribute, int> {
    protected override void DrawPropertyLayout(GUIContent label) {
      ValueEntry.SmartValue = EditorGUILayout.LayerField(label ?? GUIContent.none, ValueEntry.SmartValue);
    }
  }
}
