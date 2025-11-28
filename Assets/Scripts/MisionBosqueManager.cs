using UnityEngine;

public class MisionBosqueManager : MonoBehaviour
{
    [Header("Flags")]
    public MissionFlagsSO flags;

    [Header("Misión")]
    public string missionTitle = "- Encontrar los 3 regalos";
    public string missionCompletedText = "- Regalos encontrados";

    [Header("Diálogo NPC Alma")]
    public AlmaBosqueNPC almaNPC;

    private int giftsFound = 0;
    public int totalGifts = 3;

    private void Start()
    {
        InteractionManager.Instance?.ShowInteraction(missionTitle);
    }

    public void OnGiftFound()
    {
        giftsFound++;
        InteractionManager.Instance?.ShowInteraction($"{missionTitle} ({giftsFound}/{totalGifts})");

        if (giftsFound >= totalGifts)
        {
            flags.bosqueCompleted = true;
            InteractionManager.Instance?.ShowInteraction(missionCompletedText);
            Debug.Log("[MisionBosqueManager] flags.bosqueCompleted = true");
        }

        // Notificar a Alma para diálogo de cierre
        if (almaNPC != null)
            almaNPC.NotifyMissionCompleted();
    }
}