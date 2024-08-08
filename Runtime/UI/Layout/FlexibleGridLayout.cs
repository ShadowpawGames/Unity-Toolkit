using System;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Shadowpaw.UI {
  /// <summary>
  /// Provides a flexible grid layout for Unity UI elements.
  /// </summary>
  /// <remarks>
  /// Based on a tutorial by GameDevGuide: https://youtu.be/CGsEJToeXmA
  /// </remarks>
  [AddComponentMenu("Shadowpaw/UI/Flexible Grid Layout")]
  public class FlexibleGridLayout : LayoutGroup {
    /// <summary>
    /// The direction in which the layout flows.
    /// </summary>
    public enum Direction {
      Horizontal,
      Vertical
    }

    /// <summary>
    /// The type of fit to apply to the grid layout.
    /// This determines how the grid layout will fit its children.
    /// </summary>
    public enum FitType {
      /// <summary>
      /// Children will be resized to fit the grid layout uniformly.
      /// </summary>
      Uniform,

      /// <summary>
      /// The layout will have a fixed number of rows.
      /// </summary>
      Rows,
      /// <summary>
      /// The layout will have a fixed number of columns.
      /// </summary>
      Columns,
      /// <summary>
      /// The layout will have a fixed number of rows and columns.
      /// Some elements may spill out of the layout.
      /// </summary>
      RowsAndColumns,

      /// <summary>
      /// Children will have a fixed width.
      /// </summary>
      Width,
      /// <summary>
      /// Children will have a fixed height.
      /// </summary>
      Height,
      /// <summary>
      /// Children will have a fixed width and height.
      /// Some elements may spill out of the layout.
      /// </summary>
      WidthAndHeight,
    }

    #region Inspector Fields

    [HorizontalGroup("Padding", order: -1)]
    [Tooltip("The amount of space between each element in the grid.")]
    public Vector2Int spacing = Vector2Int.zero;

    [VerticalGroup("Settings")]
    [Tooltip("The direction in which the layout flows.")]
    public Direction direction = Direction.Horizontal;

    [VerticalGroup("Settings")]
    [Tooltip("The type of fit to apply to the grid layout.")]
    public FitType fitType = FitType.Uniform;

    [BoxGroup("Layout Info", ShowLabel = false)]

    [HorizontalGroup("Layout Info/LayoutSize")]
    [Tooltip("The number of rows in the group.")]
    [EnableIf("@IsFitType(FitType.Rows)")]
    [Min(1), Delayed] public int rows = 0;

    [LabelText("Columns")]
    [HorizontalGroup("Layout Info/LayoutSize")]
    [Tooltip("The number of rows in the group.")]
    [EnableIf("@IsFitType(FitType.Columns)")]
    [Min(1), Delayed] public int cols = 0;

    [HorizontalGroup("Layout Info/CellSize")]
    [Tooltip("The width of each element in the group.")]
    [EnableIf("@IsFitType(FitType.Width)")]
    [Min(1), Delayed] public int width = 0;

    [HorizontalGroup("Layout Info/CellSize")]
    [Tooltip("The height of each element in the group.")]
    [EnableIf("@IsFitType(FitType.Height)")]
    [Min(1), Delayed] public int height = 0;

    [HorizontalGroup("Toggles")]
    [Tooltip("Resizes children to fit the layout. If disabled they will be left at their original size.")]
    [ToggleLeft] public bool resizeChildren = true;

    [HorizontalGroup("Toggles")]
    [Tooltip("Centers children in the final row or column.")]
    [ToggleLeft] public bool centerOverflow = true;

    #endregion

    #region LayoutGroup Overrides

    public override void CalculateLayoutInputHorizontal() {
      base.CalculateLayoutInputHorizontal();
      CalculateLayout();
    }
    public override void CalculateLayoutInputVertical() { }
    public override void SetLayoutHorizontal() { }
    public override void SetLayoutVertical() { }

    #endregion

    /// <returns>
    /// The actual size of the layout area, after padding has been removed.
    /// Includes one additional unit of spacing for each axis, to account for the last element.
    /// </returns>
    private Vector2Int GetLayoutSize() {
      return new Vector2Int(
        Mathf.FloorToInt(rectTransform.rect.width - padding.horizontal + spacing.x),
        Mathf.FloorToInt(rectTransform.rect.height - padding.vertical + spacing.y)
      );
    }

    private void CalculateLayout() {
      int xOffset = 0, yOffset = 0;
      var layoutSize = GetLayoutSize();
      var childCount = rectChildren.Count;

      // Update sizing based on FitType
      switch (fitType) {
        case FitType.Uniform:
          var sqrt = Mathf.Max(1f, Mathf.CeilToInt(Mathf.Sqrt(childCount)));
          rows = cols = Mathf.CeilToInt(sqrt);
          width = Mathf.FloorToInt(layoutSize.x / cols) - spacing.x;
          height = Mathf.FloorToInt(layoutSize.y / rows) - spacing.y;
          break;

        case FitType.Rows:
          rows = Mathf.Max(1, rows);
          cols = Mathf.Max(1, Mathf.CeilToInt((float)childCount / rows));
          width = Mathf.FloorToInt(layoutSize.x / cols) - spacing.x;
          height = Mathf.FloorToInt(layoutSize.y / rows) - spacing.y;
          break;

        case FitType.Columns:
          cols = Mathf.Max(1, cols);
          rows = Mathf.Max(1, Mathf.CeilToInt((float)childCount / cols));
          width = Mathf.FloorToInt(layoutSize.x / cols) - spacing.x;
          height = Mathf.FloorToInt(layoutSize.y / rows) - spacing.y;
          break;

        case FitType.RowsAndColumns:
          rows = Mathf.Max(1, rows);
          cols = Mathf.Max(1, cols);
          width = Mathf.Max(1, Mathf.FloorToInt(layoutSize.x / cols) - spacing.x);
          height = Mathf.Max(1, Mathf.FloorToInt(layoutSize.y / rows) - spacing.y);
          break;

        case FitType.Width:
          width = Mathf.Max(1, width);
          cols = Mathf.Max(1, Mathf.CeilToInt(layoutSize.x / (width + spacing.x)));
          rows = Mathf.Max(1, Mathf.CeilToInt((float)childCount / cols));
          height = Mathf.Max(1, Mathf.FloorToInt(layoutSize.y / rows) - spacing.y);
          xOffset = (layoutSize.x - (width + spacing.x) * cols) / 2;
          break;

        case FitType.Height:
          height = Mathf.Max(1, height);
          rows = Mathf.Max(1, Mathf.CeilToInt(layoutSize.y / (height + spacing.y)));
          cols = Mathf.Max(1, Mathf.CeilToInt((float)childCount / rows));
          width = Mathf.Max(1, Mathf.FloorToInt(layoutSize.x / cols) - spacing.x);
          yOffset = (layoutSize.y - (height + spacing.y) * rows) / 2;
          break;

        case FitType.WidthAndHeight:
          width = Mathf.Max(1, width);
          height = Mathf.Max(1, height);
          cols = Mathf.Max(1, Mathf.CeilToInt(layoutSize.x / (width + spacing.x)));
          rows = Mathf.Max(1, Mathf.CeilToInt(layoutSize.y / (height + spacing.y)));
          break;
      }

      // Get limits based on Direction
      int maxRow = 0, maxCol = 0;
      switch (direction) {
        case Direction.Horizontal:
          maxRow = childCount / cols;
          maxCol = childCount % cols;
          break;
        case Direction.Vertical:
          maxRow = childCount % rows;
          maxCol = childCount / rows;
          break;
      }

      for (int i = 0; i < rectChildren.Count; i++) {
        int row = 0, col = 0, colOffset = 0, rowOffset = 0;

        // Calculate row, column, and offsets based on Direction
        switch (direction) {
          case Direction.Horizontal:
            row = i / cols;
            col = i % cols;
            if (centerOverflow && row >= maxRow) {
              colOffset = (width + spacing.x) * (cols - maxCol) / 2;
            }
            break;
          case Direction.Vertical:
            row = i % rows;
            col = i / rows;
            if (centerOverflow && col >= maxCol) {
              rowOffset = (height + spacing.y) * (rows - maxRow) / 2;
            }
            break;
        }

        // Calculate position based on row and column
        var x = col * (width + spacing.x) + padding.left + xOffset;
        var y = row * (height + spacing.y) + padding.top + yOffset;

        // Apply offsets for centering overflow
        if (centerOverflow && row >= maxRow) x += colOffset;
        if (centerOverflow && col >= maxCol) y += rowOffset;

        // Set the child's position and size
        SetChild(rectChildren[i], x, y, width, height);
      }
    }

    private void SetChild(RectTransform child, int x, int y, int width, int height) {
      if (resizeChildren) {
        // Set the child's position and size
        SetChildAlongAxis(child, 0, x, width);
        SetChildAlongAxis(child, 1, y, height);
        return;
      }

      // Update X position based on child alignment
      switch (childAlignment) {
        case TextAnchor.UpperCenter:
        case TextAnchor.MiddleCenter:
        case TextAnchor.LowerCenter:
          x += Mathf.FloorToInt((width - child.rect.width) / 2);
          break;
        case TextAnchor.UpperRight:
        case TextAnchor.MiddleRight:
        case TextAnchor.LowerRight:
          x += Mathf.FloorToInt(width - child.rect.width);
          break;
      }

      // Update Y position based on child alignment
      switch (childAlignment) {
        case TextAnchor.MiddleLeft:
        case TextAnchor.MiddleCenter:
        case TextAnchor.MiddleRight:
          y += Mathf.FloorToInt((height - child.rect.height) / 2);
          break;
        case TextAnchor.LowerLeft:
        case TextAnchor.LowerCenter:
        case TextAnchor.LowerRight:
          y += Mathf.FloorToInt(height - child.rect.height);
          break;
      }

      // Set the child's position without resizing it
      SetChildAlongAxis(child, 0, x);
      SetChildAlongAxis(child, 1, y);
    }

    /// <summary>
    /// Determine if a fit type is one of the specified types.
    /// Used to enable/disable inspector fields based on the fit type.
    /// </summary>
    private bool IsFitType(FitType type) {
      if (fitType == type) return true;
      return type switch {
        FitType.Width => fitType == FitType.WidthAndHeight,
        FitType.Height => fitType == FitType.WidthAndHeight,
        FitType.Rows => fitType == FitType.RowsAndColumns,
        FitType.Columns => fitType == FitType.RowsAndColumns,
        _ => false,
      };
    }

  }
}