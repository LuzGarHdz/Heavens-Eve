using UnityEngine;

public class MisionCuartoManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject peluchesBuenosConjunto;
    public GameObject peluchePinguinoSuelo;
    public ClosetUI closetUI;

    [Header("Flags")]
    public MissionFlagsSO flags;

    [Header("Inventario")]
    public string pelucheGiftName = "Peluche";

    [Header("Misión")]
    public string missionTitle = "- Acomodar los peluches correctos";
    public string missionCompletedText = "- Peluche de Pingüino entregado";

    private bool completed = false;

    private void Start()
    {
        if (peluchesBuenosConjunto) peluchesBuenosConjunto.SetActive(false);
        if (peluchePinguinoSuelo) peluchePinguinoSuelo.SetActive(false);

        if (closetUI != null)
        {
            // Si el ClosetUI necesita esta referencia, asegúrate que esté asignada
            Debug.Log("[MisionCuartoManager] ClosetUI asignado.");
        }

        InteractionManager.Instance?.ShowInteraction(missionTitle);
        if (flags == null) Debug.LogError("[MisionCuartoManager] flags es NULL. Asigna el asset MissionFlags.");
    }

    // Llamado por ClosetUI cuando el closet se abre
    public void OnClosetOpened()
    {
        Debug.Log("[MisionCuartoManager] Closet abierto.");
        // Aquí puedes pausar al jugador, mostrar hint, etc.
    }

    // Llamado por ClosetUI cuando el closet se cierra
    public void OnClosetClosed()
    {
        Debug.Log("[MisionCuartoManager] Closet cerrado.");
        // Aquí puedes reanudar al jugador, limpiar hints, etc.
    }

    // Llamado por ClosetUI para mostrar progreso (ej. cuántos peluches correctos colocados)
    public void UpdateMissionProgress(int current, int total)
    {
        Debug.Log($"[MisionCuartoManager] Progreso closet: {current}/{total}");
        InteractionManager.Instance?.ShowInteraction($"{missionTitle} ({current}/{total})");
    }

    // Llama ClosetUI cuando se completa la misión
    public void OnMissionCompleted()
    {
        if (completed) return;
        completed = true;

        if (peluchesBuenosConjunto) peluchesBuenosConjunto.SetActive(true);
        if (peluchePinguinoSuelo) peluchePinguinoSuelo.SetActive(true);

        if (flags != null)
        {
            flags.cuartoCompleted = true;
            Debug.Log("[MisionCuartoManager] cuartoCompleted = TRUE");
        }

        // Quitar del inventario el peluche
        if (InventoryManager.Instance != null)
        {
            bool removed = InventoryManager.Instance.RemoveGiftByName(pelucheGiftName);
            Debug.Log($"[MisionCuartoManager] Remove '{pelucheGiftName}' del inventario: {removed}");
        }

        InteractionManager.Instance?.ShowInteraction(missionCompletedText);
        var tocadiscos = FindObjectOfType<TocadiscosMission>();
        if (tocadiscos != null) tocadiscos.OnCoreMissionsStateChanged();
    }
}