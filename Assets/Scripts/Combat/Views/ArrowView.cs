using UnityEngine;
using System.Collections.Generic;

/* ARROW VIEW ARCHITECTURE
 * 
 * Why this exists: Provides visual feedback for targeting actions by showing direction and intent
 * 
 * Design reasoning:
 * - Supports both straight line and Bezier curve modes for visual variety
 * - Bezier mode uses cubic Bezier curves for smooth, curved arrows like card games
 * - Separates visual arrow into head and body for better control and positioning
 * - Updates every frame to provide responsive real-time feedback
 * - Prevents visual overlap between line and arrowhead for cleaner appearance
 * 
 * System integration: Visual component for targeting systems and player input feedback
 */

/// <summary>
/// Visual arrow that follows mouse cursor to show targeting direction and intent.
/// Supports both straight line and Bezier curve modes.
/// </summary>
/// <remarks>
/// <para><strong>Design purpose:</strong> Separates arrow into controllable head and body components</para>
/// 
/// <para><strong>Why this approach:</strong> Rather than using a single stretched sprite, 
/// this splits the arrow into a rotatable head and a flexible line body. This avoids 
/// complex sprite stretching math and gives better visual control. The line can be 
/// any length while the head stays properly oriented and positioned.</para>
/// 
/// <para><strong>Bezier Mode:</strong> Uses cubic Bezier curves with multiple nodes for 
/// a smooth, curved arrow effect commonly seen in card games like Slay the Spire.</para>
/// 
/// <para><strong>Visual benefits:</strong></para>
/// <list type="bullet">
/// <item>Line body can stretch to any length without distortion</item>
/// <item>Arrowhead stays crisp and properly oriented at any distance</item>
/// <item>Bezier curves provide smooth, visually appealing targeting feedback</item>
/// <item>Real-time updates provide immediate feedback for player targeting</item>
/// </list>
/// 
/// <para><strong>Usage pattern:</strong> Setup start point → Update continuously → Follow mouse</para>
/// </remarks>
public class ArrowView : MonoBehaviour
{
    #region Serialized Fields
    [Header("Arrow Mode")]
    [Tooltip("Enable Bezier curve mode for smooth curved arrows")]
    [SerializeField] private bool useBezierMode = true;
    
    [Header("Straight Line Mode (Legacy)")]
    /// <summary>
    /// The pointed tip part of the arrow that shows direction (used in straight line mode)
    /// </summary>
    [SerializeField] private GameObject arrowHead;
    
    /// <summary>
    /// The line body that connects start point to arrowhead (used in straight line mode)
    /// </summary>
    [SerializeField] private LineRenderer lineRenderer;
    
    [Header("Bezier Mode Settings")]
    [Tooltip("The prefab of arrow head for Bezier mode")]
    [SerializeField] private GameObject arrowHeadPrefab;
    
    [Tooltip("The prefab of arrow node for Bezier mode")]
    [SerializeField] private GameObject arrowNodePrefab;
    
    [Tooltip("The number of arrow nodes in the Bezier curve (more = denser chain)")]
    [SerializeField] private int arrowNodeNum = 15;
    
    [Tooltip("The base scale multiplier for arrow nodes (increase for larger chain links)")]
    [SerializeField] private float scaleFactor = 0.3f;
    
    [Tooltip("Additional rotation offset for chain link sprites (degrees)")]
    [SerializeField] private float rotationOffset = 0f;
    
    [Tooltip("Use linear spacing instead of logarithmic (better for chain effect)")]
    [SerializeField] private bool useLinearSpacing = true;
    
    [Tooltip("Distance between chain nodes in world units (smaller = tighter chain)")]
    [SerializeField] private float nodeSpacing = 0.15f;
    
    [Tooltip("Use dynamic node count based on curve length (recommended for chain effect)")]
    [SerializeField] private bool useDynamicNodeCount = true;
    
    [Tooltip("Additional rotation offset for the arrow head sprite (degrees). Adjust based on sprite orientation.")]
    [SerializeField] private float arrowHeadRotationOffset = -90f;
    #endregion
    
    #region Private Fields
    /// <summary>
    /// Where the arrow begins (typically the source of the targeting action)
    /// </summary>
    private Vector3 startPosition;
    
    // Bezier mode fields
    /// <summary>
    /// The list of arrow nodes' transforms for Bezier mode.
    /// </summary>
    private List<Transform> bezierArrowNodes = new List<Transform>();
    
    /// <summary>
    /// The list of control points for the cubic Bezier curve.
    /// P0 = start, P1 = control1, P2 = control2, P3 = end (mouse position)
    /// </summary>
    private Vector3[] controlPoints = new Vector3[4];
    
    /// <summary>
    /// The factors to determine the position of control points P1, P2.
    /// These create the characteristic curve shape.
    /// </summary>
    private readonly Vector2[] controlPointFactors = new Vector2[] 
    { 
        new Vector2(-0.3f, 0.8f),  // P1 factor
        new Vector2(0.1f, 1.4f)    // P2 factor
    };
    
    /// <summary>
    /// Whether the Bezier arrow nodes have been initialized.
    /// </summary>
    private bool bezierInitialized = false;
    
    /// <summary>
    /// Maximum number of nodes in the pool for dynamic node count mode.
    /// </summary>
    private const int MAX_NODE_POOL = 50;
    
    /// <summary>
    /// Reference to the arrow head transform (last element in pool).
    /// </summary>
    private Transform arrowHeadTransform;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Configures arrow components based on selected mode
    /// </summary>
    private void Awake()
    {
        if (useBezierMode)
        {
            // Hide straight line components if they exist
            HideLegacyComponents();
        }
        else
        {
            // Configure LineRenderer for straight line mode
            if (lineRenderer != null)
            {
                lineRenderer.useWorldSpace = true;
                lineRenderer.positionCount = 2;
                lineRenderer.sortingOrder = 99;
            }
        }
    }
    
    /// <summary>
    /// Hides legacy straight-line mode components (Head, Body children)
    /// </summary>
    private void HideLegacyComponents()
    {
        // Hide the serialized arrowHead reference if assigned
        if (arrowHead != null) arrowHead.SetActive(false);
        
        // Hide lineRenderer if assigned
        if (lineRenderer != null) lineRenderer.enabled = false;
        
        // Also hide any child named "Head" or "Body" that might be legacy components
        // These are the straight-line mode visuals that should be hidden in Bezier mode
        Transform headChild = transform.Find("Head");
        if (headChild != null) headChild.gameObject.SetActive(false);
        
        Transform bodyChild = transform.Find("Body");
        if (bodyChild != null) bodyChild.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Initializes Bezier arrow nodes when the object becomes active
    /// </summary>
    private void OnEnable()
    {
        if (useBezierMode)
        {
            // Ensure legacy components stay hidden when re-enabled
            HideLegacyComponents();
            
            if (!bezierInitialized)
            {
                InitializeBezierNodes();
            }
            ShowBezierNodes(true);
        }
    }
    
    /// <summary>
    /// Hides nodes when disabled to prevent visual artifacts
    /// </summary>
    private void OnDisable()
    {
        if (useBezierMode)
        {
            ShowBezierNodes(false);
        }
    }

    /// <summary>
    /// Updates arrow position and orientation to follow mouse cursor in real-time
    /// </summary>
    private void Update()
    {
        if (useBezierMode)
        {
            UpdateBezierArrow();
        }
        else
        {
            UpdateStraightArrow();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes arrow with starting position for targeting visualization
    /// </summary>
    /// <param name="startPosition">Point where the arrow begins (source of targeting action)</param>
    public void SetupArrow(Vector3 startPosition)
    {
        this.startPosition = startPosition;
        
        if (useBezierMode)
        {
            if (!bezierInitialized)
            {
                InitializeBezierNodes();
            }
            ShowBezierNodes(true);
        }
        else
        {
            // Straight line mode - provide immediate visual
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, MouseUtil.GetMousePositionInWorldSpace());
            }
        }
    }
    #endregion

    #region Bezier Mode Methods
    /// <summary>
    /// Creates the arrow node instances for Bezier mode.
    /// Uses a pool approach for dynamic node count mode.
    /// </summary>
    private void InitializeBezierNodes()
    {
        if (bezierInitialized) return;
        
        // Clear any existing nodes
        foreach (var node in bezierArrowNodes)
        {
            if (node != null) Destroy(node.gameObject);
        }
        bezierArrowNodes.Clear();
        arrowHeadTransform = null;
        
        // Determine how many nodes to create
        int nodeCount = useDynamicNodeCount ? MAX_NODE_POOL : arrowNodeNum;
        
        // Instantiate arrow nodes
        for (int i = 0; i < nodeCount; i++)
        {
            if (arrowNodePrefab != null)
            {
                GameObject node = Instantiate(arrowNodePrefab, transform);
                node.transform.position = new Vector3(-1000, -1000, 0); // Hide initially
                bezierArrowNodes.Add(node.transform);
            }
        }
        
        // Instantiate arrow head (separate from node pool)
        if (arrowHeadPrefab != null)
        {
            GameObject head = Instantiate(arrowHeadPrefab, transform);
            head.transform.position = new Vector3(-1000, -1000, 0); // Hide initially
            arrowHeadTransform = head.transform;
        }
        
        bezierInitialized = true;
    }
    
    /// <summary>
    /// Shows or hides all Bezier arrow nodes
    /// </summary>
    private void ShowBezierNodes(bool show)
    {
        if (!show)
        {
            // Move nodes off-screen to hide them
            foreach (var node in bezierArrowNodes)
            {
                if (node != null)
                {
                    node.position = new Vector3(-1000, -1000, 0);
                }
            }
            // Also hide arrow head
            if (arrowHeadTransform != null)
            {
                arrowHeadTransform.position = new Vector3(-1000, -1000, 0);
            }
        }
    }
    
    /// <summary>
    /// Updates the Bezier curve arrow positions every frame
    /// </summary>
    private void UpdateBezierArrow()
    {
        if (bezierArrowNodes.Count == 0) return;
        
        Vector3 endPosition = MouseUtil.GetMousePositionInWorldSpace();
        
        // P0 is at the start position (arrow origin)
        controlPoints[0] = startPosition;
        
        // P3 is at the mouse position (target)
        controlPoints[3] = endPosition;
        
        // Calculate the direction vector for control point factors
        Vector3 delta = endPosition - startPosition;
        
        // P1, P2 are calculated based on P0, P3 and the control point factors
        // These create the characteristic curved shape
        controlPoints[1] = startPosition + new Vector3(
            delta.x * controlPointFactors[0].x,
            delta.y * controlPointFactors[0].y,
            0
        );
        controlPoints[2] = startPosition + new Vector3(
            delta.x * controlPointFactors[1].x,
            delta.y * controlPointFactors[1].y,
            0
        );
        
        if (useDynamicNodeCount)
        {
            UpdateBezierWithDynamicNodes();
        }
        else
        {
            UpdateBezierWithFixedNodes();
        }
    }
    
    /// <summary>
    /// Updates bezier arrow using dynamic node count based on arc length.
    /// Uses smooth t-parameter distribution to avoid snapping artifacts.
    /// </summary>
    private void UpdateBezierWithDynamicNodes()
    {
        // Estimate curve length using chord approximation
        float curveLength = EstimateBezierLength(controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3]);
        
        // Calculate how many nodes we need based on spacing
        // Use a fixed minimum and smooth transition to avoid popping
        int targetNodeCount = Mathf.Clamp(Mathf.RoundToInt(curveLength / nodeSpacing), 3, bezierArrowNodes.Count);
        
        // Hide unused nodes
        for (int i = targetNodeCount; i < bezierArrowNodes.Count; i++)
        {
            if (bezierArrowNodes[i] != null)
            {
                bezierArrowNodes[i].position = new Vector3(-1000, -1000, 0);
            }
        }
        
        // Position nodes along curve
        Vector3 previousPosition = controlPoints[0];
        
        for (int i = 0; i < targetNodeCount; i++)
        {
            if (bezierArrowNodes[i] == null) continue;
            
            // Distribute nodes along the curve
            // Skip t=0 (the origin point) to avoid the first node sitting on the card
            // Start from a small offset and go to 0.95 (leaving room for arrow head)
            float normalizedIndex = targetNodeCount > 1 ? (float)i / (targetNodeCount - 1) : 0f;
            
            // Map to t range: 0.05 to 0.95 (skips the origin, leaves room for arrow head)
            // This prevents the first chain link from appearing on the card itself
            float t = 0.05f + normalizedIndex * 0.90f;
            
            Vector3 position = CalculateBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3]);
            bezierArrowNodes[i].position = position;
            
            // Calculate rotation based on curve tangent direction
            if (i > 0)
            {
                Vector3 direction = position - previousPosition;
                if (direction.sqrMagnitude > 0.0001f) // Avoid jitter from tiny movements
                {
                    float angle = Vector2.SignedAngle(Vector2.up, new Vector2(direction.x, direction.y));
                    bezierArrowNodes[i].rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
                }
            }
            
            // Uniform scale for all nodes (no perspective shrinking at start)
            float scale = scaleFactor;
            bezierArrowNodes[i].localScale = new Vector3(scale, scale, 1f);
            
            previousPosition = position;
        }
        
        // First node matches second node's rotation for consistency
        if (targetNodeCount > 1 && bezierArrowNodes[0] != null && bezierArrowNodes[1] != null)
        {
            bezierArrowNodes[0].rotation = bezierArrowNodes[1].rotation;
        }
        
        // Position arrow head at the end of the curve with smooth rotation
        UpdateArrowHead(targetNodeCount);
    }
    
    /// <summary>
    /// Cached arrow head rotation for smoothing
    /// </summary>
    private float smoothedArrowHeadAngle = 0f;
    
    /// <summary>
    /// Updates arrow head position and rotation with smoothing to prevent jitter
    /// </summary>
    private void UpdateArrowHead(int activeNodeCount)
    {
        if (arrowHeadTransform == null) return;
        
        // Place arrow head at the end point (mouse position)
        arrowHeadTransform.position = controlPoints[3];
        
        // Calculate target angle from curve tangent at end
        // Use a point slightly before t=1 to get tangent direction
        Vector3 nearEndPoint = CalculateBezierPoint(0.95f, controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3]);
        Vector3 direction = controlPoints[3] - nearEndPoint;
        
        if (direction.sqrMagnitude > 0.001f)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + arrowHeadRotationOffset;
            
            // Smooth the rotation to prevent jitter
            smoothedArrowHeadAngle = Mathf.LerpAngle(smoothedArrowHeadAngle, targetAngle, Time.deltaTime * 15f);
            arrowHeadTransform.rotation = Quaternion.Euler(0, 0, smoothedArrowHeadAngle);
        }
        
        arrowHeadTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }
    
    /// <summary>
    /// Updates bezier arrow using fixed node count (original behavior).
    /// </summary>
    private void UpdateBezierWithFixedNodes()
    {
        Vector3 previousPosition = controlPoints[0];
        int nodeCount = bezierArrowNodes.Count;
        
        for (int i = 0; i < nodeCount; i++)
        {
            if (bezierArrowNodes[i] == null) continue;
            
            // Calculate t parameter for node positioning
            // Distribute nodes evenly from t=0 to t=0.92 (leaving room for arrow head)
            float t;
            float normalizedIndex = nodeCount > 1 ? (float)i / (nodeCount - 1) : 0f;
            
            if (useLinearSpacing)
            {
                // Linear distribution: t goes from 0 to 0.92
                t = normalizedIndex * 0.92f;
            }
            else
            {
                // Logarithmic distribution for denser start
                t = Mathf.Log(normalizedIndex + 1f, 2f) * 0.92f;
            }
            
            Vector3 position = CalculateBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3]);
            bezierArrowNodes[i].position = position;
            
            // Calculate rotation
            if (i > 0)
            {
                Vector3 direction = position - previousPosition;
                float angle = Vector2.SignedAngle(Vector2.up, new Vector2(direction.x, direction.y));
                bezierArrowNodes[i].rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
            }
            
            // Scale with perspective
            float scale = scaleFactor * (1f - 0.03f * (nodeCount - 1 - i));
            bezierArrowNodes[i].localScale = new Vector3(scale, scale, 1f);
            
            previousPosition = position;
        }
        
        // First node's rotation matches second node
        if (nodeCount > 1 && bezierArrowNodes[0] != null && bezierArrowNodes[1] != null)
        {
            bezierArrowNodes[0].rotation = bezierArrowNodes[1].rotation;
        }
        
        // Position arrow head at the end
        if (arrowHeadTransform != null)
        {
            arrowHeadTransform.position = controlPoints[3];
            
            if (nodeCount > 0)
            {
                Vector3 lastNodePos = bezierArrowNodes[nodeCount - 1].position;
                Vector3 direction = controlPoints[3] - lastNodePos;
                
                // Use Atan2 for angle calculation - points along X axis by default
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrowHeadTransform.rotation = Quaternion.Euler(0, 0, angle + arrowHeadRotationOffset);
            }
            
            arrowHeadTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }
    }
    
    /// <summary>
    /// Estimates the length of a cubic Bezier curve using chord approximation.
    /// </summary>
    private float EstimateBezierLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Use 10 segments for approximation
        const int segments = 10;
        float length = 0f;
        Vector3 prevPoint = p0;
        
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 point = CalculateBezierPoint(t, p0, p1, p2, p3);
            length += Vector3.Distance(prevPoint, point);
            prevPoint = point;
        }
        
        return length;
    }
    
    /// <summary>
    /// Calculates a point on a cubic Bezier curve
    /// </summary>
    /// <param name="t">Parameter from 0 to 1 along the curve</param>
    /// <param name="p0">Start point</param>
    /// <param name="p1">First control point</param>
    /// <param name="p2">Second control point</param>
    /// <param name="p3">End point</param>
    /// <returns>Position on the Bezier curve at parameter t</returns>
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        // Cubic Bezier formula
        Vector3 point = uuu * p0;                 // (1-t)^3 * P0
        point += 3f * uu * t * p1;                // 3 * (1-t)^2 * t * P1
        point += 3f * u * tt * p2;                // 3 * (1-t) * t^2 * P2
        point += ttt * p3;                        // t^3 * P3
        
        return point;
    }
    #endregion

    #region Straight Line Mode Methods
    /// <summary>
    /// Updates the straight line arrow (legacy mode)
    /// </summary>
    private void UpdateStraightArrow()
    {
        Vector3 endPosition = MouseUtil.GetMousePositionInWorldSpace();
        
        // Direction calculated from fixed points to avoid circular dependency
        Vector3 direction = (endPosition - startPosition).normalized;
        
        // Arrowhead positioned at exact mouse location for precise targeting feedback
        if (arrowHead != null)
        {
            arrowHead.transform.position = endPosition;
            arrowHead.transform.right = direction;
        }
        
        // Line stops short of arrowhead to create visual separation and avoid overlap
        Vector3 lineEndPosition = endPosition - direction * 0.5f;
        
        // Both line points updated to ensure consistent coordinate system
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, lineEndPosition);
        }
    }
    #endregion
}
