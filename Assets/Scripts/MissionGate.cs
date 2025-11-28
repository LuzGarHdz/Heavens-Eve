using UnityEngine;

public class MissionGate : MonoBehaviour
{
    [Header("Flags")]
    public MissionFlagsSO flags;

    [Tooltip("Si está activo, exige que AllCoreCompleted() sea true para permitir interactuar.")]
    public bool requireAllCore = true;

    [TextArea]
    public string lockedMessage = "Completa las otras misiones primero.";

    public bool CanInteract()
    {
        if (flags == null) return true; // si no hay flags, no bloquear
        if (requireAllCore) return flags.AllCoreCompleted();
        return true;
    }

    public void ShowLockedMessage()
    {
        InteractionManager.Instance?.ShowMessage(lockedMessage);
    }
}