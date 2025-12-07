using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Escena")]
    public SceneManager scene;


    [Header("Fade (opcional)")]
    public CanvasGroup canvasGroup;     // Asigna si quieres fade. Si es null, se muestra inmediato.
    public float fadeDuration = 0.35f;

    [Header("Input rápido (opcional)")]
    public bool enableKeyboardShortcuts = true; // R: Restart, M: Main Menu, Q: Quit

    private bool isVisible = false;

    private void Awake()
    {
        // Asegurar estado inicial
        if (panel != null) panel.SetActive(false);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (!isVisible || !enableKeyboardShortcuts) return;

        if (Input.GetKeyDown(KeyCode.R)) OnRestart();
        if (Input.GetKeyDown(KeyCode.M)) OnMainMenu();
        if (Input.GetKeyDown(KeyCode.Q)) OnQuit();
    }

    public void Show()
    {
        isVisible = true;

        // Pausar juego
        Time.timeScale = 0f;

        if (panel != null) panel.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeDuration));
        }
    }

    public void Hide()
    {
        isVisible = false;

        // Reanudar juego
        Time.timeScale = 1f;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            StopAllCoroutines();
            StartCoroutine(FadeOutAndDisable());
        }
        else
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    // Botones UI
    public void OnRestart()
    {

            SafeUnpause();
            var scene = SceneManager.GetActiveScene();
            SceneManager.UnloadSceneAsync(scene.name);
            SceneManager.LoadScene(scene.name);
            Hide();

    }

    public void OnMainMenu()
    {
        if (GameManager.Instance != null)
        {
            SafeUnpause();
            GameManager.Instance.GoToMainMenu();
        }
        else
        {
            SafeUnpause();
            try
            {
                SceneManager.LoadScene("MainMenu");
            }
            catch
            {
                Debug.LogWarning("[GameOverUI] No existe escena 'MainMenu'. Implementa GameManager.GoToMainMenu o crea la escena.");
            }
        }
    }

    public void OnQuit()
    {
        if (GameManager.Instance != null)
        {
            SafeUnpause();
            GameManager.Instance.QuitGame();
        }
        else
        {
            SafeUnpause();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void SafeUnpause()
    {
        Time.timeScale = 1f;
    }

    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        cg.alpha = from;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // usar tiempo no escalado porque estamos en pausa
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private System.Collections.IEnumerator FadeOutAndDisable()
    {
        if (canvasGroup != null)
        {
            yield return FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, fadeDuration);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        if (panel != null) panel.SetActive(false);
    }
}