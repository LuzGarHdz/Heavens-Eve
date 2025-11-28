using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectName = "Regalo";
    public GiftData giftData; // (sigue para inventario, si lo usas en otras escenas)
    public ClosetUI closetUI; // Asigna si este objeto es el closet

    public void Interact()
    {
        // Si estás usando restricciones por misión, adáptalo según tu escena:
        if (objectName == "Closet")
        {
            if (closetUI != null)
            {
                closetUI.OpenCloset();
            }
            else
            {
                Debug.LogWarning("InteractableObject: closetUI no asignado en el closet.");
            }
            return;
        }

        // Resto de tus casos existentes...
        if (!GameManager.Instance.missionStarted && objectName != "NPC")
        {
            InteractionManager.Instance.ShowMessage("Habla con el NPC primero [E]");
            return;
        }

        if (objectName == "Regalo")
        {
            if (giftData != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddGift(giftData);
            }
            GameManager.Instance.OnRegaloRecolectado();
            gameObject.SetActive(false);
        }
        else if (objectName == "NPC")
        {
            InteractionManager.Instance.ShowInteraction("- Encontrar los 3 regalos");
            InteractionManager.Instance.NotifyTalkedToNPC();
        }
    }
}