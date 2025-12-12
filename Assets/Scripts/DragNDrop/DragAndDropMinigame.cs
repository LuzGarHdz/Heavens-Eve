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
    public RectTransform itemsContainer;
    public Transform placedContainer;
    public DropZoneUI dropZone;
    public Image stateImage;
    public Sprite[] stateSprites;
    public Image newImage;


    [Header("Timer")]
    public bool useTimer = true;
    public float timeLimitSeconds = 40f;
    public TMP_Text timerText;

    public bool useSharedTimer = false;
    public float sharedTimerSeconds = 10f;
    public string sharedTimerTag = "GameTimer";
    private Timer sharedTimer;

    [Header("Mensajes")]
    public TMP_Text progressText;
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
                    // Crear capa superior para arrastre
                    var dl = canvas.transform.Find("DragLayer") as RectTransform;
                    if (dl == null)
                    {
                        var go = new GameObject("DragLayer", typeof(RectTransform));
                        dl = go.GetComponent<RectTransform>();
                        dl.SetParent(canvas.transform, false);
                        dl.anchorMin = Vector2.zero;
                        dl.anchorMax = Vector2.one;
                        dl.offsetMin = Vector2.zero;
                        dl.offsetMax = Vector2.zero;
                    }
                    d.dragLayer = dl;
                    items.Add(d);
                }
            }
        }
        Debug.Log($"[DragAndDropMinigame] Awake. items={items.Count} required={requiredItemIds.Count} dropZone={(dropZone ? dropZone.name : "null")} canvas={(canvas ? canvas.name : "null")}");
    }

    public void StartMiniggameValidation()
    {
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

        if (stateImage != null && (stateSprites == null || stateSprites.Length == 0))
            Debug.LogWarning("[DragAndDropMinigame] stateSprites vacío; no se actualizará imagen de estado.");
    }

    private string FormatTime(float t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        return $"{m:00}:{s:00}";
    }

    /* public void StartMinigame()
     {
         StartMiniggameValidation();

         placed.Clear();
         timeLeft = timeLimitSeconds;
         running = true;

         Debug.Log($"[DragAndDropMinigame] Start. timeLimit={timeLimitSeconds}s useTimer={useTimer}");

         foreach (var d in items)
         {
             if (d == null) continue;
             var rt = d.GetComponent<RectTransform>();
             rt.SetParent(itemsContainer, worldPositionStays: false);
             // respetar layout
             d.gameObject.SetActive(true); // asegurar visibles al iniciar
         }

         UpdateStateSprite();
         UpdateProgressUI();
         UpdateTimerUI();
     }

     private void Update()
     {
         if (!running || !useTimer) return;

         float dt = Time.unscaledDeltaTime;
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
    */

    public void StartMinigame()
    {
        StartMiniggameValidation();

        placed.Clear();
        running = true;

        if (useSharedTimer)
        {
            if (sharedTimer == null)
                sharedTimer = FindSharedTimer();

            if (sharedTimer != null)
            {
                useTimer = false;                 // desactiva timer local
                sharedTimer.useUnscaledTime = true; // el minijuego pausa timeScale
                sharedTimer.onExpired = OnSharedTimerExpired;
                sharedTimer.SetTime(sharedTimerSeconds);
                sharedTimer.StartCountdown();
            }
            else
            {
                Debug.LogWarning("[DragAndDropMinigame] useSharedTimer activo pero no se encontró Timer con tag " + sharedTimerTag);
            }
        }
        else
        {
            timeLeft = timeLimitSeconds;
            if (timerText != null && useTimer)
                timerText.text = FormatTime(timeLeft);
        }

        Debug.Log($"[DragAndDropMinigame] Start. useSharedTimer={useSharedTimer} seconds={(useSharedTimer ? sharedTimerSeconds : timeLimitSeconds)}");
        UpdateProgressUI();
    }

    private Timer FindSharedTimer()
    {
        var go = GameObject.FindGameObjectWithTag(sharedTimerTag);
        if (go == null) return null;
        return go.GetComponent<Timer>();
    }

    private void Update()
    {
        if (!running) return;

        if (useSharedTimer && sharedTimer != null)
        {
            // el timer compartido controla el tiempo; no hacemos nada aquí
            return;
        }

        if (useTimer)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                if (timeLeft < 0) timeLeft = 0;
                if (timerText != null) timerText.text = FormatTime(timeLeft);
            }
            else
            {
                running = false;
                OnFailed?.Invoke();
            }
        }
    }

    private void OnSharedTimerExpired()
    {
        if (!running) return;
        running = false;
        OnFailed?.Invoke();
    }

    private void StopSharedTimer()
    {
        if (sharedTimer != null)
        {
            sharedTimer.StopCountdown();
            sharedTimer.onExpired = null;
        }
    }

    public void StopMinigame()
    {
        running = false;
        StopSharedTimer();
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

        // Aceptado: marcar colocado y ocultar el objeto
        placed.Add(item.itemId);

        // Si quieres mostrar un placeholder en placedContainer, crea un Empty con imagen/sprite por ítem aquí.
        // Por ahora, simplemente desactivamos el item para que desaparezca:
        item.gameObject.SetActive(false);

        Debug.Log($"[DragAndDropMinigame] Colocado '{item.itemId}'. Progreso: {placed.Count}/{requiredItemIds.Count}");

        UpdateStateSprite();
        UpdateProgressUI();

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