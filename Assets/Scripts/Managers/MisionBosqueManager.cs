using UnityEngine;

public class MisionBosqueManager : MonoBehaviour
{
    [Header("Flags")]
    public MissionFlagsSO flags;

    [Header("Misión")]
    public string missionTitle = "- Encontrar los 3 regalos";
    public string missionCompletedText = "- Regalos encontrados";
    public int totalGifts = 3;

    private int giftsFound = 0;
    private bool completed = false;

    private void Start()
    {
        InteractionManager.Instance?.ShowInteraction(missionTitle);
        if (flags == null) Debug.LogError("[MisionBosqueManager] flags es NULL. Asigna el asset MissionFlags.");
    }

    public void OnGiftFound()
    {
        if (completed) return;

        giftsFound++;
        Debug.Log($"[MisionBosqueManager] Gift encontrado. Progreso {giftsFound}/{totalGifts}");
        InteractionManager.Instance?.ShowInteraction($"{missionTitle} ({giftsFound}/{totalGifts})");

        if (giftsFound >= totalGifts)
        {
            completed = true;

            if (flags != null)
            {
                flags.bosqueCompleted = true;
                Debug.Log("[MisionBosqueManager] bosqueCompleted = TRUE");
            }

            InteractionManager.Instance?.ShowInteraction(missionCompletedText);
            var tocadiscos = FindObjectOfType<TocadiscosMission>();
            if (tocadiscos != null) tocadiscos.OnCoreMissionsStateChanged();

            // NUEVO: detener el timer y mostrar 00:00 cuando se hayan recolectado todos los regalos
            Timer timer = FindObjectOfType<Timer>();
            if (timer != null)
            {
                timer.StopCountdown();
                timer.SetTime(0f);
            }
        }
    }
}