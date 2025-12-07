using UnityEngine;

public class SnowFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float offsetY = 3f; // Altura sobre el jugador


    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.position.x+10, player.position.y + offsetY, transform.position.z);
        }
    }
}
