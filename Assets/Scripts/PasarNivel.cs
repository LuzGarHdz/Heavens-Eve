using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PasarNivelPorPosicion : MonoBehaviour
{
    [SerializeField] private string nombreSiguienteNivel = "Casa";
    [SerializeField] private float limiteX = 10f;
    [SerializeField] Animator transitionAnim;

    private Transform jugador;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            jugador = playerObj.transform;
        else
            Debug.LogError("No se encontró un objeto con el tag 'Player'");
    }

    void Update()
    {
        if (jugador == null) return;

        // Bloquear salida si no ha completado la misión
        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.missionCompleted) return;
        }

        if (jugador.position.x >= limiteX - 0.1f)
        {
            StartCoroutine(LoadLevel());
            enabled = false;
        }
    }

    IEnumerator LoadLevel()
    {
        if (transitionAnim != null) transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nombreSiguienteNivel);
        if (transitionAnim != null) transitionAnim.SetTrigger("Start");
    }
}