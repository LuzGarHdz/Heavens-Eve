using UnityEngine;

public class TocadiscosMission : MonoBehaviour
{
    [Header("Flags")]
    public MissionFlagsSO flags;

    [Header("Inventario")]
    public string discoGiftName = "Disco";

    [Header("Animación")]
    public Animator tocadiscosAnimator; // parámetro bool "isSpinning"
    public float spinDuration = 5f;

    public void TryActivate()
    {
        if (flags == null) { Debug.LogError("[TocadiscosMission] flags es NULL."); return; }

        if (!flags.AllCoreCompleted())
        {
            InteractionManager.Instance?.ShowMessage("Completa las otras misiones primero.");
            Debug.Log($"[TocadiscosMission] Gate bloqueado. bosque={flags.bosqueCompleted} cuarto={flags.cuartoCompleted} bici={flags.sotanoBikeCompleted}");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[TocadiscosMission] InventoryManager.Instance es null.");
            return;
        }

        bool removed = InventoryManager.Instance.RemoveGiftByName(discoGiftName);
        if (!removed)
        {
            InteractionManager.Instance?.ShowMessage("Necesitas el disco para usar el tocadiscos.");
            Debug.Log("[TocadiscosMission] Disco no encontrado en inventario.");
            return;
        }

        if (tocadiscosAnimator != null)
        {
            tocadiscosAnimator.SetBool("isSpinning", true);
            Invoke(nameof(StopSpinning), spinDuration);
        }

        flags.tocadiscosCompleted = true;
        Debug.Log("[TocadiscosMission] tocadiscosCompleted = TRUE");
        InteractionManager.Instance?.ShowInteraction("- Tocadiscos activado");
    }

    private void StopSpinning()
    {
        if (tocadiscosAnimator != null)
            tocadiscosAnimator.SetBool("isSpinning", false);
    }
}