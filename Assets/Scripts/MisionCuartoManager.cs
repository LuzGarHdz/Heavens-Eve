using UnityEngine;

public class MisionCuartoManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject peluchesBuenosConjunto;
    public GameObject peluchePinguinoSuelo;
    public ClosetUI closetUI;

    [Header("Flags")]
    public MissionFlagsSO flags;

    [Header("Misión")]
    public string missionTitle = "- Acomodar los peluches correctos";
    public string missionCompletedText = "- Peluche de Pingüino entregado";

    [Header("Inventario")]
    public string pelucheGiftName = "Peluche"; // nombre exacto en GiftData.giftName

    private void Start()
    {
        if (peluchesBuenosConjunto) peluchesBuenosConjunto.SetActive(false);
        if (peluchePinguinoSuelo) peluchePinguinoSuelo.SetActive(false);
        InteractionManager.Instance?.ShowInteraction(missionTitle);
    }

    public void OnClosetOpened() { }

    public void OnClosetClosed() { }

    public void UpdateMissionProgress(int current, int total)
    {
        InteractionManager.Instance?.ShowInteraction($"{missionTitle} ({current}/{total})");
    }

    public void OnMissionCompleted()
    {
        if (peluchesBuenosConjunto) peluchesBuenosConjunto.SetActive(true);
        if (peluchePinguinoSuelo) peluchePinguinoSuelo.SetActive(true);

        if (flags != null)
        {
            flags.cuartoCompleted = true;
            Debug.Log("[MisionCuartoManager] flags.cuartoCompleted = true");
        }

        // Eliminar el regalo del peluche del inventario
        if (InventoryManager.Instance != null)
        {
            bool removed = InventoryManager.Instance.RemoveGiftByName(pelucheGiftName);
            Debug.Log($"[MisionCuartoManager] Remove '{pelucheGiftName}' del inventario: {removed}");
        }

        InteractionManager.Instance?.ShowInteraction(missionCompletedText);
    }
}