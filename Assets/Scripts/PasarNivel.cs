using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PasarNivelPorPosicion : MonoBehaviour
{
    [SerializeField] private string nombreSiguienteNivel;  // Nombre de la escena destino
    [SerializeField] private float limiteX = 10f;          // Coordenada X que el jugador debe alcanzar
    public static PasarNivelPorPosicion instance;
    [SerializeField] Animator transitionAnim;

    private Transform jugador;

    private void Awake()
    {
        if (instance== null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // Busca el jugador por tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            jugador = playerObj.transform;


        }
        else
        {
            Debug.LogError("No se encontró un objeto con el tag 'Player'");
        }
    }

    void Update()
    {
        if (jugador == null) return;

        // Si el jugador supera la coordenada X indicada
        if (jugador.position.x >= limiteX - 0.1f)
        {
            StartCoroutine(LoadLevel());
            enabled = false; // evita múltiples llamadas
        }
    }

    IEnumerator LoadLevel()
    {
        transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(1f);
        nombreSiguienteNivel = ("Casa");
        SceneManager.LoadScene(nombreSiguienteNivel);
        transitionAnim.SetTrigger("Start");

    }


}
