using UnityEngine;

public class LineRendererConnector : MonoBehaviour
{
    public RectTransform StartRectTrans { get; set; }
    public RectTransform EndRectTrans { get; set; }
    private LineRenderer _lineRenderer;
    private Camera _camera;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            Debug.LogError("LineRenderer component not found on " + gameObject.name);
        }

        _camera = Camera.main;
        if (_camera == null)
        {
            Debug.LogError("Main camera not found. Ensure a camera is tagged as MainCamera.");
        }
    }
    public void UpdateLinePosition()
    {
        if (StartRectTrans == null || EndRectTrans == null)
        {
            Debug.LogWarning($"[LineRendererConnector] StartRectTrans or EndRectTrans is null on {gameObject.name}. Line will not be updated.");
            return;
        }

        Vector3 startWorldPos = GetWorldPosition(StartRectTrans);
        Vector3 endWorldPos = GetWorldPosition(EndRectTrans);

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, startWorldPos);
        _lineRenderer.SetPosition(1, endWorldPos);
    }

    private Vector3 GetWorldPosition(RectTransform rectTransform)
    {
        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(_camera, rectTransform.position);
        return _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, _camera.nearClipPlane));
    }
}