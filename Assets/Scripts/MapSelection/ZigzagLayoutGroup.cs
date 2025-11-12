using UnityEngine;
using UnityEngine.UI;

public class ZigzagLayoutGroup : LayoutGroup
{
    public Vector2 cellSize = new Vector2(100f, 100f);
    public Vector2 spacing = new Vector2(5f, 5f);
    public int columns = 4;
    public bool startLeftToRight = true;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        int rowCount = Mathf.CeilToInt(rectChildren.Count / (float)columns);
        float realHeight = (rowCount * cellSize.y) + ((rowCount - 1) * spacing.y) + padding.top + padding.bottom;

        SetLayoutInputForAxis(realHeight, realHeight, -1, 1);
    }

    public override void CalculateLayoutInputVertical()
    {
        int rowCount = Mathf.CeilToInt(rectChildren.Count / (float)columns);
        float realHeight = (rowCount * cellSize.y) + ((rowCount - 1) * spacing.y) + padding.top + padding.bottom;

        SetLayoutInputForAxis(realHeight, realHeight, -1, 1);
    }

    public override void SetLayoutHorizontal()
    {
        SetChildrenAlongAxis(0);
    }

    public override void SetLayoutVertical()
    {
        SetChildrenAlongAxis(1);
    }

    private void SetChildrenAlongAxis(int axis)
    {
        for (int i = 0; i < rectChildren.Count; i++)
        {
            int rowIndex = i / columns;
            int columnIndex = i % columns;

            // Flip column index if on odd-numbered row
            bool isLeftToRight = (rowIndex % 2 == 0) ? startLeftToRight : !startLeftToRight;
            if (!isLeftToRight)
            {
                columnIndex = columns - 1 - columnIndex;
            }

            RectTransform child = rectChildren[i];

            float xPos = padding.left + (cellSize.x + spacing.x) * columnIndex;
            float yPos = padding.top + (cellSize.y + spacing.y) * rowIndex;

            SetChildAlongAxis(child, 0, xPos, cellSize.x);
            SetChildAlongAxis(child, 1, yPos, cellSize.y);
        }
    }
}