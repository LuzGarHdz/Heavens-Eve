using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ClosetUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject closetPanel;

    [Header("Plush Setup")]
    public List<PlushData> plushes;
    public List<Button> plushButtons;
    public Image previewImage;
    public TMP_Text previewName;

    [Header("Diálogo")]
    public DialogueUI dialogueUI;

    [Header("Reglas")]
    public List<PlushData> correctPlushes;   // Si lo dejas vacío, usará PlushData.isCorrect
    public int negativesDamage = 1;

    [Header("Referencias")]
    public Health playerHealth;              // Asigna el Health del jugador
    public MisionCuartoManager missionManager;
    public bool pauseOnOpen = true;

    private HashSet<PlushData> selectedCorrect = new HashSet<PlushData>();
    private bool isOpen = false;
    private bool initialized = false;
    private bool isProcessing = false;
    private Coroutine activeFlow;

    private void Awake()
    {
        // closetPanel debe iniciar desactivado
    }

    private void OnEnable()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (initialized) return;

        for (int i = 0; i < plushButtons.Count; i++)
        {
            int idx = i;
            var btn = plushButtons[idx];
            if (btn == null) continue;

            // Icono
            if (idx < plushes.Count && plushes[idx] != null && plushes[idx].iconSprite != null)
            {
                var img = btn.GetComponent<Image>();
                if (img) img.sprite = plushes[idx].iconSprite;
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnPlushClicked(idx));
        }

        initialized = true;
        Debug.Log("[ClosetUI] Inicializado. Buttons=" + plushButtons.Count + " Plushes=" + plushes.Count);
    }

    public void OpenCloset()
    {
        Debug.Log("[ClosetUI] OpenCloset()");
        EnsureInitialized();
        if (isOpen) return;
        isOpen = true;

        if (pauseOnOpen) Time.timeScale = 0f;
        if (closetPanel) closetPanel.SetActive(true);

        missionManager?.OnClosetOpened();
        previewImage.enabled = false;
    }

    public void CloseCloset()
    {
        if (!isOpen) return;
        isOpen = false;

        if (dialogueUI && dialogueUI.IsRunning)
            dialogueUI.CancelDialogue();

        if (activeFlow != null)
        {
            StopCoroutine(activeFlow);
            activeFlow = null;
        }

        isProcessing = false;

        if (closetPanel) closetPanel.SetActive(false);
        if (pauseOnOpen) Time.timeScale = 1f;

        missionManager?.OnClosetClosed();
    }

    private void SetButtonsInteractable(bool value)
    {
        foreach (var b in plushButtons)
            if (b != null) b.interactable = value;
    }

    public void OnPlushClicked(int index)
    {
        if (!isOpen) return;
        if (isProcessing) return;
        previewImage.enabled = true;
        if (index < 0 || index >= plushes.Count)
        {
            Debug.LogWarning("[ClosetUI] Click index fuera de rango: " + index);
            return;
        }

        var data = plushes[index];
        Debug.Log($"[ClosetUI] Click idx={index}, plush={(data != null ? data.plushName : "NULL")}");

        if (data == null) return;

        isProcessing = true;

        // Preview
        if (previewImage) previewImage.sprite = data.detailSprite ? data.detailSprite : data.iconSprite;
        if (previewName) previewName.text = data.plushName;

        SetButtonsInteractable(false);
        activeFlow = StartCoroutine(FlowPlush(data, index));
    }

    private IEnumerator FlowPlush(PlushData data, int buttonIndex)
    {
        string confirmPrompt = "¿Elegir este peluche?\n[Q] Sí   [E] Regresar";

        // Diálogo + confirmación en el mismo cuadro
        if (dialogueUI != null)
        {
            var lines = (data.dialogLines != null) ? data.dialogLines : System.Array.Empty<string>();
            yield return dialogueUI.ShowLinesThenConfirm(lines, confirmPrompt, KeyCode.Q, KeyCode.E);

            bool yes = dialogueUI.ConfirmResult;
            if (yes)
            {
                OnConfirmPlush(data, buttonIndex);
            }
        }

        SetButtonsInteractable(true);
        isProcessing = false;
        activeFlow = null;
    }

    private void OnConfirmPlush(PlushData data, int buttonIndex)
    {
        if (data == null) return;

        if (data.isNegative)
        {
            int dmg = data.damageOnPick > 0 ? data.damageOnPick : negativesDamage;
            if (playerHealth != null)
                playerHealth.TakeHit(dmg);
            else
                Debug.LogWarning("[ClosetUI] playerHealth no asignado; no se aplicará daño.");
        }
        else
        {
            // Marcar selección correcta
            bool added = selectedCorrect.Add(data);
            if (added)
            {
                // Deshabilitar el botón del peluche correcto para no volver a contarlo
                if (buttonIndex >= 0 && buttonIndex < plushButtons.Count && plushButtons[buttonIndex] != null)
                {
                    plushButtons[buttonIndex].interactable = false;
                }

                int totalCorrect = GetTotalCorrect();
                int current = selectedCorrect.Count(p => IsCorrectPlush(p));

                missionManager?.UpdateMissionProgress(current, totalCorrect);

                if (current >= totalCorrect && totalCorrect > 0)
                {
                    Debug.Log("[ClosetUI] ¡Todos los peluches correctos seleccionados!");
                    missionManager?.OnMissionCompleted();
                    CloseCloset();
                }
            }
        }
    }

    private int GetTotalCorrect()
    {
        if (correctPlushes != null && correctPlushes.Count > 0)
            return correctPlushes.Count;

        // Fallback: contar por flag en los PlushData
        return plushes != null ? plushes.Count(p => p != null && p.isCorrect) : 0;
    }

    private bool IsCorrectPlush(PlushData data)
    {
        if (data == null) return false;

        if (correctPlushes != null && correctPlushes.Count > 0)
            return correctPlushes.Contains(data);

        return data.isCorrect;
    }
}