using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Nombre de la primera escena jugable.")]
    public string firstSceneName = "Bosque";

    [Header("Opcional: limpiar singletons persistentes antes de iniciar")]
    public bool cleanupPersistents = true;

    private void Start()
    {
        // Arranca la música de menú/juego
        AudioManager.Instance?.PlayMainLoop();
    }
    public void PlayGame()
    {
        Time.timeScale = 1f;

        if (cleanupPersistents)
            CleanupPersistents();

        SceneManager.LoadScene("CutsceneIntro");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CleanupPersistents()
    {
        DestroyIfExists(InventoryManager.Instance);
        DestroyIfExists(MissionManager.Instance);
        DestroyIfExists(FindObjectOfType<UIManager>()); // no expone Instance pública
        DestroyIfExists(FindObjectOfType<GameManager>()); // por si quedara vivo en DDOL
    }

    private void DestroyIfExists(Component comp)
    {
        if (comp != null)
            Destroy(comp.gameObject);
    }
}