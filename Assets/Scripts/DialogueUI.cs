using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueUI : MonoBehaviour
{
    [Header("Refs")]
    public Image dialogBackground;     // Asigna aquí tu sprite de la caja de diálogo (Image UI)
    public TMP_Text dialogText;        // Texto del diálogo
    public TMP_Text continueHint;      // “Click para continuar” (opcional)

    [Header("Ajustes")]
    public float charsPerSecond = 40f; // Velocidad de tipeo
    public bool useUnscaledTime = true;

    private CanvasGroup group;
    private bool running = false;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        HideImmediate();
    }

    public void ShowImmediate()
    {
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = true;
        if (continueHint) continueHint.gameObject.SetActive(false);
        if (dialogText) dialogText.text = "";
    }

    public void HideImmediate()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        if (dialogText) dialogText.text = "";
        if (continueHint) continueHint.gameObject.SetActive(false);
    }

    public IEnumerator ShowLines(string[] lines)
    {
        if (lines == null || lines.Length == 0) yield break;

        ShowImmediate();
        running = true;

        for (int i = 0; i < lines.Length; i++)
        {
            yield return TypeLine(lines[i]);

            // Mostrar hint “click para continuar”
            if (continueHint) continueHint.gameObject.SetActive(true);

            // Esperar click izquierdo o tecla (Space/Enter/E) para avanzar
            yield return WaitForAdvance();

            if (continueHint) continueHint.gameObject.SetActive(false);
        }

        running = false;
        HideImmediate();
    }

    private IEnumerator TypeLine(string line)
    {
        if (dialogText == null)
            yield break;

        dialogText.text = "";
        float delay = charsPerSecond > 0 ? 1f / charsPerSecond : 0f;

        foreach (char c in line)
        {
            dialogText.text += c;

            if (delay > 0)
            {
                if (useUnscaledTime) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }

            // Si el usuario hace click, escribir todo de golpe
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                dialogText.text = line;
                break;
            }
        }
    }

    private IEnumerator WaitForAdvance()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
                yield break;

            yield return null;
        }
    }
}