using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BicycleRepairMission : MonoBehaviour
{
    [Header("Panel / HUD")]
    public GameObject panel;
    public DragAndDropMinigame minigame;
    public bool pauseOnOpen = true;

    [Header("Bicicleta Mundo")]
    public GameObject bici;
    public Sprite repairedBikeSprite;

    [Header("Diálogo")]
    public DialogueUI dialogueUI;
    [TextArea] public string[] successLines = { "La bicicleta ha quedado como nueva." };
    [TextArea] public string[] failedLines = { "No lograste repararla a tiempo. Inténtalo de nuevo." };

    [Header("Flags globales")]
    public MissionFlagsSO flags;

    [Header("Inventario")]
    public string cascoGiftName = "Casco";

    private bool opening = false;
    private bool completed = false;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.LogWarning("[BicycleRepairMission] EventSystem creado automáticamente.");
        }
    }

    public void Open()
    {
        if (completed) return;
        if (opening) return;
        opening = true;

        if (pauseOnOpen) Time.timeScale = 0f;

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

        if (panel != null) panel.SetActive(true);
        if (minigame == null) { opening = false; return; }

        minigame.OnCompleted = OnMinigameCompleted;
        minigame.OnFailed = OnMinigameFailed;
        minigame.StartMinigame();
    }

    public void Close()
    {
        if (!opening) return;
        opening = false;
        if (panel != null) panel.SetActive(false);
        if (pauseOnOpen) Time.timeScale = 1f;
        if (minigame != null)
        {
            minigame.OnCompleted = null;
            minigame.OnFailed = null;
        }
    }

    private void OnMinigameCompleted()
    {
        completed = true;
        if (flags != null)
        {
            flags.sotanoBikeCompleted = true;
            Debug.Log("[BicycleRepairMission] sotanoBikeCompleted = TRUE");
            var tocadiscos = FindObjectOfType<TocadiscosMission>();
            if (tocadiscos != null) tocadiscos.OnCoreMissionsStateChanged();

        }
        Close();
        UpdateBikeSprite();
        // remover casco del inventario
        if (InventoryManager.Instance != null)
        {
            bool removed = InventoryManager.Instance.RemoveGiftByName(cascoGiftName);
            Debug.Log($"[BicycleRepairMission] Remove '{cascoGiftName}' del inventario: {removed}");
        }
        StartCoroutine(ShowResultDialogue(successLines));
    }

    private void OnMinigameFailed()
    {
        Close();
        StartCoroutine(ShowResultDialogue(failedLines));
    }

    private IEnumerator ShowResultDialogue(string[] lines)
    {
        if (dialogueUI == null || lines == null || lines.Length == 0) yield break;
        var cg = dialogueUI.GetComponent<CanvasGroup>();
        if (cg != null) { cg.blocksRaycasts = true; cg.interactable = true; }
        yield return dialogueUI.ShowLines(lines);
        dialogueUI.HideImmediate();
        if (cg != null) { cg.blocksRaycasts = false; cg.interactable = false; }
    }

    private void UpdateBikeSprite()
    {
        if (bici == null || repairedBikeSprite == null) return;
        var sr = bici.GetComponent<SpriteRenderer>();
        if (sr == null) sr = bici.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) { sr.sprite = repairedBikeSprite; return; }
        var img = bici.GetComponent<Image>();
        if (img != null) { img.sprite = repairedBikeSprite; }
    }
}