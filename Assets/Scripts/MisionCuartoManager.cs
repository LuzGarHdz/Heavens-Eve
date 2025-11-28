using UnityEngine;

public class MisionCuartoManager : MonoBehaviour
{
    [Header("Refs")]
    public GameObject peluchesBuenosConjunto;   // Grupo de peluches buenos colocados en el cuarto (desactivado al inicio)
    public GameObject peluchePinguinoSuelo;     // El peluche de pingüino en el suelo (desactivado al inicio)
    public ClosetUI closetUI;

    [Header("Misión")]
    public string missionTitle = "- Acomodar los peluches correctos";
    public string missionCompletedText = "- Peluche de Pingüino entregado";

    private void Start()
    {
        if (peluchesBuenosConjunto) peluchesBuenosConjunto.SetActive(false);
        if (peluchePinguinoSuelo) peluchePinguinoSuelo.SetActive(false);

        // Mostrar misión (si deseas al entrar a la escena)
        InteractionManager.Instance?.ShowInteraction(missionTitle);
    }

    public void OnClosetOpened()
    {
        // Si quieres cambiar algo al abrir closet, hazlo aquí
    }

    public void OnClosetClosed()
    {
        // Al cerrar sin completar, no pasa nada especial
    }

    public void UpdateMissionProgress(int current, int total)
    {
        InteractionManager.Instance?.ShowInteraction($"{missionTitle} ({current}/{total})");
    }

    public void OnMissionCompleted()
    {
        // Activar elementos finales en la habitación
        if (peluchesBuenosConjunto) peluchesBuenosConjunto.SetActive(true);
        if (peluchePinguinoSuelo) peluchePinguinoSuelo.SetActive(true);

        // Marcar misión completada a nivel global si quieres
        if (GameManager.Instance != null)
        {
            GameManager.Instance.missionCompleted = true;
            // También podrías tener una bandera específica de “Cuarto completado”
        }

        InteractionManager.Instance?.ShowInteraction(missionCompletedText);
    }
}