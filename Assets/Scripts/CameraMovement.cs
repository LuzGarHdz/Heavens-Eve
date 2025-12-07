using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // el jugador a seguir
    public float smoothSpeed = 0.125f; // suavidad del movimiento
    public Vector3 offset; // distancia de la cámara respecto al jugador

    // límites del movimiento de cámara
    public float minX; // límite izquierdo
    public float maxX; // límite derecho

    void LateUpdate()
    {
        if (player == null)
            return;

        // posición deseada de la cámara
        Vector3 desiredPosition = new Vector3(player.position.x + offset.x, transform.position.y, transform.position.z);

        // limitar el movimiento de la cámara entre los bordes del escenario
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

        // movimiento suave hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}

