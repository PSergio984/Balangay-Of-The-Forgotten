using UnityEngine;

/* MOUSE UTILITY DOCUMENTATION
 * 
 * Purpose: Converts mouse screen position to world coordinates for game objects
 * 
 * How it works:
 * - Takes mouse position from screen coordinates (pixels)
 * - Converts it to world space coordinates that game objects can use
 * - Uses camera projection and ray casting for accurate positioning
 * 
 * Integration: Used by card dragging, object placement, and mouse interactions
 */

/// <summary>
/// Utility class for converting mouse positions from screen space to world space
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Converts mouse screen coordinates to world coordinates</para>
/// 
/// <para><strong>What it does:</strong> This utility helps convert the mouse position 
/// from screen pixels to world coordinates that game objects can understand. 
/// It's essential for dragging cards, placing objects, and any interaction 
/// where you need to know where the mouse is pointing in the game world.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Gets current mouse position in screen pixels</item>
/// <item>Creates a ray from camera through mouse position</item>
/// <item>Projects ray onto a plane at specified Z depth</item>
/// <item>Returns world coordinates where ray hits the plane</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> Main camera must exist in the scene for coordinate conversion</para>
/// 
/// <para><strong>Works with:</strong> Card dragging system, object placement, mouse interactions</para>
/// 
/// <para><strong>How to use:</strong> Call GetMousePositionInWorldSpace() to get current mouse world position</para>
/// </remarks>
public static class MouseUtil
{
    /// <summary>
    /// Reference to the main camera used for coordinate conversion
    /// </summary>
    /// <remarks>
    /// This field caches the main camera to avoid repeated lookups.
    /// Used for converting screen coordinates to world coordinates through ray casting.
    /// </remarks>
    private static Camera _camera = Camera.main;

    /// <summary>
    /// Converts current mouse position from screen coordinates to world coordinates
    /// </summary>
    /// <param name="zValue">The Z depth in world space where the mouse position should be calculated</param>
    /// <returns>World space coordinates of the mouse position at the specified Z depth</returns>
    /// <remarks>
    /// This method takes the current mouse position and converts it to world coordinates
    /// by casting a ray from the camera through the mouse position and finding where
    /// it intersects with a plane at the specified Z depth.
    /// </remarks>
    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0f)
    {
        // Create a plane facing the camera at the specified Z depth
        Plane dragPlane = new(_camera.transform.forward, new Vector3(0, 0, zValue));
        // Create a ray from camera through the mouse position
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        // Check if the ray hits the plane and get the distance
        if (dragPlane.Raycast(ray, out float distance))
        {
            // Return the world position where the ray hits the plane
            return ray.GetPoint(distance);
        }
        // Return zero if ray doesn't hit the plane (shouldn't happen normally)
        return Vector3.zero;
    }
}
