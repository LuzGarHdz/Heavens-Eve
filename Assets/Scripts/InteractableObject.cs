using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectName = "Regalo";
    public GiftData giftData; // Asigna un GiftData si es un regalo

    public void Interact()
    {
        // Respetar tu lógica de misión previa
        if (!GameManager.Instance.missionStarted && objectName != "NPC")
        {
            InteractionManager.Instance.ShowMessage("");
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