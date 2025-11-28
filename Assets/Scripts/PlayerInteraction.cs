using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private InteractableObject currentObject; // objeto con el que puede interactuar
    [Header("Lock")]
    public bool interactionsLocked = true; // bloqueado hasta hablar con NPC

    void Update()
    {
        if (currentObject != null && Input.GetKeyDown(KeyCode.E))
        {
            // Permitir siempre hablar con NPC; bloquear otros si aún no inicia misión
            if (currentObject.objectName == "NPC")
            {
                currentObject.Interact();
            }
            else
            {
                if (!interactionsLocked)
                {
                    currentObject.Interact();
                }
                else
                {
                    InteractionManager.Instance.ShowMessage("Habla con el NPC primero [E]");
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable"))
        {
            currentObject = collision.GetComponent<InteractableObject>();
            if (!interactionsLocked)
                InteractionManager.Instance.ShowMessage("[E para abrir]");
            else
                InteractionManager.Instance.ShowMessage("Habla con el NPC primero [E]");
        }
        else if (collision.CompareTag("NPC"))
        {
            currentObject = collision.GetComponent<InteractableObject>();
            InteractionManager.Instance.ShowMessage("[E para hablar]");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable") || collision.CompareTag("NPC"))
        {
            if (currentObject != null && currentObject.gameObject == collision.gameObject)
            {
                currentObject = null;
                InteractionManager.Instance.HideMessage();
            }
        }
    }
}