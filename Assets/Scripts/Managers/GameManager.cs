using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Mission")]
    public bool missionStarted = false;
    public bool missionCompleted = false;
    public int regalosObjetivo = 3;
    public int regalosActuales = 0;

    [Header("Timer")]
    public Timer timer; // Asignar el componente Timer en Bosque
    public float missionDuration = 30f;
    public float enemySpawnDelay = 10f;

    [Header("Enemy")]
    public EnemyController enemyController; // Asignar prefab o referencia en escena (desactivado al inicio)

    [Header("Player/Health")]
    public Health playerHealth; // Asignar componente Health del jugador
    public Movement playerMovement; // Asignar Movement del jugador
    public PlayerInteraction playerInteraction; // Asignar PlayerInteraction del jugador

    [Header("UI")]
    public GameOverUI gameOverUI; // Asignar panel UI de Game Over (desactivado al inicio)
    public InteractionManager interactionManager; // Referencia al InteractionManager

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Mantener si deseas persistencia entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Estado inicial
        missionStarted = false;
        missionCompleted = false;
        regalosActuales = 0;

        // Asegura que el enemigo no esté activo
        if (enemyController != null)
        {
            enemyController.gameObject.SetActive(false);
        }

        // Bloquear recolección antes de hablar con NPC
        if (playerInteraction != null)
        {
            playerInteraction.interactionsLocked = true;
        }

        // Timer oculto/inactivo al comienzo (lo controla el propio Timer + UI)
        if (timer != null)
        {
            timer.enabled = false;
            timer.SetTime(missionDuration);
        }
    }

    // Llamado desde InteractionManager al hablar con NPC
    public void OnTalkedToNPC()
    {
        if (missionStarted) return;

        missionStarted = true;

        // Desbloquear interacción para regalos, pero restringir salida
        if (playerInteraction != null)
        {
            playerInteraction.interactionsLocked = false; // Puede recoger regalos
        }

        // Iniciar timer
        if (timer != null)
        {
            timer.enabled = true;
            timer.StartCountdown();
        }

        // Programar aparición de enemigo a los 10s
        if (enemyController != null)
        {
            Invoke(nameof(SpawnEnemy), enemySpawnDelay);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyController == null) return;
        enemyController.ResetEnemy();
        enemyController.gameObject.SetActive(true);
        enemyController.BeginChase();
    }

    public void OnRegaloRecolectado()
    {
        if (!missionStarted) return;

        regalosActuales++;
        if (interactionManager != null)
        {
            interactionManager.ShowInteraction($"- Regalos: {regalosActuales}/{regalosObjetivo}");
        }

        if (regalosActuales >= regalosObjetivo)
        {
            missionCompleted = true;
            // Puedes bloquear enemigo o finalizar misión
            if (enemyController != null)
            {
                enemyController.StopChase();
            }
            // Aquí puedes permitir salida o transición de escena
            // Por requerimiento: no salir hasta encontrar los 3 regalos, ahora ya puede.
        }
    }

    public void OnTimeExpired()
    {
        if (!missionCompleted)
        {
            TriggerGameOver();
        }
    }

    public void OnPlayerDeath()
    {
        TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        // Detener movimiento y lógica
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerInteraction != null) playerInteraction.enabled = false;

        if (enemyController != null) enemyController.StopChase();

        Time.timeScale = 0f; // Pausar el juego

        if (gameOverUI != null)
        {
            gameOverUI.Show();
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Implementar la escena
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}