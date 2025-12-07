using System.Collections;
using UnityEngine;

public class TocadiscosMission : MonoBehaviour
{
    [Header("Flags (Asset compartido)")]
    public MissionFlagsSO flags;

    [Header("Gate visual")]
    public GameObject mantaA; // tapa: se oculta cuando AllCoreCompleted
    public GameObject mantaB; // destapado: se muestra cuando AllCoreCompleted
    public bool autoApplyGateOnStart = true;
    public bool autoPollFlags = true;  // opcional: refresca mantas si el estado cambia en runtime
    public float pollInterval = 0.5f;

    [Header("Inventario")]
    public string discoGiftName = "Disco";

    [Header("Disco visual")]
    public Transform diskSlot;               // punto donde se coloca
    public SpriteRenderer diskRenderer;      // renderer del disco (inicialmente oculto)
    public Sprite diskSprite;                // sprite del disco

    [Header("Animación y audio")]
    public Animator tocadiscosAnimator;      // parámetro bool "isSpinning"
    public float spinDuration = 6f;
    public bool stopAfterDuration = false;   // true si quieres detener después de spinDuration
    public AudioSource audioSource;          // AudioSource dedicado del tocadiscos
    public AudioClip trackClip;              // canción

    [Header("Interacción / UI")]
    public InteractableObject interactable;  // arrástralo
    public string needAllMissionsMsg = "Completa las otras misiones primero.";
    public string needDiskMsg = "Necesitas el disco para usar el tocadiscos.";
    public string activatedMsg = "- Tocadiscos activado";

    private bool activated = false;

    private void Awake()
    {
        Debug.Log("[TocadiscosMission] Awake");

        // Asegurar AudioSource no suene solo
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            if (audioSource.isPlaying) audioSource.Stop();
        }

        // Ocultar disco al inicio
        if (diskRenderer != null) diskRenderer.enabled = false;

        // Conectar con el interactable
        if (interactable == null) interactable = GetComponent<InteractableObject>();
        if (interactable != null)
        {
            interactable.tocadiscosMission = this;
            Debug.Log("[TocadiscosMission] InteractableObject enlazado.");
        }
        else
        {
            Debug.LogWarning("[TocadiscosMission] No hay InteractableObject en el tocadiscos.");
        }

        if (autoApplyGateOnStart) ApplyGateVisual();

        if (autoPollFlags) StartCoroutine(PollFlagsRoutine());
    }

    private IEnumerator PollFlagsRoutine()
    {
        while (true)
        {
            ApplyGateVisual();
            yield return new WaitForSecondsRealtime(pollInterval);
        }
    }

    private void ApplyGateVisual()
    {
        bool unlocked = flags != null && flags.AllCoreCompleted();
        if (mantaA != null) mantaA.SetActive(!unlocked);
        if (mantaB != null) mantaB.SetActive(unlocked);
        // Debug opcional:
        // Debug.Log($"[TocadiscosMission] Gate visual -> unlocked={unlocked}");
    }

    private void IntegrateAudio()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTurntableTrack();
        }
        else
        {
            Debug.LogWarning("[TocadiscosMission] AudioManager.Instance es null, no se puede cambiar música.");
        }
    }
    // Llamado desde InteractableObject.Interact()
    public void TryActivate()
    {
        Debug.Log("[TocadiscosMission] TryActivate()");
        if (activated)
        {
            Debug.Log("[TocadiscosMission] Ya activado. Ignorando.");
            return;
        }

        // Gate de misiones
        if (flags == null || !flags.AllCoreCompleted())
        {
            InteractionManager.Instance?.ShowMessage(needAllMissionsMsg);
            Debug.LogWarning($"[TocadiscosMission] Gate bloqueado. bosque={flags?.bosqueCompleted} cuarto={flags?.cuartoCompleted} bici={flags?.sotanoBikeCompleted}");
            return;
        }

        // Consumir disco del inventario
        if (InventoryManager.Instance == null || !InventoryManager.Instance.RemoveGiftByName(discoGiftName))
        {
            InteractionManager.Instance?.ShowMessage(needDiskMsg);
            Debug.LogWarning("[TocadiscosMission] Disco no encontrado en inventario.");
            return;
        }

        // Colocar disco, animar y reproducir audio
        PlaceDiskVisual();
        StartSpinning();
        IntegrateAudio();

        activated = true;
        if (flags != null) flags.tocadiscosCompleted = true;
        InteractionManager.Instance?.ShowInteraction(activatedMsg);
        Debug.Log("[TocadiscosMission] Activado. tocadiscosCompleted = TRUE");
    }

    private void PlaceDiskVisual()
    {
        if (diskRenderer == null)
        {
            Debug.LogWarning("[TocadiscosMission] diskRenderer no asignado.");
            return;
        }

        if (diskSprite != null) diskRenderer.sprite = diskSprite;
        diskRenderer.enabled = true;

        if (diskSlot != null)
        {
            diskRenderer.transform.position = diskSlot.position;
            diskRenderer.transform.rotation = diskSlot.rotation;
            // Ajusta escala si hace falta:
            // diskRenderer.transform.localScale = diskSlot.localScale;
        }
        Debug.Log("[TocadiscosMission] Disco colocado visualmente.");
    }

    private void StartSpinning()
    {
        if (tocadiscosAnimator != null)
        {
            tocadiscosAnimator.SetBool("isSpinning", true);
            if (stopAfterDuration)
            {
                Invoke(nameof(StopSpinning), spinDuration);
            }
            Debug.Log("[TocadiscosMission] Animación de giro iniciada.");
        }
        else
        {
            // Fallback: rotación manual del sprite si no tienes Animator
            StartCoroutine(SpinFallback());
            Debug.Log("[TocadiscosMission] Giro fallback (sin Animator).");
        }
    }

    private IEnumerator SpinFallback()
    {
        float t = 0f;
        while (!stopAfterDuration || t < spinDuration)
        {
            t += Time.deltaTime;
            if (diskRenderer != null)
            {
                diskRenderer.transform.Rotate(0f, 0f, -180f * Time.deltaTime); // rota a 180 grados/s
            }
            yield return null;
        }
    }

    private void StopSpinning()
    {
        if (tocadiscosAnimator != null)
        {
            tocadiscosAnimator.SetBool("isSpinning", false);
            Debug.Log("[TocadiscosMission] Giro detenido.");
        }
    }

    private void PlayTrack()
    {
        if (audioSource != null && trackClip != null)
        {
            audioSource.clip = trackClip;
            audioSource.Play();
            Debug.Log("[TocadiscosMission] Reproduciendo canción.");
        }
        else
        {
            Debug.LogWarning("[TocadiscosMission] AudioSource o trackClip no asignados.");
        }
    }

    // Llamable desde otras misiones al completar para refrescar mantaA/B
    public void OnCoreMissionsStateChanged()
    {
        ApplyGateVisual();
    }
}