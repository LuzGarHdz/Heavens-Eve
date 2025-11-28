using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
    }

    // Botones UI
    public void OnRestart()
    {
        GameManager.Instance.RestartLevel();
    }

    public void OnMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }

    public void OnQuit()
    {
        GameManager.Instance.QuitGame();
    }
}