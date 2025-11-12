using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class LevelSelectSystemEventHandler : DynamicEventSystemHandler
{
    private Image _selectableImage;
    private MapButton _mapButton;

    private MapSelectManager _mapSelectManager;

    private bool _initialMoveComplete;

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

        _selectableImage = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<Image>() : null;
        _mapButton = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<MapButton>() : null;

        if (_mapButton != null)
        {
            if (_mapSelectManager != null && _mapSelectManager.LevelHeaderText != null)
            {
                if (_mapButton.MapData != null)
                {
                    _mapSelectManager.LevelHeaderText.SetText(_mapButton.MapData.MapId);
                }
                else
                {
                    _mapSelectManager.LevelHeaderText.SetText("Unknown");
                }
                RectTransform rectTrans = eventData.selectedObject.GetComponent<RectTransform>();

                if (_initialMoveComplete)
                    _mapSelectManager.MovePlayerToButton(_mapSelectManager.PlayerObj, rectTrans, _mapSelectManager.WorldSpaceCanvasRect);

                _initialMoveComplete = true;
            }

            if (_selectableImage != null)
            {
                _selectableImage.color = Color.red;
            }
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        if (_mapButton != null)
        {
            if (_mapSelectManager != null && _mapSelectManager.LevelHeaderText != null)
            {
                _mapSelectManager.LevelHeaderText.SetText("");
            }
            if (_selectableImage != null)
            {
                _selectableImage.color = _mapButton.ReturnColor;
            }
        }
    }


}