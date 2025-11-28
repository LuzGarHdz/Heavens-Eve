using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BicycleRepairMission : MonoBehaviour
{
    [Header("Panel/UI")]
    public GameObject panel;                 // HUD del minijuego (inactivo al inicio)
    public DragAndDropMinigame minigame;     // Controlador
    public bool pauseOnOpen = true;
    public GameObject bici;
    public Sprite repairedBikeSprite;

    [Header("Diálogo")]
    public DialogueUI dialogueUI;
    [TextArea] public string[] successLines = { "La bicicleta ha quedado como nueva." };
    [TextArea] public string[] failedLines = { "No lograste repararla a tiempo. Inténtalo de nuevo." };

    [Header("Flags globales")]
    public MissionFlagsSO flags;             // Marca sotanoBikeCompleted al completar

    private bool opening = false;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        Debug.Log($"[BicycleRepairMission] Awake. panel={(panel ? panel.name : "null")} minigame={(minigame ? minigame.name : "null")}");

        // Garantizar EventSystem en la escena
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.LogWarning("[BicycleRepairMission] No había EventSystem. Se creó uno automáticamente.");
        }
    }

    public void Open()
    {
        if (opening)
        {
            Debug.LogWarning("[BicycleRepairMission] Open called but already opening.");
            return;

        }
        opening = true;
        dialogueUI.enabled = false;

        Debug.Log($"[BicycleRepairMission] Open. pauseOnOpen={pauseOnOpen}");
        if (pauseOnOpen) Time.timeScale = 0f;

        // Ocultar y deshabilitar raycasts del diálogo para que no bloquee el HUD
        if (dialogueUI != null)
        {
            dialogueUI.HideImmediate(); // oculta
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
            Debug.Log("[BicycleRepairMission] Panel activated");
        }
        else
        {
            Debug.LogError("[BicycleRepairMission] Panel is null; cannot open.");
        }

        if (minigame == null)
        {
            Debug.LogError("[BicycleRepairMission] minigame is null; cannot start.");
            return;
        }

        // Enlazar callbacks
        minigame.OnCompleted = OnMinigameCompleted;
        minigame.OnFailed = OnMinigameFailed;

        Debug.Log("[BicycleRepairMission] Starting minigame...");
        minigame.StartMinigame();
    }

    public void Close()
    {
        opening = false;

        Debug.Log("[BicycleRepairMission] Close");
        if (panel != null) panel.SetActive(false);
        if (pauseOnOpen) Time.timeScale = 1f;
    }

    private void OnMinigameCompleted()
    {
        
        Debug.Log("[BicycleRepairMission] Minigame COMPLETED");
        if (flags != null)
        {
            flags.sotanoBikeCompleted = true;
            Debug.Log("[BicycleRepairMission] Flags updated: sotanoBikeCompleted=true");
        }
        else
        {
            Debug.LogWarning("[BicycleRepairMission] flags is null; cannot mark completion globally.");
        }

        Close();
        dialogueUI.enabled = true;
        // Cambiar sprite de la bici a reparada
        if (bici != null && repairedBikeSprite != null)
        {
            var sr = bici.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = repairedBikeSprite;
                Debug.Log("[BicycleRepairMission] Bicycle sprite updated to repaired version.");
            }
            else
            {
                Debug.LogWarning("[BicycleRepairMission] Bicycle GameObject has no SpriteRenderer; cannot update sprite.");
            }
        }
        else
        {
            Debug.LogWarning("[BicycleRepairMission] Bicycle GameObject or repairedBikeSprite is null; cannot update sprite.");
        }

        if (dialogueUI != null && successLines != null && successLines.Length > 0)
        {
            Debug.Log("[BicycleRepairMission] Showing success dialogue");
            StartCoroutine(dialogueUI.ShowLines(successLines));
            

        }
        dialogueUI.enabled = false;
    }

    private void OnMinigameFailed()
    {
        Debug.Log("[BicycleRepairMission] Minigame FAILED");
        Close();

        dialogueUI.enabled = true;
        if (dialogueUI != null && failedLines != null && failedLines.Length > 0)
        {
            Debug.Log("[BicycleRepairMission] Showing failed dialogue");
            StartCoroutine(dialogueUI.ShowLines(failedLines));
        }
        dialogueUI.enabled = false;
    }
}