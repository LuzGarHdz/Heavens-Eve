using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Vida")]
    public int maxHits = 5; // 5 toques
    public int currentHits = 0;

    [Header("UI")]
    public Image[] hearts; // Asignar 5 sprites/imagenes representando cada "vida/toque"

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
            GameManager.Instance.OnPlayerDeath();
        }
    }

    private void UpdateUI()
    {
        if (hearts == null || hearts.Length == 0) return;

        // Mostrar corazones llenos vs vacíos (si usas sprites diferentes)
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < (maxHits - currentHits);
        }
    }
}