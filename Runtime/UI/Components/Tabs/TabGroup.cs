using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shadowpaw.UI {
  public class TabGroup : MonoBehaviour {
    [field: SerializeField] public TabButton Selected { get; private set; }
    public int SelectedIndex => GetIndex(Selected);

    [field: SerializeField] public TabButton HoverTarget { get; private set; }
    public int HoverIndex => GetIndex(HoverTarget);

    [SerializeField] private List<TabButton> tabs = new();

    private void OnEnable() {
      // Force selected events to fire
      var temp = Selected != null ? Selected : tabs.FirstOrDefault();
      Selected = null;
      SetSelected(temp);
    }

    private void UpdateTabs() {
      foreach (var tab in tabs) {
        if (tab == null || tab.Image == null) continue;
        if (tab == Selected) {
          tab.Image.sprite = tab.SelectedSprite != null ? tab.SelectedSprite : tab.NormalSprite;
          tab.Image.color = tab.SelectedColor;
        } else if (tab == HoverTarget) {
          tab.Image.sprite = tab.HoverSprite != null ? tab.HoverSprite : tab.NormalSprite;
          tab.Image.color = tab.HoverColor;
        } else {
          tab.Image.sprite = tab.NormalSprite;
          tab.Image.color = tab.NormalColor;
        }
      }
    }

    public void Subscribe(TabButton tab) {
      if (!tabs.Contains(tab)) tabs.Add(tab);
    }

    public void Unsubscribe(TabButton tab) {
      tabs.Remove(tab);

      if (tab == Selected) ClearSelected();
      if (tab == HoverTarget) ClearHoverTarget();
    }

    public int GetIndex(TabButton tab) {
      return tabs.IndexOf(tab);
    }

    public int NextIndex(int index) {
      for (int i = 0; i < tabs.Count; i++) {
        var tab = GetButton(index++);
        if (tab != null && tab.gameObject.activeInHierarchy) {
          break;
        }
      }
      return index;
    }

    public int PreviousIndex(int index) {
      for (int i = 0; i < tabs.Count; i++) {
        var tab = GetButton(index--);
        if (tab != null && tab.gameObject.activeInHierarchy) {
          break;
        }
      }
      return index;
    }

    public TabButton GetButton(int index) {
      if (index < 0) return GetButton(tabs.Count + index);
      return tabs[index % tabs.Count];
    }

    #region Selected

    public void ClearSelected()
      => SetSelected(null);

    public void SetSelected(TabButton tab) {
      if (Selected == tab) return;

      if (Selected != null && Selected.revealTarget != null) {
        Selected.revealTarget.SetActive(false);
      }

      Selected = tab;
      UpdateTabs();

      if (Selected != null && Selected.revealTarget != null) {
        Selected.revealTarget.SetActive(true);
      }
    }

    public void SetSelected(int index)
      => SetSelected(GetButton(index));

    public void SelectNext()
      => SetSelected(NextIndex(SelectedIndex));

    public void SelectPrevious()
      => SetSelected(PreviousIndex(SelectedIndex));

    public void SelectHovered()
      => SetSelected(HoverTarget != null ? HoverTarget : Selected);

    #endregion

    #region Hover Target

    public void ClearHoverTarget()
      => SetHoverTarget(null);

    public void SetHoverTarget(TabButton tab) {
      HoverTarget = tab;
      UpdateTabs();
    }

    public void SetHoverTarget(int index)
      => SetHoverTarget(GetButton(index));

    public void HoverNext()
      => SetHoverTarget(NextIndex(HoverIndex));

    public void HoverPrevious()
      => SetHoverTarget(PreviousIndex(HoverIndex));

    #endregion
  }
}

