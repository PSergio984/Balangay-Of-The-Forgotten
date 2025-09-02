using UnityEngine;

/* ARROW VIEW ARCHITECTURE
 * 
 * Why this exists: Provides visual feedback for targeting actions by showing direction and intent
 * 
 * Design reasoning:
 * - Separates visual arrow into head and body for better control and positioning
 * - Uses LineRenderer for body to avoid complex sprite stretching calculations
 * - Updates every frame to provide responsive real-time feedback
 * - Prevents visual overlap between line and arrowhead for cleaner appearance
 * 
 * System integration: Visual component for targeting systems and player input feedback
 */

/// <summary>
/// Visual arrow that follows mouse cursor to show targeting direction and intent
/// </summary>
/// <remarks>
/// <para><strong>Design purpose:</strong> Separates arrow into controllable head and body components</para>
/// 
/// <para><strong>Why this approach:</strong> Rather than using a single stretched sprite, 
/// this splits the arrow into a rotatable head and a flexible line body. This avoids 
/// complex sprite stretching math and gives better visual control. The line can be 
/// any length while the head stays properly oriented and positioned.</para>
/// 
/// <para><strong>Visual benefits:</strong></para>
/// <list type="bullet">
/// <item>Line body can stretch to any length without distortion</item>
/// <item>Arrowhead stays crisp and properly oriented at any distance</item>
/// <item>No overlap between line and head creates clean visual separation</item>
/// <item>Real-time updates provide immediate feedback for player targeting</item>
/// </list>
/// 
/// <para><strong>Usage pattern:</strong> Setup start point → Update continuously → Follow mouse</para>
/// </remarks>
public class ArrowView : MonoBehaviour
{
    /// <summary>
    /// The pointed tip part of the arrow that shows direction
    /// </summary>
    /// <remarks>
    /// Separate GameObject allows independent rotation and positioning.
    /// Stays at mouse position to show exactly where player is targeting.
    /// </remarks>
    [SerializeField] private GameObject arrowHead;
    
    /// <summary>
    /// The line body that connects start point to arrowhead
    /// </summary>
    /// <remarks>
    /// LineRenderer chosen over sprite stretching to avoid distortion math.
    /// Can extend to any length while maintaining consistent thickness.
    /// </remarks>
    [SerializeField] private LineRenderer lineRenderer;
    
    /// <summary>
    /// Where the arrow begins (typically the source of the targeting action)
    /// </summary>
    /// <remarks>
    /// Stored to avoid recalculating the fixed start point every frame.
    /// Set once when arrow is created, then used for all direction calculations.
    /// </remarks>
    private Vector3 startPosition;

    /// <summary>
    /// Configures LineRenderer to avoid common Unity setup problems
    /// </summary>
    /// <remarks>
    /// Called once at startup to fix coordinate system and rendering issues.
    /// Prevents common problems like local space positioning and missing point counts.
    /// High sorting order ensures arrow appears above game elements.
    /// </remarks>
    private void Awake()
    {
        // Prevent coordinate system confusion by forcing world space
        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            // High sorting order ensures visibility above other game elements
            lineRenderer.sortingOrder = 99;
        }
    }

    /// <summary>
    /// Updates arrow position and orientation to follow mouse cursor in real-time
    /// </summary>
    /// <remarks>
    /// Called every frame to provide immediate visual feedback for targeting.
    /// Order of operations matters: direction calculation must happen before positioning
    /// to avoid circular dependencies between arrowhead position and direction calculation.
    /// </remarks>
    private void Update()
    {
        Vector3 endPosition = MouseUtil.GetMousePositionInWorldSpace();
        
        // Direction calculated from fixed points to avoid circular dependency
        Vector3 direction = (endPosition - startPosition).normalized;
        
        // Arrowhead positioned at exact mouse location for precise targeting feedback
        arrowHead.transform.position = endPosition;
        arrowHead.transform.right = direction;
        
        // Line stops short of arrowhead to create visual separation and avoid overlap
        Vector3 lineEndPosition = endPosition - direction * 0.5f;
        
        // Both line points updated to ensure consistent coordinate system
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, lineEndPosition);
    }

    /// <summary>
    /// Initializes arrow with starting position for targeting visualization
    /// </summary>
    /// <param name="startPosition">Point where the arrow begins (source of targeting action)</param>
    /// <remarks>
    /// Called once when targeting begins to establish the arrow's origin point.
    /// Sets initial line positions to provide immediate visual feedback before mouse movement.
    /// Starting position typically comes from character position, card location, or UI element.
    /// </remarks>
    public void SetupArrow(Vector3 startPosition)
    {
        // Store origin point for all future direction calculations
        this.startPosition = startPosition;
        // Provide immediate visual by setting initial line positions
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, MouseUtil.GetMousePositionInWorldSpace());
    }
}
