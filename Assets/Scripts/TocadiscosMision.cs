using UnityEngine;
using UnityEngine.UI;

public class TocadiscosMission : MonoBehaviour
{
    [Header("Flags")]
    public MissionFlagsSO flags;

    [Header("Inventario")]
    public string discoGiftName = "Disco";

    [Header("Animación")]
    public Animator tocadiscosAnimator; // parámetro bool "isSpinning"
    public float spinDuration = 5f;

    [Header("Mantas")]
    public GameObject mantaA;
    public GameObject mantaB;

    public void TryActivate()
    {
        if (!flags.AllCoreCompleted())
        {
            InteractionManager.Instance?.ShowMessage("Completa las otras misiones primero.");
            return;
        }

        // Debe tener el disco en inventario
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[TocadiscosMission] InventoryManager.Instance es null.");
            return;
        }

        bool removed = InventoryManager.Instance.RemoveGiftByName(discoGiftName);
        if (!removed)
        {
            InteractionManager.Instance?.ShowMessage("Necesitas el disco para usar el tocadiscos.");
            return;
        }

        mantaA.SetActive(false);
        mantaB.SetActive(true);

        // Animar tocadiscos
        if (tocadiscosAnimator != null)
        {
            tocadiscosAnimator.SetBool("isSpinning", true);
            Invoke(nameof(StopSpinning), spinDuration);
        }

        flags.tocadiscosCompleted = true;
        InteractionManager.Instance?.ShowInteraction("- Tocadiscos activado");
    }

    private void StopSpinning()
    {
        if (tocadiscosAnimator != null)
        {
            tocadiscosAnimator.SetBool("isSpinning", false);
        }
    }
}