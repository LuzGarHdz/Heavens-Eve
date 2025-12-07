using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// Permite cambiar de escena presionando una tecla (Q) cuando el jugador
/// entra en una zona trigger. Muestra un mensaje tipo: "[Q] para ir a Casa".
/// Adem�s, puedes definir en el inspector las coordenadas de spawn del jugador
/// en la escena destino SIN crear m�s scripts.
///
/// Uso:
/// - A�ade este script a un GameObject con un Collider2D marcado como isTrigger.
/// - Asigna el nombre de la escena destino en 'nombreSiguienteNivel'.
/// - (Opcional) Activa 'usarSpawnCoordenadas' y define 'spawnDestino'.
/// - (Opcional) Asigna un TMP_Text para el mensaje. Si lo dejas vac�o y tienes InteractionManager, lo usar�.
/// - (Opcional) Si 'requiereMisionCompletada' est� en true, solo permitir� el cambio cuando la misi�n est� completa.
public class PasarNivelPorTecla : MonoBehaviour
{
    [Header("Configuraci�n de nivel")]
    [SerializeField] private string nombreSiguienteNivel = "Casa";
    [SerializeField] private bool requiereMisionCompletada = true;

    [Header("Spawn destino (opcional)")]
    [SerializeField] private bool usarSpawnCoordenadas = false;
    [SerializeField] private Vector2 spawnDestino = Vector2.zero;

    [Header("Interfaz")]
    [SerializeField] private string mensajeBase = "[Q] para ir a ";
    [SerializeField] private TMP_Text textoUI; // opcional. Si es null, intentar� usar InteractionManager
    [SerializeField] private bool ocultarTextoAlSalir = true;

    [Header("Transici�n")]
    [SerializeField] private Animator transitionAnim;
    [SerializeField] private float delayTransicion = 1f; // segundos

    [Header("Input")]
    [SerializeField] private KeyCode teclaCambio = KeyCode.Q;

    private bool jugadorDentro = false;
    private bool cargando = false;

    // Variables est�ticas para pasar el spawn sin crear m�s scripts
    private static bool s_tieneSpawnPendiente = false;
    private static Vector2 s_spawnPendiente;
    private static string s_escenaObjetivo = null;
    private static bool s_escuchandoSceneLoaded = false;

    private void OnEnable()
    {
        AsegurarEscuchaSceneLoaded();
    }

    private void Start()
    {
        if (textoUI != null) textoUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!jugadorDentro || cargando) return;

        if (requiereMisionCompletada && GameManager.Instance != null && !GameManager.Instance.missionCompleted)
            return;

        if (Input.GetKeyDown(teclaCambio))
        {
            // Guardar spawn para la escena destino si aplica
            if (usarSpawnCoordenadas)
            {
                s_tieneSpawnPendiente = true;
                s_spawnPendiente = spawnDestino;
                s_escenaObjetivo = nombreSiguienteNivel;
            }

            StartCoroutine(CargarNivel());
        }
    }

    private IEnumerator CargarNivel()
    {
        cargando = true;

        if (transitionAnim != null)
            transitionAnim.SetTrigger("End");

        // Usar tiempo real para no depender de Time.timeScale (por si el juego est� en pausa)
        yield return new WaitForSecondsRealtime(delayTransicion);

        SceneManager.LoadScene(nombreSiguienteNivel);

        if (transitionAnim != null)
            transitionAnim.SetTrigger("Start");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        jugadorDentro = true;
        MostrarMensaje();
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        jugadorDentro = false;
        if (ocultarTextoAlSalir) OcultarMensaje();
    }

    private void MostrarMensaje()
    {
        string txt = mensajeBase + nombreSiguienteNivel;
        if (requiereMisionCompletada && GameManager.Instance != null && !GameManager.Instance.missionCompleted)
            txt = "Primero completa la misi�n.";

        if (textoUI != null)
        {
            textoUI.text = txt;
            textoUI.gameObject.SetActive(true);
        }
        else if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.ShowMessage(txt);
        }
        else
        {
            Debug.Log($"[PasarNivelPorTecla] {txt}");
        }
    }

    private void OcultarMensaje()
    {
        if (textoUI != null)
            textoUI.gameObject.SetActive(false);
        else if (InteractionManager.Instance != null)
            InteractionManager.Instance.HideMessage();
    }

    // --------------------- Manejo de spawn est�tico ---------------------

    private static void AsegurarEscuchaSceneLoaded()
    {
        if (!s_escuchandoSceneLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoadedEstatico;
            s_escuchandoSceneLoaded = true;
        }
    }

    private static void OnSceneLoadedEstatico(Scene scene, LoadSceneMode mode)
    {
        if (!s_tieneSpawnPendiente) return;
        if (!string.IsNullOrEmpty(s_escenaObjetivo) && scene.name != s_escenaObjetivo) return;

        // Buscar al jugador y reposicionarlo
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var rb2d = playerObj.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.position = s_spawnPendiente;
                rb2d.linearVelocity = Vector2.zero;
            }
            else
            {
                playerObj.transform.position = s_spawnPendiente;
            }
        }
        else
        {
            Debug.LogWarning("[PasarNivelPorTecla] No se encontr� 'Player' en la escena destino para aplicar el spawn.");
        }

        // Limpiar flags
        s_tieneSpawnPendiente = false;
        s_escenaObjetivo = null;
    }

#if UNITY_EDITOR
    // Dibuja una marca en la escena para visualizar el spawnDestino
    private void OnDrawGizmosSelected()
    {
        if (!usarSpawnCoordenadas) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnDestino, 0.2f);
        UnityEditor.Handles.Label(spawnDestino + Vector2.up * 0.3f, $"Spawn destino: {spawnDestino}");
    }
#endif
}