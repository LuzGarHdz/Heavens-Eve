using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Panel principal")]
    public GameObject inventoryPanel; // El panel completo (preset). Desactivado al inicio.

    [Header("Slots fijos (4)")]
    public Image[] slotImages;        // Asignar EXACTAMENTE 4 Image (los cuadros de la derecha)
    private GiftData[] slotData;      // Internamente guardamos cuál GiftData está en cada slot

    [Header("Detalle")]
    public Image detailImage;         // Área grande a la izquierda
    public TMP_Text detailNameText;
    public TMP_Text detailDescriptionText;

    [Header("Control")]
    public KeyCode toggleKey = KeyCode.I;
    public bool pauseOnOpen = true;
    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
        slotData = new GiftData[slotImages.Length];

        // Inicialmente limpiar slots
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].sprite = null;
                slotImages[i].enabled = false;
            }
        }
        detailImage.enabled = false;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        ClearDetail();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (isOpen) CloseInventory();
        else OpenInventory();
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(true);
        isOpen = true;

        if (pauseOnOpen)
            Time.timeScale = 0f;
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;
        inventoryPanel.SetActive(false);
        isOpen = false;

        if (pauseOnOpen)
            Time.timeScale = 1f;
    }

    // Añadir un regalo al primer slot vacío
    public void AddGift(GiftData gift)
    {
        if (gift == null) return;

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotData[i] == null)
            {
                slotData[i] = gift;
                slotImages[i].sprite = gift.iconSprite;
                slotImages[i].enabled = true;
                return;
            }
        }

        // Inventario lleno: podrías mostrar un mensaje
        Debug.Log("Inventario lleno (4/4).");
    }

    // Llamado por los botones de cada slot
    public void OnSlotClicked(int index)
    {
        if (index < 0 || index >= slotData.Length) return;
        var gift = slotData[index];
        if (gift == null) return;

        ShowDetail(gift);
    }

    private void ShowDetail(GiftData gift)
    {
        detailImage.enabled = true;
        if (detailImage != null)
            detailImage.sprite = gift.detailSprite != null ? gift.detailSprite : gift.iconSprite;

        if (detailNameText != null)
            detailNameText.text = gift.giftName;

        if (detailDescriptionText != null)
            detailDescriptionText.text = gift.description;
    }

    public void ClearDetail()
    {
        detailImage.enabled = false;
        if (detailImage != null) detailImage.sprite = null;
        if (detailNameText != null) detailNameText.text = "";
        if (detailDescriptionText != null) detailDescriptionText.text = "";
    }

    public bool IsOpen() => isOpen;
}