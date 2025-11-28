using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Referencia al jugador")]
    public Transform player;

    [Header("Velocidad y escalado")]
    public float baseSpeed = 0.8f;
    public float maxSpeed = 4f;
    public float missionTotalTime = 30f; // duración total de la misión para escalar velocidad

    public enum FollowMode { HorizontalOnly, FollowYClamped, Full2D }
    [Header("Modo de seguimiento")]
    public FollowMode followMode = FollowMode.FollowYClamped;

    [Header("Seguimiento vertical (si clamped)")]
    public float minY = -5f;
    public float maxY = 5f;
    public float maxVerticalSpeed = 2.0f; // qué tan rápido ajusta su Y

    [Header("Física")]
    public bool zeroGravityWhileActive = true; // fantasma: sin gravedad
    public bool triggerThroughTerrain = true;  // marcar el collider como isTrigger para atravesar suelo/obstáculos

    [Header("Daño")]
    public int hitDamage = 1;

    private Rigidbody2D rb;
    private float elapsed;
    private float currentSpeed;
    private bool chasing;
    private float yLocked; // si solo horizontal

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (zeroGravityWhileActive) rb.gravityScale = 0f;
        if (triggerThroughTerrain)
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
        }
    }

    public void ResetEnemy()
    {
        elapsed = 0f;
        currentSpeed = 0f;
        chasing = false;
        yLocked = transform.position.y;
    }

    public void BeginChase()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        chasing = true;
        elapsed = 0f;
        yLocked = transform.position.y;
    }

    public void StopChase()
    {
        chasing = false;
        rb.linearVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!chasing || player == null) return;

        // Escalar velocidad en el tiempo
        elapsed += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(elapsed / missionTotalTime);
        currentSpeed = Mathf.Lerp(baseSpeed, maxSpeed, t);

        Vector2 current = rb.position;

        // X siempre sigue al jugador con la velocidad actual
        float nextX = Mathf.MoveTowards(current.x, player.position.x, currentSpeed * Time.fixedDeltaTime);

        // Y según modo
        float nextY = current.y;

        if (followMode == FollowMode.HorizontalOnly)
        {
            nextY = yLocked;
        }
        else if (followMode == FollowMode.FollowYClamped)
        {
            float desiredY = Mathf.Clamp(player.position.y, minY, maxY);
            nextY = Mathf.MoveTowards(current.y, desiredY, maxVerticalSpeed * Time.fixedDeltaTime);
        }
        else // Full2D
        {
            float desiredY = player.position.y;
            nextY = Mathf.MoveTowards(current.y, desiredY, maxVerticalSpeed * Time.fixedDeltaTime);
        }

        rb.MovePosition(new Vector2(nextX, nextY));

        // Flip opcional
        transform.localScale = new Vector3(player.position.x < transform.position.x ? -1 : 1, 1, 1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!chasing) return;

        if (other.CompareTag("Player"))
        {
            var h = other.GetComponent<Health>();
            if (h != null) h.TakeHit(hitDamage);
        }
    }
}