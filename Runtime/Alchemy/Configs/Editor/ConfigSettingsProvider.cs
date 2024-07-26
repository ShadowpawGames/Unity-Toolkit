using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shadowpaw.Alchemy.Editors {
  public class ConfigSettingsProvider : SettingsProvider {
    #region Fields

    /// <summary>
    /// Whether to draw the asset field at the top of the settings window.
    /// Useful for quickly locating the config asset in the project.
    /// </summary>
    public static bool DrawAssetField = false;

    /// <summary>
    /// The root path within the project settings window.
    /// </summary>
    public const string ProjectSettingsPath = "Configs";

    /// <summary>
    /// The folder path where new config assets will be created.
    /// This path MUST contain a "Resources" folder.
    /// </summary>
    public const string ResourceFolderPath = "Plugins/Shadowpaw/Resources/Configs";

    private Type type;
    private Editor editor;
    private string helpURL;
    private UnityEngine.Object target;
    private UnityEngine.Object[] assetTargets;
    private SerializedObject serializedObject;

    #endregion

    #region Statics

    private static readonly GUIContent helpIcon;
    private static readonly GUIStyle iconStyle;
    static ConfigSettingsProvider() {
      // Initialize the help icon and icon button style
      helpIcon = EditorGUIUtility.IconContent("_help", "View Documentation");
      iconStyle = GUI.skin.FindStyle("IconButton")
        ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("IconButton");
    }

    private static string GetSettingsPath(Type type)
      => $"{ProjectSettingsPath}/{GetDisplayName(type)}";

    private static string GetDisplayName(Type type) {
      var attr = type.GetCustomAttribute<ConfigInfoAttribute>();

      if (!string.IsNullOrEmpty(attr?.DisplayName)) return attr.DisplayName;

      return type.Name
        .Replace("Configuration", string.Empty)
        .Replace("Config", string.Empty)
        .Nicify();
    }

    [MenuItem("Tools/Alchemy Configs")]
    private static void OpenWindow() => SettingsService.OpenProjectSettings(ProjectSettingsPath);

    [SettingsProviderGroup]
    private static SettingsProvider[] CreateConfigSettingsProviders() {
      try {
        return ReflectionUtils
          .GetAllTypes()
          .ConcretionsOf<ISingleton>()
          .Where(t => t.IsSubclassOfRawGeneric(typeof(Config<>)))
          .Where(t => {
            var attr = t.GetCustomAttribute<ConfigInfoAttribute>();
            return attr == null || !attr.Hidden;
          })
          .Select(t => new ConfigSettingsProvider(t))
          .ToArray();
      } catch (Exception e) {
        Debug.LogError("Alchemy: Failed to create Config settings providers");
        Debug.LogException(e);
        return default;
      }
    }

    #endregion

    private ConfigSettingsProvider(Type type) : this(type, GetSettingsPath(type)) { }

    private ConfigSettingsProvider(Type type, string path) : base(path, SettingsScope.Project) {
      // Ensure the type is a subclass of Config
      Debug.Assert(type.IsSubclassOfRawGeneric(typeof(Config<>)));

      this.type = type;
      var attr = type.GetCustomAttribute<ConfigInfoAttribute>();
      if (attr != null) helpURL = attr.HelpURL;
    }

    private UnityEngine.Object GetOrCreateConfig(Type type) {
      // Look for an existing config asset
      var config = Resources.LoadAll("", type).FirstOrDefault();
      if (config != null) return config;

      // Create a new config asset
      config = ScriptableObject.CreateInstance(type);

      // Create the Resources folder if it doesn't exist
      var directory = Path.Combine(Application.dataPath, ResourceFolderPath);
      Directory.CreateDirectory(directory);

      // Create the asset file if it doesn't exist.
      var filePath = Path.Combine(ResourceFolderPath, $"{type.Name}.asset");
      AssetDatabase.CreateAsset(config, Path.Combine("Assets", filePath));
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      return config;
    }

    public override void OnActivate(string searchContext, VisualElement rootElement) {
      target = GetOrCreateConfig(type);
      serializedObject = new SerializedObject(target);
      assetTargets = new UnityEngine.Object[] { target };
      keywords = GetSearchKeywordsFromSerializedObject(serializedObject);
    }

    public override void OnTitleBarGUI() {
      const int upperMargin = 6, rightMargin = 2;

      var rect = GUILayoutUtility.GetRect(helpIcon, iconStyle);
      rect.y = upperMargin;
      rect.x -= rightMargin;
      PresetSelector.DrawPresetButton(rect, assetTargets);

      if (string.IsNullOrEmpty(helpURL)) return;

      rect.x -= rect.width + rightMargin;
      if (GUI.Button(rect, helpIcon, iconStyle))
        Application.OpenURL(helpURL);
    }

    public override void OnGUI(string searchContext) {
      if (target == null) {
        EditorGUILayout.HelpBox("An error occurred while rendering the settings window." +
        "Try re-opening the settings window or restarting the Unity editor.", MessageType.Error);
        return;
      }

      // Update the cached editor
      Editor.CreateCachedEditor(serializedObject.targetObject, null, ref editor);

      EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

      if (DrawAssetField) {
        // Draw a reference to the config asset with no label
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField(target, typeof(ScriptableObject), false);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Space();
      }

      // Draw the editor
      editor.OnInspectorGUI();

      EditorGUILayout.EndVertical();
    }
  }
}