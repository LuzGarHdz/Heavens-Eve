using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CutscenePlayer : MonoBehaviour
{
    public string nextSceneName = "Bosque";
    public float autoDuration = 54f;
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    [Header("Limpieza")]
    public bool cleanupOnStart = false;   // true en CutsceneFinal para borrar UI persistente al entrar
    public bool cleanupOnFinish = true;   // true para que al salir a MainMenu no quede nada persistente

    [Header("Refs")]
    public PlayableDirector timeline;
    public CanvasGroup fade;
    public Button skipButton;

    [Header("Fade")]
    public float fadeInTime = 0.35f;
    public float fadeOutTime = 0.35f;

    private bool finishing = false;

    void Start()
    {
        if (cleanupOnStart) CleanupPersistents();
        if (skipButton) skipButton.onClick.AddListener(Skip);
        StartCoroutine(PlayFlow());
    }

    IEnumerator PlayFlow()
    {
        yield return Fade(1f, 0f, fadeInTime); // de negro a visible
        if (timeline) timeline.Play();

        float t = 0f;
        while (!finishing)
        {
            if (allowSkip && Input.GetKeyDown(skipKey)) Skip();

            if (timeline == null)
            {
                t += Time.deltaTime;
                if (t >= autoDuration) Finish();
            }
            else if (timeline.state != PlayState.Playing)
            {
                Finish();
            }
            yield return null;
        }
    }

    public void Skip()
    {
        if (!allowSkip || finishing) return;
        if (timeline) timeline.Stop();
        Finish();
    }

    void Finish()
    {
        if (finishing) return;
        finishing = true;
        StartCoroutine(FinishRoutine());
    }

    IEnumerator FinishRoutine()
    {
        yield return Fade(0f, 1f, fadeOutTime); // a negro
        Time.timeScale = 1f;
        if (cleanupOnFinish) CleanupPersistents();
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(float from, float to, float time)
    {
        if (fade == null) yield break;
        float e = 0f;
        fade.alpha = from;
        while (e < time)
        {
            e += Time.deltaTime;
            fade.alpha = Mathf.Lerp(from, to, e / time);
            yield return null;
        }
        fade.alpha = to;
    }

    private void CleanupPersistents()
    {
        DestroyIfExists(MissionManager.Instance);
        DestroyIfExists(InventoryManager.Instance);
        DestroyIfExists(FindObjectOfType<UIManager>());
        DestroyIfExists(FindObjectOfType<GameManager>());
        // NO destruir AudioManager para mantener la música final.
    }

    private void DestroyIfExists(Component comp)
    {
        if (comp != null) Destroy(comp.gameObject);
    }
}