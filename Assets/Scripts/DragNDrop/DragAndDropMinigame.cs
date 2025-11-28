using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DragAndDropMinigame : MonoBehaviour
{
    [Header("Items requeridos (IDs)")]
    public List<string> requiredItemIds = new List<string>();

    [Header("UI")]
    public Canvas canvas;
    public RectTransform itemsContainer;   // contenedor donde están los DraggableItemUI
    public Transform placedContainer;      // donde reubicamos los ítems ya colocados
    public DropZoneUI dropZone;            // zona de “reparación”
    public Image stateImage;               // imagen que muestra la bicicleta en sus estados
    public Sprite[] stateSprites;          // 0=rota ... N=arreglada

    [Header("Timer")]
    public bool useTimer = true;
    public float timeLimitSeconds = 40f;
    public TMP_Text timerText;

    [Header("Mensajes")]
    public TMP_Text progressText;          // opcional: “X/Y piezas”
    public string progressFormat = "{0}/{1}";

    private HashSet<string> placed = new HashSet<string>();
    private List<DraggableItemUI> items = new List<DraggableItemUI>();
    private float timeLeft;
    private bool running;

    public System.Action OnCompleted;
    public System.Action OnFailed;

    private void Awake()
    {
        if (dropZone != null) dropZone.controller = this;

        items.Clear();
        if (itemsContainer != null)
        {
            foreach (Transform t in itemsContainer)
            {
                var d = t.GetComponent<DraggableItemUI>();
                if (d != null)
                {
                    d.canvas = canvas;
                    items.Add(d);
                }
            }
        }
        Debug.Log($"[DragAndDropMinigame] Awake. items={items.Count} required={requiredItemIds.Count} dropZone={(dropZone ? dropZone.name : "null")} canvas={(canvas ? canvas.name : "null")}");
    }

    public void StartMinigame()
    {
        // Validar requiredItemIds contra los DraggableItemUI presentes
        var availableIds = new HashSet<string>();
        foreach (var d in items)
        {
            if (d != null && !string.IsNullOrEmpty(d.itemId))
                availableIds.Add(d.itemId);
        }
        foreach (var req in requiredItemIds)
        {
            if (!availableIds.Contains(req))
                Debug.LogWarning($"[DragAndDropMinigame] Required itemId '{req}' no existe en itemsContainer.");
        }

        // Validar sprites de estado
        if (stateImage != null && (stateSprites == null || stateSprites.Length == 0))
            Debug.LogWarning("[DragAndDropMinigame] stateSprites vacío; no se actualizará imagen de estado.");

        placed.Clear();
        timeLeft = timeLimitSeconds;
        running = true;

        Debug.Log($"[DragAndDropMinigame] Start. timeLimit={timeLimitSeconds}s useTimer={useTimer}");

        // Reset de ítems a su contenedor original (si quieres anclar posiciones iniciales, ya se guardan en DraggableItemUI al comenzar drag)
        foreach (var d in items)
        {
            if (d == null) continue;
            var rt = d.GetComponent<RectTransform>();
            rt.SetParent(itemsContainer, worldPositionStays: false);
            // No cambiamos anchoredPosition para respetar el layout
        }

        UpdateStateSprite();
        UpdateProgressUI();
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!running || !useTimer) return;

        float dt = Time.unscaledDeltaTime; // independiente de pausa
        timeLeft -= dt;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;
            UpdateTimerUI();
            Debug.Log("[DragAndDropMinigame] Time expired. Failing.");
            OnFailed?.Invoke();
        }
        else
        {
            UpdateTimerUI();
        }
    }

    public void TryPlace(DraggableItemUI item, DropZoneUI zone)
    {
        if (item == null)
        {
            Debug.LogWarning("[DragAndDropMinigame] TryPlace: item is null");
            return;
        }

        Debug.Log($"[DragAndDropMinigame] TryPlace: itemId={item.itemId}");

        if (!requiredItemIds.Contains(item.itemId))
        {
            Debug.Log($"[DragAndDropMinigame] Item '{item.itemId}' no requerido. Volviendo al origen.");
            ReturnToOrigin(item);
            return;
        }

        if (placed.Contains(item.itemId))
        {
            Debug.Log($"[DragAndDropMinigame] Item '{item.itemId}' ya estaba colocado. Volviendo al origen.");
            ReturnToOrigin(item);
            return;
        }

        // Aceptado
        placed.Add(item.itemId);
        var rt = item.GetComponent<RectTransform>();
        rt.SetParent(placedContainer != null ? placedContainer : zone.transform, worldPositionStays: false);
        rt.anchoredPosition = Vector2.zero;

        Debug.Log($"[DragAndDropMinigame] Colocado '{item.itemId}'. Progreso: {placed.Count}/{requiredItemIds.Count}");

        UpdateStateSprite();
        UpdateProgressUI();

        // ¿Completado?
        if (placed.Count >= requiredItemIds.Count)
        {
            running = false;
            Debug.Log("[DragAndDropMinigame] Todos los items colocados. Completed.");
            OnCompleted?.Invoke();
        }
    }

    private void ReturnToOrigin(DraggableItemUI item)
    {
        var rt = item.GetComponent<RectTransform>();
        rt.SetParent(item.originalParent != null ? item.originalParent : itemsContainer, worldPositionStays: false);
        rt.anchoredPosition = item.originalAnchoredPos;
    }

    private void UpdateStateSprite()
    {
        if (stateImage == null || stateSprites == null || stateSprites.Length == 0) return;
        int idx = Mathf.Clamp(placed.Count, 0, stateSprites.Length - 1);
        stateImage.sprite = stateSprites[idx];
    }

    private void UpdateProgressUI()
    {
        if (progressText == null) return;
        progressText.text = string.Format(progressFormat, placed.Count, requiredItemIds.Count);
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int t = Mathf.CeilToInt(timeLeft);
        timerText.text = t.ToString("00");
    }
}