using System.Collections;
using UnityEngine;

public class AlmaBosqueNPC : MonoBehaviour
{
    [Header("Diálogo")]
    public DialogueUI dialogueUI;

    [Header("Flags / Misión Bosque")]
    public MissionFlagsSO flags;              // Asset de banderas globales
    public MisionBosqueManager bosqueManager; // Manager que lleva el conteo de regalos

    [Header("Opciones")]
    public bool autoFocusOnFirstInteract = true; // Si quieres que el jugador siempre vea este diálogo al interactuar la primera vez
    public float delayAfterMissionComplete = 0.5f;

    private bool introShown = false;
    private bool completionShown = false;
    private bool playingDialogue = false;

    // Líneas del diálogo inicial (separadas para el typewriter)
    private readonly string[] introLines = new string[]
    {
        "Pareces nuevo por aquí, así que voy a guiarte.",
        "Una vez mueres, y todavía vagas por la tierra, es porque tienes asuntos sin resolver que te encadenan a este lugar.",
        "Tienes hasta antes del amanecer para poder desprenderte de esas cargas, o si no, te volverás un alma errante que es incapaz de ser libre.",
        "Si no recuerdas qué es, busca en el bosque; tal vez ahí encuentres alguna pista de qué es lo que te está atando.",
        "Y cuidado, que no estarás solo; quienes envidian tu lugar intentarán cazarte."
    };

    private readonly string[] completionLines = new string[]
    {
        "Algo tienes que hacer con esos regalos, ¿no crees?"
    };

    // Llamado por InteractableObject cuando el jugador presiona E
    public void OnPlayerInteract()
    {
        if (playingDialogue) return;

        // Si la misión ya terminó y no se mostró el diálogo final
        if (flags != null && flags.bosqueCompleted && !completionShown)
        {
            StartCoroutine(PlayCompletionDialogue());
            return;
        }

        // Mostrar intro si no se ha mostrado
        if (!introShown)
        {
            StartCoroutine(PlayIntroDialogue());
            return;
        }

        // Si ya se mostró todo, puedes poner un diálogo neutro opcional
        // Ej: StartCoroutine(dialogueUI.ShowLines(new[]{"Debes darte prisa..."}));
    }

    private IEnumerator PlayIntroDialogue()
    {
        introShown = true;
        playingDialogue = true;

        if (dialogueUI != null)
            yield return dialogueUI.ShowLines(introLines);

        playingDialogue = false;
    }

    private IEnumerator PlayCompletionDialogue()
    {
        completionShown = true;
        playingDialogue = true;

        if (delayAfterMissionComplete > 0f)
            yield return new WaitForSecondsRealtime(delayAfterMissionComplete);

        if (dialogueUI != null)
            yield return dialogueUI.ShowLines(completionLines);

        playingDialogue = false;
    }

    // Método que puede invocar el manager cuando detecta que se completaron los regalos
    public void NotifyMissionCompleted()
    {
        if (!completionShown)
        {
            // Si quieres que el diálogo final salga automáticamente sin interacción:
            StartCoroutine(PlayCompletionDialogue());
        }
    }
}