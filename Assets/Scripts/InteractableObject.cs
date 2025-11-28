using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectName = "Regalo";
    public GiftData giftData; // (sigue para inventario, si lo usas en otras escenas)
    public ClosetUI closetUI; // Asigna si este objeto es el closet

    [Header("Missions")]
    public BicycleRepairMission bicycleMission; // Asigna para el caso "Bicicleta"

    [Header("Estado")]
    public bool isDisabled = false;     // si está deshabilitado, no muestra prompt ni interactúa

    public void Interact()
    {
        Debug.Log($"[InteractableObject] Interact called on '{objectName}' (disabled={isDisabled})");

        if (isDisabled)
        {
            Debug.Log("[InteractableObject] Object is disabled. Ignoring.");
            return;
        }

        // Misión Gate (opcional en este objeto)
        var gate = GetComponent<MissionGate>();
        if (gate != null)
        {
            bool can = gate.CanInteract();
            Debug.Log($"[InteractableObject] MissionGate present. CanInteract={can}");
            if (!can)
            {
                gate.ShowLockedMessage();
                return;
            }
        }

        if (objectName == "Closet")
        {
            Debug.Log("[InteractableObject] Opening Closet UI");
            if (closetUI != null) closetUI.OpenCloset();
            else Debug.LogWarning("InteractableObject: closetUI no asignado en el closet.");
            return;
        }

        if (objectName == "Bicicleta")
        {
            Debug.Log("[InteractableObject] Opening Bicycle mission");
            if (bicycleMission != null) bicycleMission.Open();
            else Debug.LogWarning("InteractableObject: bicycleMission no asignado.");
            return;
        }

        // Resto de tus casos existentes...
        bool gmStarted = GameManager.Instance?.missionStarted ?? false;
        Debug.Log($"[InteractableObject] GameManager missionStarted={gmStarted}");

        if (!gmStarted)
        {
            if (objectName != "NPC")
            {
                if (InteractionManager.Instance != null)
                    InteractionManager.Instance.ShowMessage("Habla con el NPC primero [E]");
                else
                    Debug.LogWarning("[InteractableObject] InteractionManager.Instance is null; cannot show message.");
                return;
            }
        }

        if (objectName == "Regalo")
        {
            Debug.Log("[InteractableObject] Picking up Gift");
            if (giftData != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddGift(giftData);
            }
            GameManager.Instance?.OnRegaloRecolectado();
            gameObject.SetActive(false);
        }
        else if (objectName == "NPC")
        {
            Debug.Log("[InteractableObject] NPC interaction");
            InteractionManager.Instance?.ShowInteraction("- Encontrar los 3 regalos");
            InteractionManager.Instance?.NotifyTalkedToNPC();
        }
    }
}