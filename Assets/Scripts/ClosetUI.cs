using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClosetUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject closetPanel;                   // Panel del closet (desactivado al inicio)

    [Header("Plush Setup")]
    public List<PlushData> plushes;                 // Lista total de peluches a mostrar
    public List<Button> plushButtons;               // Botones en tu layout (mismo orden que plushes)
    public Image previewImage;                      // Imagen grande (opcional)
    public TMP_Text previewName;                    // Nombre (opcional)

    [Header("Diálogo")]
    public DialogueUI dialogueUI;                   // Referencia al DialogueUI (en el mismo Canvas)
    public TMP_Text confirmText;                    // Texto tipo “¿Elegir este peluche? [Q] Sí [E] Regresar”

    [Header("Reglas")]
    public List<PlushData> correctPlushes;          // Peluches correctos (define en el inspector)
    public int negativesDamage = 1;                 // Daño por peluche negativo (si no usas damageOnPick de cada Plush)

    [Header("Referencias")]
    public Health playerHealth;                     // Asigna el Health del jugador
    public MisionCuartoManager missionManager;     // Orquestador de la misión (puede ser null si no lo usas)
    public bool pauseOnOpen = true;

    private HashSet<PlushData> selectedCorrect = new HashSet<PlushData>();
    private bool isOpen = false;
    private bool waitingConfirm = false;
    private PlushData currentPlush = null;

    void Start()
    {
        if (closetPanel) closetPanel.SetActive(false);

        // Mapear clicks de botones a peluches
        for (int i = 0; i < plushButtons.Count; i++)
        {
            int idx = i;
            if (plushButtons[idx] != null)
            {
                // Icono del botón
                if (idx < plushes.Count && plushes[idx] != null && plushes[idx].iconSprite != null)
                {
                    var img = plushButtons[idx].GetComponent<Image>();
                    if (img) img.sprite = plushes[idx].iconSprite;
                }

                plushButtons[idx].onClick.AddListener(() => OnPlushClicked(idx));
            }
        }
        if (confirmText) confirmText.gameObject.SetActive(false);
    }

    public void OpenCloset()
    {
        if (isOpen) return;
        isOpen = true;

        if (pauseOnOpen) Time.timeScale = 0f;
        if (closetPanel) closetPanel.SetActive(true);

        if (missionManager != null)
            missionManager.OnClosetOpened();
    }

    public void CloseCloset()
    {
        if (!isOpen) return;
        isOpen = false;

        if (closetPanel) closetPanel.SetActive(false);
        if (pauseOnOpen) Time.timeScale = 1f;

        if (missionManager != null)
            missionManager.OnClosetClosed();
    }

    public void OnPlushClicked(int index)
    {
        if (index < 0 || index >= plushes.Count) return;
        var data = plushes[index];
        currentPlush = data;

        // Preview opcional
        if (previewImage) previewImage.sprite = data.detailSprite ? data.detailSprite : data.iconSprite;
        if (previewName) previewName.text = data.plushName;

        // Lanzar diálogos del peluche
        StartCoroutine(HandlePlushDialogueAndConfirm(data));
    }

    public IEnumerator HandlePlushDialogueAndConfirm(PlushData data)
    {
        // Diálogos
        if (dialogueUI != null && data.dialogLines != null && data.dialogLines.Length > 0)
        {
            yield return dialogueUI.ShowLines(data.dialogLines);
        }

        // Confirmación
        waitingConfirm = true;
        if (confirmText)
        {
            confirmText.text = $"¿Elegir este peluche?\n[Q] Sí   [E] Regresar";
            confirmText.gameObject.SetActive(true);
        }

        // Esperar elección
        while (waitingConfirm)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnConfirmPlush(data);
                waitingConfirm = false;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                // Regresar sin elegir
                waitingConfirm = false;
            }
            yield return null;
        }

        if (confirmText) confirmText.gameObject.SetActive(false);
        currentPlush = null;
    }

    public void OnConfirmPlush(PlushData data)
    {
        if (data.isNegative)
        {
            int dmg = data.damageOnPick > 0 ? data.damageOnPick : negativesDamage;
            if (playerHealth != null)
                playerHealth.TakeHit(dmg);
        }
        else
        {
            selectedCorrect.Add(data);
            missionManager?.UpdateMissionProgress(selectedCorrect.Count, correctPlushes.Count);

            // ¿Completó todos los correctos?
            if (IsAllCorrectSelected())
            {
                missionManager?.OnMissionCompleted();
                CloseCloset();
            }
        }
    }

    private bool IsAllCorrectSelected()
    {
        if (correctPlushes == null || correctPlushes.Count == 0) return false;
        foreach (var p in correctPlushes)
        {
            if (!selectedCorrect.Contains(p))
                return false;
        }
        return true;
    }
}