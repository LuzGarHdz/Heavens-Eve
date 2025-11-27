using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // referencia al Canvas del menú

    private bool isPaused = false;

    private static PauseMenu instance;



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.GetComponent<Movement>().enabled = true;
    }

    void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // Desactivar control del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.GetComponent<Movement>().enabled = false;
    }


}
