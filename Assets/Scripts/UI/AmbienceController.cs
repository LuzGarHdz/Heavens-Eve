using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip mainLoopClip;
    public AudioClip finalTrackClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PlayMainLoop()
    {
        if (musicSource == null || mainLoopClip == null) return;
        if (musicSource.clip == mainLoopClip && musicSource.isPlaying) return;

        musicSource.loop = true;
        musicSource.clip = mainLoopClip;
        musicSource.Play();
    }

    public void PlayFinalTrack()
    {
        if (musicSource == null || finalTrackClip == null) return;

        musicSource.loop = false;
        musicSource.Stop();
        musicSource.clip = finalTrackClip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }
}