using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DropZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public DragAndDropMinigame controller;
    public Image highlight; // opcional

    public void OnDrop(PointerEventData eventData)
    {
        var go = eventData.pointerDrag;
        var drag = go ? go.GetComponent<DraggableItemUI>() : null;
        Debug.Log($"[DropZoneUI] OnDrop. dragGO={(go ? go.name : "null")} hasDraggable={(drag != null)} zone={name}");

        if (controller == null)
        {
            Debug.LogError("[DropZoneUI] controller is null; cannot place.");
            return;
        }

        if (drag != null)
        {
            controller.TryPlace(drag, this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = false;
    }
}