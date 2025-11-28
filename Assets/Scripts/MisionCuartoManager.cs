using UnityEngine;

public class MisionCuartoManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject peluchesBuenosConjunto;   // Grupo de peluches buenos colocados en el cuarto (desactivado al inicio)
    public GameObject peluchePinguinoSuelo;     // El peluche de pingüino en el suelo (desactivado al inicio)
    public ClosetUI closetUI;

    [Header("Interactable Closet")]
    public InteractableObject closetInteractable;   // Asigna el InteractableObject del closet
    public Collider2D closetTrigger;                // Opcional: el collider del closet (para desactivarlo)
    public string missionTitle = "- Acomodar los peluches correctos";
    public string missionCompletedText = "- Peluche de Pingüino entregado";

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

        // Deshabilitar closet y ocultar prompt
        DisableCloset();

        // Marcar misión (si hay GameManager)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.missionCompleted = true;
        }

        InteractionManager.Instance?.ShowInteraction(missionCompletedText);
    }

    private void DisableCloset()
    {
        // 1) Marcar el interactable como deshabilitado
        if (closetInteractable != null)
            closetInteractable.isDisabled = true;

        // 2) Desactivar su collider (opcional)
        if (closetTrigger != null)
            closetTrigger.enabled = false;

        // 3) Opcional: cambiar tag para que PlayerInteraction ni siquiera lo considere
        if (closetInteractable != null)
            closetInteractable.gameObject.tag = "Untagged";

        // 4) Quitar cualquier mensaje visible
        InteractionManager.Instance?.HideMessage();
    }
}