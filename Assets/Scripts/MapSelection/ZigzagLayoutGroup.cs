using UnityEngine;
using UnityEngine.UI;


/* ZIGZAG LAYOUT GROUP
 * 
 * Purpose: Arranges UI buttons in a zigzag pattern (like a snake/S-shape on the screen)
 * 
 * How it works:
 * - First row: buttons go left to right
 * - Second row: buttons go right to left
 * - Third row: back to left to right
 * - Keeps alternating for each row
 * 
 * Integration: Used in level select screen to create a path-like layout for map buttons
 * 
 * Example:
 * Normal grid:     Zigzag pattern:
 * 1  2  3  4       1 → 2 → 3 → 4
 * 5  6  7  8       8 ← 7 ← 6 ← 5
 * 9  10 11 12      9 → 10→ 11→ 12
 */


/// <summary>
/// Automatically arranges UI elements in a zigzag pattern (alternating left-to-right, right-to-left rows)
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Creates a natural progression path for level select screens (like following a road)</para>
/// <para><strong>How:</strong> Uses Unity's LayoutGroup system to position children in alternating row directions</para>
/// </remarks>
public class ZigzagLayoutGroup : LayoutGroup
{
    /// <summary>
    /// How big each button/cell should be (width and height in pixels)
    /// </summary>
    public Vector2 cellSize = new Vector2(100f, 100f);
    
    /// <summary>
    /// Gap between buttons (horizontal and vertical spacing in pixels)
    /// </summary>
    public Vector2 spacing = new Vector2(5f, 5f);
    
    /// <summary>
    /// How many buttons to fit in each row before going to the next row
    /// </summary>
    public int columns = 4;
    
    /// <summary>
    /// Should the first row go left-to-right? (If false, starts right-to-left)
    /// </summary>
    public bool startLeftToRight = true;


    /// <summary>
    /// Calculates how wide the layout needs to be to fit all columns
    /// </summary>
    /// <remarks>
    /// <para><strong>How:</strong> Width = (number of columns × cell width) + (gaps between them) + padding</para>
    /// </remarks>
    public override void CalculateLayoutInputHorizontal()
    {
        // Let Unity do the basic setup first
        base.CalculateLayoutInputHorizontal();

        int colCount = columns;
        
        // Calculate total width needed:
        // (4 columns × 100px each) + (3 gaps × 5px each) + left/right padding
        float realWidth = (colCount * cellSize.x) + ((colCount - 1) * spacing.x) + padding.left + padding.right;

        // Tell Unity this is how much horizontal space we need
        SetLayoutInputForAxis(realWidth, realWidth, -1, 0);
    }


    /// <summary>
    /// Calculates how tall the layout needs to be to fit all rows
    /// </summary>
    /// <remarks>
    /// <para><strong>How:</strong> Height = (number of rows × cell height) + (gaps between rows) + padding</para>
    /// </remarks>
    public override void CalculateLayoutInputVertical()
    {
        // Figure out how many rows we need based on total children and columns per row
        // Example: 10 children ÷ 4 columns = 2.5 rows → rounds up to 3 rows
        int rowCount = Mathf.CeilToInt(rectChildren.Count / (float)columns);
        
        // Calculate total height needed:
        // (3 rows × 100px each) + (2 gaps × 5px each) + top/bottom padding
        float realHeight = (rowCount * cellSize.y) + ((rowCount - 1) * spacing.y) + padding.top + padding.bottom;

        // Tell Unity this is how much vertical space we need
        SetLayoutInputForAxis(realHeight, realHeight, -1, 1);
    }


    /// <summary>
    /// Applies the horizontal positioning to all children (called by Unity)
    /// </summary>
    public override void SetLayoutHorizontal()
    {
        // Position children on the horizontal axis (x-axis)
        SetChildrenAlongAxis(0);
    }


    /// <summary>
    /// Applies the vertical positioning to all children (called by Unity)
    /// </summary>
    public override void SetLayoutVertical()
    {
        // Position children on the vertical axis (y-axis)
        SetChildrenAlongAxis(1);
    }


    /// <summary>
    /// Positions all child UI elements in the zigzag pattern
    /// </summary>
    /// <param name="axis">0 = horizontal (x), 1 = vertical (y)</param>
    private void SetChildrenAlongAxis(int axis)
    {
        // Go through each child button and position it
        for (int i = 0; i < rectChildren.Count; i++)
        {
            // Figure out which row this button is in
            // Example: button 5 ÷ 4 columns = row 1 (second row)
            int rowIndex = i / columns;
            
            // Figure out which column this button is in
            // Example: button 5 % 4 columns = column 1 (second column)
            int columnIndex = i % columns;

            // Decide if this row goes left-to-right or right-to-left
            // Even rows (0, 2, 4...) follow startLeftToRight
            // Odd rows (1, 3, 5...) do the opposite
            bool isLeftToRight = (rowIndex % 2 == 0) ? startLeftToRight : !startLeftToRight;
            
            // If this row goes right-to-left, flip the column position
            // Example: column 0 becomes column 3, column 1 becomes column 2, etc.
            if (!isLeftToRight)
            {
                columnIndex = columns - 1 - columnIndex;
            }

            // Get the actual button we're positioning
            RectTransform child = rectChildren[i];

            // Calculate the exact pixel position for this button
            // X position = left padding + (cell width + spacing) × which column
            float xPos = padding.left + (cellSize.x + spacing.x) * columnIndex;
            
            // Y position = top padding + (cell height + spacing) × which row
            float yPos = padding.top + (cellSize.y + spacing.y) * rowIndex;

            // Actually move the button to this position
            SetChildAlongAxis(child, 0, xPos, cellSize.x);  // Set X position and width
            SetChildAlongAxis(child, 1, yPos, cellSize.y);  // Set Y position and height
        }
    }
}