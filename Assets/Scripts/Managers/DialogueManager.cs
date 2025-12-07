using System.Collections;
using UnityEngine;

public class SimpleDialogue : MonoBehaviour
{
    public enum TriggerMode
    {
        Manual,         // Llamar Play() desde otro script
        OnStart,        // Al iniciar la escena (Start)
        OnTriggerEnter, // Al entrar el player en un trigger (requiere collider IsTrigger)
        OnInteract,     // Llamado por InteractableObject.Interact()
        OnFlagTrue      // Cuando cierta bandera de MissionFlagsSO se vuelve true
    }

    [Header("UI")]
    public DialogueUI dialogueUI;            // Asigna el DialogueUI (mismo Canvas)

    [Header("Contenido")]
    [TextArea] public string[] lines;        // Tus líneas de diálogo
    public bool playOnce = true;             // Reproducir sólo una vez
    public bool autoHideAfter = true;        // Ocultar el DialogueUI al terminar (usa HideImmediate)

    [Header("Disparo")]
    public TriggerMode triggerMode = TriggerMode.OnInteract;
    public float startDelay = 0f;            // Delay antes de reproducir (OnStart / OnFlagTrue)
    public KeyCode interactKey = KeyCode.E;  // Si quieres interceptar tecla (OnTriggerEnter con interacción)
    public bool requireInteractInput = false;// Si está en OnTriggerEnter y quieres que el jugador presione tecla

    [Header("Trigger Area (para OnTriggerEnter)")]
    public Collider2D triggerArea;           // Opcional (si null y el GO tiene collider IsTrigger, usa ese)
    public string playerTag = "Player";      // Tag del jugador para el trigger

    [Header("Flags (para OnFlagTrue)")]
    public MissionFlagsSO flags;             // Asset con banderas
    public FlagToWatch flagToWatch = FlagToWatch.None;

    public enum FlagToWatch
    {
        None,
        BosqueCompleted,
        CuartoCompleted,
        SotanoBikeCompleted,
        TocadiscosCompleted
    }

    [Header("Estado")]
    public bool disableMovementDuringDialogue = false; // Si quieres bloquear movimiento mientras corre
    public MonoBehaviour movementComponent;             // Referencia al script de movimiento (opcional)

    private bool hasPlayed = false;
    private bool awaitingInputInsideTrigger = false;
    private bool running = false;

    private void Reset()
    {
        // Intentar auto-asignar triggerArea
        if (triggerArea == null)
            triggerArea = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (triggerMode == TriggerMode.OnStart)
            StartCoroutine(PlayRoutineWithDelay());

        if (triggerMode == TriggerMode.OnFlagTrue)
            StartCoroutine(WatchFlagRoutine());
    }

    private void Update()
    {
        if (awaitingInputInsideTrigger && requireInteractInput && Input.GetKeyDown(interactKey))
        {
            Play();
        }
    }

    private IEnumerator PlayRoutineWithDelay()
    {
        if (playOnce && hasPlayed) yield break;
        if (startDelay > 0f)
            yield return new WaitForSecondsRealtime(startDelay);
        Play();
    }

    private IEnumerator WatchFlagRoutine()
    {
        if (playOnce && hasPlayed) yield break;
        // Espera hasta que la bandera seleccionada sea true
        while (!IsFlagTrue())
            yield return new WaitForSecondsRealtime(0.25f);

        yield return PlayRoutineWithDelay();
    }

    private bool IsFlagTrue()
    {
        if (flags == null) return false;
        switch (flagToWatch)
        {
            case FlagToWatch.BosqueCompleted: return flags.bosqueCompleted;
            case FlagToWatch.CuartoCompleted: return flags.cuartoCompleted;
            case FlagToWatch.SotanoBikeCompleted: return flags.sotanoBikeCompleted;
            case FlagToWatch.TocadiscosCompleted: return flags.tocadiscosCompleted;
            default: return false;
        }
    }

    // Llamar externamente para reproducir (Manual / OnInteract / otros)
    public void Play()
    {
        if (dialogueUI == null)
        {
            Debug.LogWarning("[SimpleDialogue] dialogueUI no asignado.");
            return;
        }
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[SimpleDialogue] No hay líneas de diálogo.");
            return;
        }
        if (playOnce && hasPlayed)
        {
            Debug.Log("[SimpleDialogue] playOnce=true y ya se reprodujo. Ignorando.");
            return;
        }
        if (running)
        {
            Debug.Log("[SimpleDialogue] Ya hay un diálogo corriendo.");
            return;
        }

        StartCoroutine(RunDialogue());
    }

    private IEnumerator RunDialogue()
    {
        running = true;
        hasPlayed = true;

        // Bloquear movimiento
        if (disableMovementDuringDialogue && movementComponent != null)
            movementComponent.enabled = false;

        Debug.Log("[SimpleDialogue] Iniciando diálogo...");
        yield return dialogueUI.ShowLines(lines);

        Debug.Log("[SimpleDialogue] Diálogo terminado.");

        if (autoHideAfter)
            dialogueUI.HideImmediate();

        // Rehabilitar movimiento
        if (disableMovementDuringDialogue && movementComponent != null)
            movementComponent.enabled = true;

        running = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerMode != TriggerMode.OnTriggerEnter) return;
        if (other.CompareTag(playerTag))
        {
            if (playOnce && hasPlayed) return;

            if (requireInteractInput)
            {
                awaitingInputInsideTrigger = true;
                Debug.Log("[SimpleDialogue] Player entró. Presiona " + interactKey + " para iniciar diálogo.");
            }
            else
            {
                Play();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (triggerMode != TriggerMode.OnTriggerEnter) return;
        if (other.CompareTag(playerTag))
        {
            awaitingInputInsideTrigger = false;
        }
    }
}