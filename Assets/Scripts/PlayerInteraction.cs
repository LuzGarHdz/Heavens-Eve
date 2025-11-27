using UnityEngine;


public class PlayerInteraction : MonoBehaviour
{

    private InteractableObject currentObject; // objeto con el que puede interactuar

    void Update()
    {
        // Si el jugador está cerca de un objeto y presiona E
        if (currentObject != null && Input.GetKeyDown(KeyCode.E))
        {
            currentObject.Interact();
        }
    }

    // Cuando entra en el área del objeto
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable"))
        {
            currentObject = collision.GetComponent<InteractableObject>();
            InteractionManager.Instance.ShowMessage("[E para abrir]");
        }
        else if (collision.CompareTag("NPC"))
        {
            currentObject=collision.GetComponent<InteractableObject>();
            InteractionManager.Instance.ShowMessage("[E para hablar]");

        }
    }

    // Cuando sale del área del objeto
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable"))
        {
            currentObject = null;
            InteractionManager.Instance.HideMessage();
        }
        else if (collision.CompareTag("NPC"))
        {
            currentObject=null;
            InteractionManager.Instance.HideMessage();
        }
    }
}
