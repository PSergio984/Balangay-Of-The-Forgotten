using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class LevelSelectSystemEventHandler : DynamicEventSystemHandler
{
    private MapSelectManager _mapSelectManager;

    protected void Awake()
    {
        base.Awake();
        _mapSelectManager = GetComponentInParent<MapSelectManager>();
        if (_mapSelectManager == null)
        {
            Debug.LogError("[LevelSelectSystemEventHandler] Could not find MapSelectManager in parent hierarchy. Disabling component.", this);
            enabled = false;
        }
    }

    public override void OnPointerEnter(BaseEventData eventData)
    {
    }

    public override void OnPointerExit(BaseEventData eventData)
    {
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        var image = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<Image>() : null;
        var mapButton = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<MapButton>() : null;
        if (mapButton != null)
        {
            if (_mapSelectManager != null && _mapSelectManager.LevelHeaderText != null)
            {
                _mapSelectManager.LevelHeaderText.SetText(mapButton.MapData.MapId);
            }

            if (image != null)
            {
                image.color = Color.red;
            }
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        var image = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<Image>() : null;
        var mapButton = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<MapButton>() : null;
        if (mapButton != null)
        {
            if (_mapSelectManager != null && _mapSelectManager.LevelHeaderText != null)
            {
                _mapSelectManager.LevelHeaderText.SetText("");
            }
            if (image != null)
            {
                image.color = mapButton.ReturnColor;
            }
        }
    }


}