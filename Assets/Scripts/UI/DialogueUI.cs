using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueUI : MonoBehaviour
{
    [Header("Refs")]
    public Image dialogBackground;
    public TMP_Text dialogText;
    public TMP_Text continueHint;

    [Header("Typewriter")]
    public float charsPerSecond = 40f;
    public bool useUnscaledTime = true;
    public float inputBlockSecondsOnStart = 0.12f;
    public KeyCode[] advanceKeys = { KeyCode.Space, KeyCode.Return, KeyCode.E };

    private CanvasGroup group;
    private Coroutine activeCoroutine;
    private bool skippingType;
    private float unblockTime;

    public bool IsRunning { get; private set; }
    public bool ConfirmResult { get; private set; }

    void Awake()
    {
        EnsureGroup();
        HideImmediate();
    }

    // Inicialización perezosa
    private void EnsureGroup()
    {
        if (group == null)
        {
            group = GetComponent<CanvasGroup>();
            if (group == null)
            {
                Debug.LogError("[DialogueUI] Falta CanvasGroup en el mismo GameObject.");
            }
        }
    }

    public void ShowImmediate()
    {
        EnsureGroup();
        if (group == null) return;

        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = true;
        if (continueHint) continueHint.gameObject.SetActive(false);
        if (dialogText) dialogText.text = "";
    }

    public void HideImmediate()
    {
        EnsureGroup();
        if (group == null) return;

        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        if (dialogText) dialogText.text = "";
        if (continueHint) continueHint.gameObject.SetActive(false);
    }

    public void CancelDialogue()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        IsRunning = false;
        skippingType = false;
        HideImmediate();
    }

    public IEnumerator ShowLines(string[] lines)
    {
        yield return ShowLinesInternal(lines);
    }

    public IEnumerator ShowLinesThenConfirm(string[] lines, string confirmPrompt, KeyCode yesKey, KeyCode noKey)
    {
        yield return ShowLinesInternal(lines);

        if (useUnscaledTime) yield return new WaitForSecondsRealtime(0.08f);
        else yield return new WaitForSeconds(0.08f);
        yield return null;

        if (dialogText) dialogText.text = confirmPrompt;
        if (continueHint) continueHint.gameObject.SetActive(false);

        yield return WaitForYesNo(yesKey, noKey);

        HideImmediate();
        IsRunning = false;
    }

    private IEnumerator ShowLinesInternal(string[] lines)
    {
        if (lines == null) lines = System.Array.Empty<string>();

        CancelDialogue();
        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        unblockTime = now + Mathf.Max(0f, inputBlockSecondsOnStart);

        ShowImmediate();
        IsRunning = true;

        for (int i = 0; i < lines.Length; i++)
        {
            yield return TypeLine(lines[i]);
            if (continueHint) continueHint.gameObject.SetActive(true);
            yield return WaitForAdvance();
            if (continueHint) continueHint.gameObject.SetActive(false);
        }
    }

    private IEnumerator TypeLine(string line)
    {
        if (dialogText == null) yield break;

        dialogText.text = "";
        skippingType = false;
        float delay = charsPerSecond > 0 ? 1f / charsPerSecond : 0f;

        for (int i = 0; i < line.Length; i++)
        {
            if (skippingType)
            {
                dialogText.text = line;
                break;
            }

            dialogText.text += line[i];

            if (delay > 0)
            {
                if (useUnscaledTime) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }

            if (IsAdvanceInputPressed())
                skippingType = true;
        }
    }

    private IEnumerator WaitForAdvance()
    {
        yield return null;
        while (true)
        {
            if (IsAdvanceInputPressed())
            {
                yield return null;
                yield break;
            }
            yield return null;
        }
    }

    private bool IsAdvanceInputPressed()
    {
        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        if (now < unblockTime) return false;

        foreach (var k in advanceKeys)
        {
            if (Input.GetKeyDown(k)) return true;
        }
        return false;
    }

    private IEnumerator WaitForYesNo(KeyCode yesKey, KeyCode noKey)
    {
        ConfirmResult = false;
        yield return null;
        while (true)
        {
            if (Input.GetKeyDown(yesKey))
            {
                ConfirmResult = true;
                yield return null;
                yield break;
            }
            if (Input.GetKeyDown(noKey))
            {
                ConfirmResult = false;
                yield return null;
                yield break;
            }
            yield return null;
        }
    }
}