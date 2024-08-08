using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shadowpaw.UI {
  [RequireComponent(typeof(Image))]
  public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    #region Inspector Fields

    [SerializeField, Required]
    [Tooltip("The tab group that this tab button belongs to.")]
    private TabGroup _tabGroup;
    public TabGroup TabGroup {
      get => _tabGroup;
      set {
        if (_tabGroup != null) _tabGroup.Unsubscribe(this);
        _tabGroup = value;
        if (_tabGroup != null) _tabGroup.Subscribe(this);
      }
    }

    [Tooltip("The target panel (or object) to reveal when this tab is selected.")]
    public GameObject revealTarget;

    public Image Image { get; private set; }

    [TabGroup("Settings", "Colors")]
    [Tooltip("The image color to use when the tab is not selected.")]
    public Color NormalColor = Color.white;

    [TabGroup("Settings", "Colors")]
    [Tooltip("The image color to use when the tab is in a hover state.")]
    public Color HoverColor = Color.yellow;

    [TabGroup("Settings", "Colors")]
    [Tooltip("The image color to use when the tab is selected.")]
    public Color SelectedColor = Color.blue;

    [TabGroup("Settings", "Sprites")]
    [Tooltip("The sprite to display when the tab is not selected.")]
    public Sprite NormalSprite;

    [TabGroup("Settings", "Sprites")]
    [Tooltip("The sprite to display when the tab is in a hover state.")]
    public Sprite HoverSprite;

    [TabGroup("Settings", "Sprites")]
    [Tooltip("The sprite to display when the tab is selected.")]
    public Sprite SelectedSprite;

    #endregion

    private void Awake() {
      TabGroup.Subscribe(this);

      Image = GetComponent<Image>();
      if (NormalSprite == null) NormalSprite = Image.sprite;

      Image.sprite = NormalSprite;
      Image.color = NormalColor;
    }

    public void OnPointerEnter(PointerEventData eventData) {
      TabGroup.SetHoverTarget(this);
    }

    public void OnPointerExit(PointerEventData eventData) {
      TabGroup.ClearHoverTarget();
    }

    public void OnPointerClick(PointerEventData eventData) {
      TabGroup.SetSelected(this);
    }
  }
}

