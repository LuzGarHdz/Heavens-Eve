using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private InteractableObject currentObject; // objeto con el que puede interactuar

    [Header("Lock")]
    public bool interactionsLocked = true; // bloqueado hasta hablar con NPC (escena Bosque). En Cuarto puedes desmarcarlo en el Inspector.

    private static readonly string[] alwaysAllowed = { "NPC", "Closet" };

    void Update()
    {
        if (currentObject != null && Input.GetKeyDown(KeyCode.E))
        {
            if (currentObject.isDisabled) return; // NUEVO: no interactuar si está deshabilitado

            if (IsAlwaysAllowed(currentObject.objectName))
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
            var io = collision.GetComponent<InteractableObject>();
            currentObject = io;

            // NUEVO: si está deshabilitado, no mostrar nada
            if (io != null && io.isDisabled)
            {
                return;
            }

            if (io != null && io.objectName == "Closet")
            {
                InteractionManager.Instance.ShowMessage("[E para abrir]");
            }
            else
            {
                if (!interactionsLocked)
                    InteractionManager.Instance.ShowMessage("[E para abrir]");
                else
                    InteractionManager.Instance.ShowMessage("Habla con el NPC primero [E]");
            }
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

    private bool IsAlwaysAllowed(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        for (int i = 0; i < alwaysAllowed.Length; i++)
        {
            if (alwaysAllowed[i] == name) return true;
        }
        return false;
    }
}