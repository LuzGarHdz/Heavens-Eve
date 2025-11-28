using UnityEngine;

public class AmbienceController : MonoBehaviour
{
    public AudioSource source;
    public AudioClip ambienceClip;
    public float volume = 0.6f;

    private void Awake()
    {
        if (source != null && ambienceClip != null)
        {
            source.clip = ambienceClip;
            source.loop = true;
            source.volume = volume;
            source.Play();
        }
    }
}