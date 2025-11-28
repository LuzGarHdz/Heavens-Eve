using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Permite pasar al siguiente nivel presionando una tecla (Q) cuando el jugador
/// está dentro de una zona (collider con isTrigger) y opcionalmente cuando la misión se haya completado.
/// Muestra un texto del estilo: "[Q] para ir a Casa".
/// Estilo basado en PasarNivelPorPosicion.
/// </summary>
public class PasarNivelPorTecla : MonoBehaviour
{
    [Header("Configuración de nivel")]
    [SerializeField] private string nombreSiguienteNivel = "Casa";
    [SerializeField] private bool requiereMisionCompletada = true;

    [Header("Interfaz")]
    [SerializeField] private string mensajeBase = "[Q] para ir a ";
    [SerializeField] private TMP_Text textoUI; // Texto en pantalla
    [SerializeField] private bool ocultarTextoAlSalir = true;

    [Header("Transición")]
    [SerializeField] private Animator transitionAnim;
    [SerializeField] private float delayTransicion = 1f;

    [Header("Input")]
    [SerializeField] private KeyCode teclaCambio = KeyCode.Q;

    private Transform jugador;
    private bool jugadorDentro = false;
    private bool cargando = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            jugador = playerObj.transform;
        else
            Debug.LogError("No se encontró un objeto con el tag 'Player'");

        // Si el texto existe, lo ocultamos al inicio
        if (textoUI != null)
            textoUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!jugadorDentro || cargando) return;

        // Validar condición de misión si corresponde
        if (requiereMisionCompletada && GameManager.Instance != null && !GameManager.Instance.missionCompleted)
            return;

        // Escuchar la tecla
        if (Input.GetKeyDown(teclaCambio))
        {
            StartCoroutine(CargarNivel());
        }
    }

    private IEnumerator CargarNivel()
    {
        cargando = true;

        if (transitionAnim != null)
            transitionAnim.SetTrigger("End");

        yield return new WaitForSeconds(delayTransicion);

        SceneManager.LoadScene(nombreSiguienteNivel);

        if (transitionAnim != null)
            transitionAnim.SetTrigger("Start");
    }

    // Detecta entrada del jugador en la zona trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        jugadorDentro = true;

        // Mostrar mensaje si se cumple (o todavía no, pero anunciamos)
        MostrarMensaje();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        jugadorDentro = false;

        if (ocultarTextoAlSalir)
            OcultarMensaje();
    }

    private void MostrarMensaje()
    {
        string textoFinal = mensajeBase + nombreSiguienteNivel;

        // Si requiere misión y aún no está completada, podrías mostrar otro mensaje
        if (requiereMisionCompletada && GameManager.Instance != null && !GameManager.Instance.missionCompleted)
        {
            textoFinal = "Primero completa la misión.";
        }

        if (textoUI != null)
        {
            textoUI.text = textoFinal;
            textoUI.gameObject.SetActive(true);
        }
        else if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.ShowMessage(textoFinal);
        }
        else
        {
            Debug.Log($"[PasarNivelPorTecla] {textoFinal}");
        }
    }

    private void OcultarMensaje()
    {
        if (textoUI != null)
            textoUI.gameObject.SetActive(false);
        else if (InteractionManager.Instance != null)
            InteractionManager.Instance.HideMessage();
    }
}