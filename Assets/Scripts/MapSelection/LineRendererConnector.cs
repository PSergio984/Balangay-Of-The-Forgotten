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
        _camera = Camera.main;
    }

    public void UpdateLinePosition()
    {
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