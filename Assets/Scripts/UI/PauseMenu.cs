using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // referencia al Canvas del menú

    private bool isPaused = false;

    private static PauseMenu instance;

    public string mainMenuSceneName = "MainMenu";
    public bool cleanupPersistents = true;



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
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            SceneManager.LoadScene("Bosque");
        }
        else { 
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            player.GetComponent<Movement>().enabled = true;
        }
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

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (cleanupPersistents)
            CleanupPersistents();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void CleanupPersistents()
    {
        DestroyIfExists(MissionManager.Instance);
        DestroyIfExists(InventoryManager.Instance);
        DestroyIfExists(AudioManager.Instance);
        DestroyIfExists(FindObjectOfType<UIManager>()); // si no expone Instance
        DestroyIfExists(FindObjectOfType<GameManager>());
    }

    private void DestroyIfExists(Component comp)
    {
        if (comp != null) Destroy(comp.gameObject);
    }
}
