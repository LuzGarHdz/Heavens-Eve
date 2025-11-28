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
            // Muerte del jugador
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDeath();
            }
            else
            {
                // Fallback local si no hay GameManager (como en la escena Cuarto)
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
        if (hearts == null || hearts.Length == 0) return;

        // Mostrar corazones habilitados/deshabilitados según los toques
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].enabled = i < (maxHits - currentHits);
        }
    }
}