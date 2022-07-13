using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    #region Members
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }

    public FitType fitType;
    public int rows, columns;
    public Vector2 cellSize, spacing;
    public bool fitX, fitY;
    #endregion

    #region CalculateLayoutInputHorizontal
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        if (fitType == FitType.Uniform || fitType == FitType.Width || fitType == FitType.Height)
        {
            fitX = fitY = true;
            float sqrtValue = Mathf.Sqrt(transform.childCount);
            rows = columns = Mathf.CeilToInt(sqrtValue);
        }
        if (fitType == FitType.Width || fitType == FitType.FixedColumns)
        {
            rows = Mathf.CeilToInt(transform.childCount / columns);
        }
        if (fitType == FitType.Height || fitType == FitType.FixedRows)
        {
            columns = Mathf.CeilToInt(transform.childCount / rows);
        }
        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;
        if (columns != 0 && rows != 0)
        {
            float cellWidth = parentWidth / columns - spacing.x / columns * 2 - padding.left / columns - padding.right / columns;
            float cellHeight = parentHeight / rows - spacing.y / rows * 2 - padding.top / rows - padding.bottom / rows;
            cellSize.x = fitX ? cellWidth : cellSize.x;
            cellSize.y = fitY ? cellHeight : cellSize.y;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                int rowCount = i / columns;
                int columnCount = i % columns;
                var item = rectChildren[i];
                var xPos = cellSize.x * columnCount + spacing.x * columnCount + padding.left;
                var yPos = cellSize.y * rowCount + spacing.y * rowCount + padding.top;
                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }
        }
    }
    #endregion

    #region UnusedLayoutMethods
    public override void CalculateLayoutInputVertical()
    {

    }

    public override void SetLayoutHorizontal()
    {

    }

    public override void SetLayoutVertical()
    {

    }
    #endregion
}