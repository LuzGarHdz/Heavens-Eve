using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BicycleRepairMission : MonoBehaviour
{
    [Header("Panel / HUD")]
    public GameObject panel;                 // HUD del minijuego (inactivo al inicio)
    public DragAndDropMinigame minigame;     // Controlador drag & drop
    public bool pauseOnOpen = true;

    [Header("Bicicleta Mundo")]
    [Tooltip("GameObject de la bicicleta rota en la escena (fuera del HUD).")]
    public GameObject bici;
    [Tooltip("Sprite final de bicicleta reparada.")]
    public Sprite repairedBikeSprite;

    [Header("Diálogo")]
    public DialogueUI dialogueUI;
    [TextArea] public string[] successLines = { "La bicicleta ha quedado como nueva." };
    [TextArea] public string[] failedLines = { "No lograste repararla a tiempo. Inténtalo de nuevo." };

    [Header("Flags globales")]
    public MissionFlagsSO flags; // Marca sotanoBikeCompleted al completar

    private bool opening = false;
    private bool completed = false;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        Debug.Log($"[BicycleRepairMission] Awake panel={(panel ? panel.name : "null")} minigame={(minigame ? minigame.name : "null")}");

        // Garantizar EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.LogWarning("[BicycleRepairMission] No había EventSystem. Se creó uno automáticamente.");
        }
    }

    public void Open()
    {
        if (completed)
        {
            Debug.Log("[BicycleRepairMission] Misión ya completada. Ignorando nueva apertura.");
            return;
        }

        if (opening)
        {
            Debug.LogWarning("[BicycleRepairMission] Open llamado mientras ya está abierto.");
            return;
        }
        opening = true;

        Debug.Log($"[BicycleRepairMission] Open (pauseOnOpen={pauseOnOpen})");

        if (pauseOnOpen) Time.timeScale = 0f;

        // Asegurar que el diálogo esté oculto y no bloquee (NO deshabilitar script)
        if (dialogueUI != null)
        {
            dialogueUI.HideImmediate();
            var cg = dialogueUI.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
        }

        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("[BicycleRepairMission] Panel activado.");
        }
        else
        {
            Debug.LogError("[BicycleRepairMission] Panel es null; no se puede abrir.");
            opening = false;
            return;
        }

        if (minigame == null)
        {
            Debug.LogError("[BicycleRepairMission] minigame es null; no se puede iniciar.");
            opening = false;
            return;
        }

        // Enlazar callbacks (limpiando previos)
        minigame.OnCompleted = OnMinigameCompleted;
        minigame.OnFailed = OnMinigameFailed;

        Debug.Log("[BicycleRepairMission] Iniciando minijuego...");
        minigame.StartMinigame();
    }

    public void Close()
    {
        if (!opening) return;

        opening = false;
        Debug.Log("[BicycleRepairMission] Close()");

        if (panel != null) panel.SetActive(false);
        if (pauseOnOpen) Time.timeScale = 1f;

        // Limpiar callbacks para evitar llamadas después
        if (minigame != null)
        {
            minigame.OnCompleted = null;
            minigame.OnFailed = null;
        }
    }

    private void OnMinigameCompleted()
    {
        Debug.Log("[BicycleRepairMission] Minigame COMPLETED callback");
        completed = true;

        if (flags != null)
        {
            flags.sotanoBikeCompleted = true;
            Debug.Log("[BicycleRepairMission] Flag sotanoBikeCompleted = true");
        }
        else
        {
            Debug.LogWarning("[BicycleRepairMission] flags es null; no puedo marcar global.");
        }

        Close();
        UpdateBikeSprite();
        StartCoroutine(ShowResultDialogue(successLines));
    }

    private void OnMinigameFailed()
    {
        Debug.Log("[BicycleRepairMission] Minigame FAILED callback");
        Close();
        StartCoroutine(ShowResultDialogue(failedLines));
    }

    private IEnumerator ShowResultDialogue(string[] lines)
    {
        if (dialogueUI == null || lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[BicycleRepairMission] No hay diálogo o líneas para mostrar.");
            yield break;
        }

        // Preparar diálogo para mostrarse
        var cg = dialogueUI.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }

        Debug.Log("[BicycleRepairMission] Mostrando diálogo de resultado...");
        yield return dialogueUI.ShowLines(lines);

        Debug.Log("[BicycleRepairMission] Diálogo finalizado. Ocultando.");
        dialogueUI.HideImmediate();
        if (cg != null)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    private void UpdateBikeSprite()
    {
        if (bici == null)
        {
            Debug.LogWarning("[BicycleRepairMission] 'bici' es null; no puedo cambiar sprite.");
            return;
        }

        if (repairedBikeSprite == null)
        {
            Debug.LogWarning("[BicycleRepairMission] repairedBikeSprite es null; asigna el sprite reparado.");
            return;
        }

        // Intentar SpriteRenderer directo
        var sr = bici.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            // Intentar en hijos
            sr = bici.GetComponentInChildren<SpriteRenderer>();
        }

        if (sr != null)
        {
            var prev = sr.sprite ? sr.sprite.name : "null";
            sr.sprite = repairedBikeSprite;
            Debug.Log($"[BicycleRepairMission] SpriteRenderer actualizado. Antes='{prev}' Ahora='{repairedBikeSprite.name}'");
            return;
        }

        // Intentar que sea UI Image (Canvas)
        var img = bici.GetComponent<Image>();
        if (img != null)
        {
            var prev = img.sprite ? img.sprite.name : "null";
            img.sprite = repairedBikeSprite;
            Debug.Log($"[BicycleRepairMission] Image UI actualizado. Antes='{prev}' Ahora='{repairedBikeSprite.name}'");
            return;
        }

        Debug.LogWarning("[BicycleRepairMission] 'bici' no tiene SpriteRenderer ni Image. No se pudo actualizar el sprite.");
    }
}