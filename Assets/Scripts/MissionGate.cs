using UnityEngine;

public class MissionGate : MonoBehaviour
{
    public MissionFlagsSO flags;
    public bool requireAllCore = true;
    public string lockedMessage = "Completa primero las misiones necesarias.";

    public bool CanInteract()
    {
        if (flags == null)
        {
            Debug.LogError("[MissionGate] flags es NULL.");
            return false;
        }
        bool can = !requireAllCore || flags.AllCoreCompleted();
        Debug.Log($"[MissionGate] CanInteract={can} (bosque={flags.bosqueCompleted} cuarto={flags.cuartoCompleted} bici={flags.sotanoBikeCompleted})");
        return can;
    }

    public void ShowLockedMessage()
    {
        InteractionManager.Instance?.ShowMessage(lockedMessage);
    }
}