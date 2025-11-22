using UnityEngine;


/* LINE RENDERER CONNECTOR
 * 
 * Purpose: Draws a line between two UI buttons (like connecting dots on a map)
 * 
 * How it works:
 * - Takes two UI buttons (start and end points)
 * - Draws a visible line connecting them using LineRenderer
 * - Converts UI positions to world space so the line shows up correctly
 * 
 * Integration: Used in map selection to show connections between levels
 */


/// <summary>
/// Draws a connecting line between two UI elements on the screen
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Shows visual connections between map locations (like roads on a level select map)</para>
/// <para><strong>How:</strong> Takes two UI button positions and draws a line between them using Unity's LineRenderer</para>
/// </remarks>
public class LineRendererConnector : MonoBehaviour
{
    /// <summary>
    /// The UI button where the line starts from
    /// </summary>
    public RectTransform StartRectTrans { get; set; }
    
    /// <summary>
    /// The UI button where the line ends at
    /// </summary>
    public RectTransform EndRectTrans { get; set; }
    
    /// <summary>
    /// The Unity component that actually draws the line
    /// </summary>
    private LineRenderer _lineRenderer;
    
    /// <summary>
    /// Reference to the main camera (needed to convert UI positions to world positions)
    /// </summary>
    private Camera _camera;


    /// <summary>
    /// Validates camera reference and re-finds if destroyed during scene transitions
    /// </summary>
    /// <returns>True if camera is valid, false otherwise</returns>
    private bool ValidateCamera()
    {
        // Check if camera reference is null or destroyed
        if (_camera == null || !_camera)
        {
            Debug.LogWarning($"[LineRendererConnector] Camera reference lost on {gameObject.name}, searching for camera...", this);
            
            // Try to find main camera first
            _camera = Camera.main;
            if (_camera == null)
            {
                // Find any camera in the active scene
                Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                foreach (Camera cam in allCameras)
                {
                    // Prefer cameras in this scene
                    if (cam.gameObject.scene == gameObject.scene)
                    {
                        _camera = cam;
                        Debug.LogWarning($"[LineRendererConnector] Found camera '{cam.name}' in scene.", this);
                        break;
                    }
                }
                
                // Use any camera as fallback
                if (_camera == null && allCameras.Length > 0)
                {
                    _camera = allCameras[0];
                    Debug.LogWarning($"[LineRendererConnector] Using camera '{_camera.name}' from another scene as fallback.", this);
                }
                
                if (_camera == null)
                {
                    Debug.LogError($"[LineRendererConnector] No camera found in any scene! Line rendering will fail on {gameObject.name}.", this);
                    return false;
                }
            }
        }
        return true;
    }


    /// <summary>
    /// Runs when the script first loads - finds the LineRenderer and camera
    /// </summary>
    private void Awake()
    {
        // Find the LineRenderer component on this same GameObject
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            Debug.LogError("LineRenderer component not found on " + gameObject.name);
        }

        // Initial camera setup
        ValidateCamera();
    }
    
    /// <summary>
    /// Updates the line to connect the start and end buttons
    /// </summary>
    /// <remarks>
    /// <para><strong>When:</strong> Call this after changing StartRectTrans or EndRectTrans to redraw the line</para>
    /// </remarks>
    public void UpdateLinePosition()
    {
        // Safety check - make sure we have both buttons assigned
        if (StartRectTrans == null || EndRectTrans == null)
        {
            Debug.LogWarning($"[LineRendererConnector] StartRectTrans or EndRectTrans is null on {gameObject.name}. Line will not be updated.");
            return;
        }

        // Validate camera before attempting to use it
        if (!ValidateCamera())
        {
            Debug.LogError($"[LineRendererConnector] Cannot update line position - no valid camera available on {gameObject.name}.", this);
            return;
        }

        // Convert the UI button positions to world positions (where the line actually draws)
        Vector3 startWorldPos = GetWorldPosition(StartRectTrans);
        Vector3 endWorldPos = GetWorldPosition(EndRectTrans);

        // Tell the LineRenderer to draw 2 points (start and end)
        _lineRenderer.positionCount = 2;
        
        // Set where those 2 points are located
        _lineRenderer.SetPosition(0, startWorldPos); // Point 0 = start
        _lineRenderer.SetPosition(1, endWorldPos);   // Point 1 = end
    }


    /// <summary>
    /// Converts a UI element's position into a world position for the LineRenderer
    /// </summary>
    /// <param name="rectTransform">The UI element to get the position from</param>
    /// <returns>World space position where the line should be drawn</returns>
    /// <remarks>
    /// <para><strong>Why:</strong> UI uses different coordinates than 3D world - this converts between them</para>
    /// </remarks>
    private Vector3 GetWorldPosition(RectTransform rectTransform)
    {
        // Step 1: Convert UI position to screen pixel position
        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(_camera, rectTransform.position);
        
        // Step 2: Convert screen pixel position to 3D world position
        return _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, _camera.nearClipPlane));
    }
}