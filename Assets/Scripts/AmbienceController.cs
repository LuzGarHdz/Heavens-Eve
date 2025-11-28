using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Fuentes")]
    public AudioSource ambientSource;     // Fuente looping ambiente
    public AudioSource turntableSource;   // Fuente para el disco (puede loop o one-shot)

    [Header("Clips")]
    public AudioClip ambientClip;
    public AudioClip turntableClip;

    [Header("Volúmenes")]
    [Range(0f, 1f)] public float ambientVolume = 0.6f;
    [Range(0f, 1f)] public float turntableVolume = 0.8f;

    [Header("Fades")]
    public float fadeDurationToTurntable = 2f;
    public float fadeDurationBackToAmbient = 2f;

    [Header("Reinicio / Estado")]
    public bool playAmbientOnStart = true;
    public bool turntableLoop = true;     // True si quieres que la canción gire indefinidamente
    private bool turntableActivated = false;

    private Coroutine ambientFadeCoroutine;
    private Coroutine turntableFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Validar fuentes
        if (ambientSource == null) Debug.LogError("[AudioManager] Falta ambientSource.");
        if (turntableSource == null) Debug.LogError("[AudioManager] Falta turntableSource.");

        // Configurar fuentes inicialmente
        if (ambientSource != null)
        {
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            ambientSource.volume = 0f; // Fade-in luego
        }
        if (turntableSource != null)
        {
            turntableSource.loop = turntableLoop;
            turntableSource.playOnAwake = false;
            turntableSource.volume = 0f;
        }
    }

    private void Start()
    {
        if (playAmbientOnStart) PlayAmbientInitial();
    }

    public void PlayAmbientInitial()
    {
        if (ambientSource == null || ambientClip == null) return;

        ambientSource.clip = ambientClip;
        ambientSource.volume = 0f;
        ambientSource.Play();
        // Fade-in rápido para no arrancar silencioso total
        StartCoroutine(FadeVolume(ambientSource, 0f, ambientVolume, 1f));
    }

    public void PlayTurntableTrack()
    {
        // Evitar segunda activación
        if (turntableActivated)
        {
            Debug.Log("[AudioManager] Turntable ya activado, ignorando.");
            return;
        }
        turntableActivated = true;

        // Iniciar pista del tocadiscos con fade cruzado
        if (turntableSource == null || turntableClip == null)
        {
            Debug.LogWarning("[AudioManager] No hay fuente o clip de tocadiscos asignado.");
            return;
        }

        turntableSource.clip = turntableClip;
        turntableSource.volume = 0f;
        turntableSource.Play();

        // Fade out ambiente + fade in tocadiscos
        if (ambientSource != null)
        {
            if (ambientFadeCoroutine != null) StopCoroutine(ambientFadeCoroutine);
            ambientFadeCoroutine = StartCoroutine(FadeVolume(ambientSource, ambientSource.volume, 0f, fadeDurationToTurntable));
        }
        if (turntableFadeCoroutine != null) StopCoroutine(turntableFadeCoroutine);
        turntableFadeCoroutine = StartCoroutine(FadeVolume(turntableSource, 0f, turntableVolume, fadeDurationToTurntable));
    }

    public void ReturnToAmbient()
    {
        // Usar esto si quieres volver al ambiente después que termine la pista (si no loop)
        if (turntableSource != null && turntableSource.isPlaying)
        {
            turntableSource.Stop();
        }
        turntableActivated = false;

        if (ambientSource != null)
        {
            ambientSource.volume = 0f;
            if (!ambientSource.isPlaying)
            {
                ambientSource.clip = ambientClip;
                ambientSource.Play();
            }
            StartCoroutine(FadeVolume(ambientSource, 0f, ambientVolume, fadeDurationBackToAmbient));
        }
    }

    // Puedes llamar esto desde GameOver, por ejemplo
    public void StopAllMusicImmediate()
    {
        if (ambientSource != null) ambientSource.Stop();
        if (turntableSource != null) turntableSource.Stop();
    }

    private IEnumerator FadeVolume(AudioSource src, float from, float to, float duration)
    {
        if (src == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // música no se detiene si pauso Time.timeScale
            float lerp = Mathf.Lerp(from, to, t / duration);
            src.volume = lerp;
            yield return null;
        }
        src.volume = to;
    }

    // Método auxiliar para saber si ya activamos el tocadiscos
    public bool IsTurntableActivated() => turntableActivated;
}