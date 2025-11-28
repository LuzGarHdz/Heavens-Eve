using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item")]
    public string itemId = "pieza";

    [Header("UI")]
    public Canvas canvas;                // Canvas raíz del HUD
    public RectTransform dragLayer;      // Capa superior (hijo del Canvas) para arrastrar por encima de todo

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector2 originalAnchoredPos;

    private RectTransform rect;
    private CanvasGroup group;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null)
        {
            Debug.LogWarning($"[DraggableItemUI] Canvas no asignado para '{itemId}'.");
            return;
        }

        originalParent = rect.parent;
        originalAnchoredPos = rect.anchoredPosition;

        // Elevar al dragLayer si está asignado, si no, al Canvas root
        if (dragLayer != null)
            rect.SetParent(dragLayer, worldPositionStays: false);
        else
            rect.SetParent(canvas.transform, worldPositionStays: false);

        group.blocksRaycasts = false; // para que DropZone reciba el drop
        group.alpha = 0.95f;

        Debug.Log($"[DraggableItemUI] BeginDrag '{itemId}' pos={originalAnchoredPos}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.blocksRaycasts = true;
        group.alpha = 1f;

        // Si no lo recogió un DropZone (reparent), vuelve al origen
        if (rect.parent == originalParent || rect.parent == canvas.transform || (dragLayer != null && rect.parent == dragLayer))
        {
            rect.SetParent(originalParent, worldPositionStays: false);
            rect.anchoredPosition = originalAnchoredPos;
        }

        Debug.Log($"[DraggableItemUI] EndDrag '{itemId}' parent={(rect.parent ? rect.parent.name : "null")}");
    }
}