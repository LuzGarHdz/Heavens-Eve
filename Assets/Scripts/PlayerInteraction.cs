using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private InteractableObject currentObject; // objeto con el que puede interactuar

    [Header("Lock")]
    public bool interactionsLocked = true; // Bosque: true; En Cuarto puedes desmarcarlo en el Inspector.

    // Objetos que SIEMPRE pueden interactuar aunque haya lock
    private static readonly string[] alwaysAllowed = { "NPC", "Closet" };

    void Update()
    {
        if (currentObject != null && Input.GetKeyDown(KeyCode.E))
        {
            if (IsAlwaysAllowed(currentObject.objectName))
            {
                Debug.Log($"[PlayerInteraction] Interact con {currentObject.objectName}");
                currentObject.Interact();
            }
            else
            {
                if (!interactionsLocked)
                {
                    Debug.Log($"[PlayerInteraction] Interact (sin lock) con {currentObject.objectName}");
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

            if (currentObject != null)
            {
                if (IsAlwaysAllowed(currentObject.objectName))
                    InteractionManager.Instance.ShowMessage("[E para abrir]");
                else
                    InteractionManager.Instance.ShowMessage(!interactionsLocked ? "[E para abrir]" : "Habla con el NPC primero [E]");
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