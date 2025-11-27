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
        textPanel.SetActive(false);
    }

    public void ShowInteraction(string text)
    {
        interaction_message.text = text;

    }

    public void ShowMessage(string message)
    {
        textPanel.SetActive(true);
        textUI.text = message;
    }

    public void HideMessage()
    {
        textPanel.SetActive(false);
    }
}