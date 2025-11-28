using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class TransitionOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private bool blockOnlyWhenVisible = true;
    [SerializeField] private float visibleThreshold = 0.01f;

    private void Reset()
    {
        group = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        group.interactable = false;
        UpdateBlocking();
    }

    private void UpdateBlocking()
    {
        if (!group) return;
        group.blocksRaycasts = blockOnlyWhenVisible ? group.alpha > visibleThreshold : group.blocksRaycasts;
    }

    public void SetAlpha(float a)
    {
        if (!group) return;
        group.alpha = a;
        UpdateBlocking();
    }

    public IEnumerator FadeOut(float duration) => FadeTo(1f, duration);
    public IEnumerator FadeIn(float duration) => FadeTo(0f, duration);

    public IEnumerator FadeTo(float target, float duration)
    {
        if (!group) yield break;
        float start = group.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // fade independiente del Time.timeScale
            float a = Mathf.Lerp(start, target, t / duration);
            group.alpha = a;
            UpdateBlocking();
            yield return null;
        }
        group.alpha = target;
        UpdateBlocking();
    }

    // Métodos para Animation Events si usas Animator:
    public void BeginBlockRaycasts() { if (group) group.blocksRaycasts = true; }
    public void EndBlockRaycasts() { if (group) group.blocksRaycasts = false; }
}