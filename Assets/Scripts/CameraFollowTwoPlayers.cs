using UnityEngine;

public class CameraFollowTwoPlayers : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    public float smoothSpeed = 5f;
    public Vector3 offset;

    public float minZoom = 5f;
    public float maxZoom = 10f;
    public float zoomLimiter = 10f;

    public Camera cam;

    [Header("Rotación")]
    public float rotationSmoothSpeed = 5f;

    void LateUpdate()
    {
        if (player1 == null || player2 == null)
            return;

        float dt = Time.unscaledDeltaTime;

        // Punto medio
        Vector3 midpoint = (player1.position + player2.position) / 2f;
        Vector3 desiredPosition = midpoint + offset;

        // Movimiento suave
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * dt);

        // Distancia entre jugadores (zoom)
        float distance = Vector3.Distance(player1.position, player2.position);
        float newZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, dt);

        // ROTACIÓN SUAVE
        Vector3 direction = player2.position - player1.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed * dt);
    }
}