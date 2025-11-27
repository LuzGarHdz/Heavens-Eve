using UnityEngine;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public string objectName;

    public void Interact()
    {
        if (objectName == "Regalo")
        {
            Debug.Log("Abriendo " + objectName);
            gameObject.SetActive(false); // simula que se “abre” o desaparece
        }
        else if (objectName == "NPC")
        {
            Debug.Log("Hablando con " + objectName);
            InteractionManager.Instance.ShowInteraction("- Encontrar los 3 regalos");

        }
    }
}