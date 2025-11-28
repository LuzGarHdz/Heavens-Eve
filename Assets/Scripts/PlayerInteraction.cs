using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private InteractableObject currentObject; // objeto con el que puede interactuar

    [Header("Lock")]
    public bool interactionsLocked = true; // bloqueado hasta hablar con NPC (escena Bosque). En Cuarto/Sótano puedes desmarcarlo en el Inspector.

    private static readonly string[] alwaysAllowed = { "NPC", "Closet", "Bicicleta" };
    public string interactionm;
    void Update()
    {
        if (currentObject != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[PlayerInteraction] E pressed on {currentObject.objectName} (disabled={currentObject.isDisabled}, locked={interactionsLocked})");

            if (currentObject.isDisabled)
            {
                Debug.Log("[PlayerInteraction] currentObject is disabled, ignoring interaction.");
                return; // no interactuar si está deshabilitado
            }

            if (IsAlwaysAllowed(currentObject.objectName))
            {
                Debug.Log("[PlayerInteraction] Always-allowed object. Calling Interact().");
                currentObject.Interact();
            }
            else
            {
                if (!interactionsLocked)
                {
                    Debug.Log("[PlayerInteraction] Interactions unlocked. Calling Interact().");
                    currentObject.Interact();
                }
                else
                {
                    Debug.Log("[PlayerInteraction] Interactions locked. Showing 'Habla con el NPC primero'.");
                    if (InteractionManager.Instance != null)
                        InteractionManager.Instance.ShowMessage("Habla con el NPC primero [E]");
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[PlayerInteraction] OnTriggerEnter2D name={collision.name} tag={collision.tag}");

        if (collision.CompareTag("Interactable"))
        {
            var io = collision.GetComponent<InteractableObject>();
            currentObject = io;

            if (io == null)
            {
                Debug.LogWarning("[PlayerInteraction] Collider with tag 'Interactable' has NO InteractableObject component.");
                return;
            }

            Debug.Log($"[PlayerInteraction] Entered interactable: {io.objectName}, disabled={io.isDisabled}");

            if (io.isDisabled)
            {
                Debug.Log("[PlayerInteraction] Interactable is disabled; not showing prompt.");
                return;
            }

            if (io.objectName == "Closet")
            {
                interactionm="[E para abrir]";
            }else if (io.objectName == "Bicicleta")
            {
                interactionm="[E para reparar]";
            }

            if (io.objectName == "Closet" || io.objectName == "Bicicleta")
            {
                Debug.Log($"[PlayerInteraction] Showing prompt for {io.objectName}");
                if (InteractionManager.Instance != null)
                    InteractionManager.Instance.ShowMessage(interactionm);
            }
            else
            {
                if (InteractionManager.Instance != null)
                {
                    if (!interactionsLocked)
                    {
                        Debug.Log("[PlayerInteraction] interactionsUnlocked: Showing [E para abrir]");
                        InteractionManager.Instance.ShowMessage("[E para abrir]");
                    }
                    else
                    {
                        Debug.Log("[PlayerInteraction] interactionsLocked: Showing 'Habla con el NPC primero [E]'");
                        InteractionManager.Instance.ShowMessage("Habla con el NPC primero [E]");
                    }
                }
                else
                {
                    Debug.LogWarning("[PlayerInteraction] InteractionManager.Instance is null; prompt not shown.");
                }
            }
        }
        else if (collision.CompareTag("NPC"))
        {
            currentObject = collision.GetComponent<InteractableObject>();
            if (currentObject == null)
            {
                Debug.LogWarning("[PlayerInteraction] NPC collider has no InteractableObject component.");
                return;
            }

            Debug.Log("[PlayerInteraction] Entered NPC area. Showing [E para hablar]");
            if (InteractionManager.Instance != null)
                InteractionManager.Instance.ShowMessage("[E para hablar]");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"[PlayerInteraction] OnTriggerExit2D name={collision.name} tag={collision.tag}");

        if (collision.CompareTag("Interactable") || collision.CompareTag("NPC"))
        {
            if (currentObject != null && currentObject.gameObject == collision.gameObject)
            {
                Debug.Log("[PlayerInteraction] Exiting current interactable; clearing and hiding prompt.");
                currentObject = null;
                if (InteractionManager.Instance != null)
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