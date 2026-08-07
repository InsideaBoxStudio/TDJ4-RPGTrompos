using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraFollowPlayers : MonoBehaviour
{
    public List<Transform> players = new List<Transform>();

    public float smoothSpeed = 5f;
    public Vector3 offset;

    public float minZoom = 5f;
    public float maxZoom = 10f;
    public float zoomLimiter = 10f;

    public Camera cam;
    public PixelPerfectCamera ppc;

    public bool isPixelPerfect = false;

    [Header("Rotación")]
    public float rotationSmoothSpeed = 5f;

    private void Start()
    {
        players.AddRange(
            GameObject.FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .Select(go => go.transform)
        );

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].gameObject.activeSelf == false) //si el jugador no esta activo, lo elimino de la lista
            {
                players.RemoveAt(i);
                i--;
            }
        }
    }

    void LateUpdate()
    {
        if (players.Count == 0) return;

        float dt = Time.unscaledDeltaTime;

        // ========================
        // ====== Movimiento ======
        // ========================

        // Punto medio
        Bounds bounds = GetPlayersBounds();
        Vector3 midpoint = bounds.center;
        Vector3 desiredPosition = midpoint + offset;

        // Movimiento suave
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * dt);

        // ============================
        // ====== Zoom Adaptable ======
        // ============================

        float greatestDistance = Mathf.Max(bounds.size.x, bounds.size.y);

        if (isPixelPerfect && ppc != null)
        {
            // Distancia normalizada entre 0 y 1
            float t = Mathf.Clamp01(greatestDistance / zoomLimiter);

            // Cuando los jugadores están lejos usamos menor PPU (más alejado)
            int targetPPU = Mathf.RoundToInt(Mathf.Lerp( minZoom * 100, maxZoom * 100, t));

            // Cambio suave
            float smoothPPU = Mathf.Lerp(
                ppc.assetsPPU,
                targetPPU,
                smoothSpeed * dt
            );

            ppc.assetsPPU = Mathf.RoundToInt(smoothPPU);
        }
        else
        {
            float newZoom = Mathf.Lerp(
                minZoom,
                maxZoom,
                Mathf.Clamp01(greatestDistance / zoomLimiter)
            );

            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                newZoom,
                smoothSpeed * dt
            );
        }

        // ======================
        // ====== Rotacion ======
        // ======================

        Transform a = players[0];
        Transform b = players[1];

        if (players.Count > 2) // si hay mas de 2 jugadores
        {
            float maxDistance = 0f;

            for (int i = 0; i < players.Count; i++)
            {
                for (int j = i + 1; j < players.Count; j++) //recorre cada par de jugadores
                {
                    float dist = Vector3.Distance( //calcula la distancia entre el par de jugadores
                        players[i].position,
                        players[j].position
                    );

                    if (dist > maxDistance) //si la distancia es mayor a la anterior
                    {
                        maxDistance = dist; //actualiza la distancia maxima
                        a = players[i];
                        b = players[j];
                    }
                }
            }
        }

        Vector3 direction = b.position - a.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed * dt);
    }

    Bounds GetPlayersBounds()
    {
        Bounds bounds = new Bounds(players[0].position, Vector3.zero);

        for (int i = 1; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }

        return bounds;
    }
}