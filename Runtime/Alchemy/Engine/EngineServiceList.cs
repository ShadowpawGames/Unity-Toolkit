using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// A list of all available engine services,
  /// which can be enabled/disabled in the inspector.
  /// </summary>
  /// <remarks>
  /// This class is used by <see cref="EngineConfig"/>
  /// exclusively and should not be used elsewhere.
  /// </remarks>
  [Serializable, InlineProperty, HideLabel]
  internal class EngineServiceList {
    /// <summary>
    /// Contains information about a service type.
    /// </summary>
    [Serializable, Toggle("enabled")]
    private struct ServiceInfo {
      /// <summary>
      /// If the Service should be initialized by the Engine.
      /// </summary>
      public bool enabled;

      /// <summary>
      /// If the Service is required by the Engine to function. (Cannot be disabled)
      /// </summary>
      [HorizontalGroup, ToggleLeft, ReadOnly] public bool required;

      /// <summary>
      /// If the Service has been overridden by another Service.
      /// </summary>
      [HorizontalGroup, ToggleLeft, ReadOnly] public bool overridden;

      /// <summary>
      /// The types of services which this Service overrides.
      /// </summary>
      [ListDrawerSettings(ShowFoldout = false)]
      [HideIf("@serviceOverrides.Length == 0")]
      [ReadOnly, Space] public string[] serviceOverrides;

      /// <summary>
      /// The initialization priority of the service.
      /// </summary>
      [HideInInspector] public int priority;

      /// <summary>
      /// The full name of the service type.
      /// </summary>
      [HideInInspector] public string fullName;

      /// <summary>
      /// An Inspector-friendly description of the service.
      /// </summary>
      [HideInInspector] public string description;

      /// <summary>
      /// The assembly-qualified name of the service type.
      /// </summary>
      [HideInInspector] public string assemblyQualifiedName;

      public readonly Type Type => Type.GetType(assemblyQualifiedName);

      public ServiceInfo(Type type, bool enabled) {
        assemblyQualifiedName = type.AssemblyQualifiedName;
        fullName = type.FullName;
        overridden = false;

        var attr = type.GetCustomAttribute<ServiceInfoAttribute>();
        required = attr?.Required ?? false;
        priority = attr?.Priority ?? 0;

        // Set the enabled flag
        this.enabled = enabled || required;

        // Build an inspector friendly description
        description = attr?.DisplayName ?? type.Name;
        if (required) description = $"[!] {description}";
        description = $"{priority}\t{description}";

        // Build the Service Overrides array
        serviceOverrides = type
          .GetCustomAttributes<ServiceOverridesAttribute>()
          .SelectMany(attr => attr.ServiceTypes)
          .Select(t => t.FullName)
          .ToArray();
      }
    }

    /// <summary>
    /// List of ServiceInfo for all available Service types.
    /// </summary>
    [SerializeField]
    [LabelText("@$property.Parent.NiceName")]
    [ValidateInput("OnValidateServiceList")]
    [DisableIf("@Engine.IsInitialized")]
    [ListDrawerSettings(
      ShowFoldout = false, IsReadOnly = true, ListElementLabelName = "description",
      ElementColor = "GetServiceColor", OnTitleBarGUI = "DrawServiceListUI"
    )]
    private List<ServiceInfo> serviceList = new();

    /// <summary>
    /// All Service Types
    /// </summary>
    public IEnumerable<Type> AllServiceTypes => serviceList
      .Select(s => s.Type);

    /// <summary>
    /// All Service Types which are enabled and not overriden
    /// </summary>
    public IEnumerable<Type> EnabledServiceTypes => serviceList
      .Where(service => service.enabled && !service.overridden)
      .Select(s => s.Type);

    /// <summary>
    /// All Service types which are overriden by an enabled Service
    /// </summary>
    public IEnumerable<Type> OverriddenServiceTypes => EnabledServiceTypes
      .SelectMany(type => type.GetCustomAttributes<ServiceOverridesAttribute>())
      .SelectMany(attr => attr.ServiceTypes);

#if UNITY_EDITOR
    // Guard Flags
    private int enabledCount = -1;
    private bool isRefreshing = false;

    // UI Colors
    private static Color enabledColor = new(.2f, 0.4f, .2f);
    private static Color requiredColor = new(.2f, .2f, 0.4f);
    private static Color overriddenColor = new(0.4f, .2f, .2f);


    /// <returns>
    /// The background color to use for the service list item at the given index.
    /// </returns>
    private Color GetServiceColor(int index, Color defaultColor) {
      if (index >= serviceList.Count) return defaultColor;
      var serviceInfo = serviceList[index];
      if (serviceInfo.overridden) return overriddenColor;
      if (serviceInfo.required) return requiredColor;
      if (serviceInfo.enabled) return enabledColor;
      return defaultColor;
    }

    /// <summary>
    /// Adds a refresh button to the service list toolbar.
    /// </summary>
    private void DrawServiceListUI() {
      if (!SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh)) return;
      RefreshServiceList();
    }

    /// <summary>
    /// Validates changes to the service list.
    /// </summary>
    private bool OnValidateServiceList() {
      OnValidate();
      return true;
    }

    /// <summary>
    /// Ensures that required and overridden services are marked correctly.
    /// </summary>
    private void OnValidate() {
      if (serviceList == null || serviceList.Count == 0) {
        serviceList ??= new();
        RefreshServiceList();
      }

      // Guard to prevent updating every validation
      if (enabledCount == serviceList.Count(s => s.enabled)) return;

      var overriddenTypes = OverriddenServiceTypes;
      for (int i = 0; i < serviceList.Count; i++) {
        var service = serviceList[i];

        service.enabled = service.enabled || service.required;
        service.overridden = overriddenTypes.Contains(service.Type);

        serviceList[i] = service;
      }

      enabledCount = serviceList.Count(s => s.enabled);
    }

    /// <summary>
    /// Rebuilds the service list.
    /// </summary>
    private void RefreshServiceList() {
      if (isRefreshing) return;
      isRefreshing = true;

      var serviceTypes = ReflectionUtils.GetAllTypes().ConcretionsOf<IService>();
      var serviceInfo = new List<ServiceInfo>();

      foreach (var serviceType in serviceTypes) {
        // Skip abstract classes and interfaces
        if (serviceType.IsAbstract || serviceType.IsInterface) continue;

        var enabled = serviceList.FirstOrDefault(s => s.Type == serviceType).enabled;
        serviceInfo.Add(new ServiceInfo(serviceType, enabled));
      }

      // Sort by description, then required, then init order
      // This minimizes the Topological Sort at initialization
      serviceInfo = serviceInfo
        .OrderBy(s => s.description)
        .OrderByDescending(s => s.priority)
        .OrderByDescending(s => s.required)
        .ToList();

      // Update the service list
      serviceList.Clear();
      serviceList.AddRange(serviceInfo);

      enabledCount = -1;
      OnValidate();

      isRefreshing = false;
    }

    /// <summary>
    /// Rebuilds the service list when scripts are reloaded.
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
      EngineConfig.Instance.ServiceList?.RefreshServiceList();
    }
#endif
  }

}