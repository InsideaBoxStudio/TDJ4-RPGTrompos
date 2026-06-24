using UnityEngine;
using UnityEngine.InputSystem;

public class Prompter : MonoBehaviour
{
    private GameObject cam;
    private GameObject pointerPlayer;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    
    [SerializeField] private int playerIndex = 0;

    private float angle;
    private Vector2 input;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        BuscarRival();
    }

    // Busca al rival (otro Player activo en otra layer). Reintentable: el rival
    // puede activarse tarde por el sistema de selección de personajes.
    private void BuscarRival()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player != gameObject &&
                player.activeInHierarchy &&
                player.layer != gameObject.layer)
            {
                pointerPlayer = player;
                return;
            }
        }
    }

    void Update()
    {
        // actualizar estado del turno SIEMPRE
        bool turnActive = false;
        if (rpgTurn) {
            turnActive = rpgTurn.isTurnActive;
        }

        // verificar que exista el gamepad
        if (Gamepad.all.Count > playerIndex)
        {
            input = Gamepad.all[playerIndex].rightStick.ReadValue();
        }

        // >>> TECLADO <<< si se toca WASD/IJKL, apuntar con el teclado
        Vector2 teclas = TeclasJugador.Apuntar(playerIndex);
        if (teclas != Vector2.zero) input = teclas;
        // <<< TECLADO <<<

        if (turnActive && input != Vector2.zero)
        {
            // Obtener direcciones de la cámara
            Vector3 camForward = cam.transform.up;     // eje Y de la cámara en 2D
            Vector3 camRight = cam.transform.right;    // eje X de la cámara

            // Convertir input a dirección en mundo
            Vector3 moveDir = camRight * input.x + camForward * input.y;

            // Calcular ángulo
            angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        }
        else if (!turnActive)
        {
            // Si todavía no encontró al rival (se activó tarde), reintentar.
            if (pointerPlayer == null) BuscarRival();
            if (pointerPlayer != null)
            {
                Vector2 direction = (pointerPlayer.transform.position - transform.position).normalized;
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}