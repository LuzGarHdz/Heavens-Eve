using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Vida")]
    public int maxHits = 5; // 5 toques
    public int currentHits = 0;

    [Header("UI")]
    public Image[] hearts; // Asignar 5 sprites/imagenes representando cada "vida/toque"

    [Header("Fallback (sin GameManager)")]
    public GameOverUI fallbackGameOverUI;      // Opcional: arrastra tu panel de Game Over
    public Movement playerMovement;            // Arrastra Movement del jugador
    public PlayerInteraction playerInteraction;// Arrastra PlayerInteraction del jugador

    private void Awake()
    {
        TryAutoAssignHearts();
        if (playerMovement == null) playerMovement = FindObjectOfType<Movement>();
        if (playerInteraction == null) playerInteraction = FindObjectOfType<PlayerInteraction>();
    }

    public void ResetHealth()
    {
        currentHits = 0;
        UpdateUI();
    }

    public void TakeHit(int amount = 1)
    {
        currentHits += amount;
        UpdateUI();

        if (currentHits >= maxHits)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDeath();
            }
            else
            {
                if (playerMovement != null) playerMovement.enabled = false;
                if (playerInteraction != null) playerInteraction.enabled = false;
                Time.timeScale = 0f;
                if (fallbackGameOverUI != null)
                {
                    fallbackGameOverUI.Show();
                }
                else
                {
                    Debug.LogWarning("[Health] Player murió, pero no hay GameManager ni fallbackGameOverUI asignado.");
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (hearts == null || hearts.Length == 0)
        {
            Debug.LogWarning("[Health] hearts está vacío o null; intenta auto-asignar o configura en Inspector.");
            return;
        }

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].enabled = i < (maxHits - currentHits);
        }
    }

    private void TryAutoAssignHearts()
    {
        // Si ya están asignados, no tocar
        if (hearts != null && hearts.Length > 0 && hearts.Any(h => h != null)) return;

        // Busca un contenedor con tag "HeartsContainer" (pon este tag al parent de las imágenes de corazones en tu HUD persistente)
        var container = GameObject.FindWithTag("HeartsContainer");
        if (container != null)
        {
            hearts = container.GetComponentsInChildren<Image>(includeInactive: true)
                              .Where(img => img.gameObject != container)
                              .ToArray();
            if (hearts.Length > 0)
            {
                Debug.Log($"[Health] hearts auto-asignados desde tag HeartsContainer: {hearts.Length}");
                return;
            }
        }

        // Fallback: busca cualquier Image hijo en este mismo objeto persistente
        var found = GetComponentsInChildren<Image>(includeInactive: true);
        if (found != null && found.Length > 0)
        {
            hearts = found;
            Debug.Log($"[Health] hearts auto-asignados por fallback local: {hearts.Length}");
        }
        else
        {
            Debug.LogWarning("[Health] No se encontraron imágenes de corazones; asigna en Inspector o pon tag 'HeartsContainer'.");
        }
    }
}