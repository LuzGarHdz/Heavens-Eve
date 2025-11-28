using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    public GameObject textPanel; // referencia al panel del texto (UI)
    public TMP_Text textUI;      // referencia al componente TMP_Text
    public TMP_Text interaction_message;

    private void Awake()
    {
        Instance = this;
        if (textPanel != null) textPanel.SetActive(false);
    }

    public void ShowInteraction(string text)
    {
        if (interaction_message != null)
            interaction_message.text = text;
    }

    public void ShowMessage(string message)
    {
        if (textPanel != null) textPanel.SetActive(true);
        if (textUI != null) textUI.text = message;
    }

    public void HideMessage()
    {
        if (textPanel != null) textPanel.SetActive(false);
    }

    // Nuevo: avisar al GameManager cuando se hable con el NPC
    public void NotifyTalkedToNPC()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTalkedToNPC();
        }
    }
}