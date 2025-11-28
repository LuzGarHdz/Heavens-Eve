using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string objectName = "Regalo";

    public void Interact()
    {
        // Si la misión no ha empezado, sólo permitir hablar con NPC
        if (!GameManager.Instance.missionStarted)
        {
            if (objectName == "NPC")
            {
                Debug.Log("Hablando con " + objectName);
                InteractionManager.Instance.ShowInteraction("- Encontrar los 3 regalos");
                InteractionManager.Instance.NotifyTalkedToNPC();
            }
            else
            {
                // Bloquear interacción con regalos antes de iniciar la misión
                InteractionManager.Instance.ShowMessage("Habla con el NPC primero [E]");
            }
            return;
        }

        // Misión en curso: permitir recoger regalos
        if (objectName == "Regalo")
        {
            Debug.Log("Recogiendo " + objectName);
            gameObject.SetActive(false);
            GameManager.Instance.OnRegaloRecolectado();
        }
        else if (objectName == "NPC")
        {
            InteractionManager.Instance.ShowInteraction("- Sigue buscando los regalos");
        }
    }
}