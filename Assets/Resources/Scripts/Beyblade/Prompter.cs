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
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);

        cam = GameObject.FindGameObjectWithTag("MainCamera");

        BuscarRival();
    }

    // Busca al rival activo que se encuentra en otra layer.
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

        pointerPlayer = null;
    }

    private void Update()
    {
        // Consultar siempre el estado del turno.
        bool turnActive = rpgTurn != null && rpgTurn.isTurnActive;

        if (turnActive)
        {
            // ============================
            // APUNTADO MANUAL
            // ============================

            input = Vector2.zero;

            // Apuntado con joystick.
            if (Gamepad.all.Count > playerIndex)
            {
                input = Gamepad.all[playerIndex].rightStick.ReadValue();
            }

            // Apuntado con teclado.
            Vector2 teclas = TeclasJugador.Apuntar(playerIndex);

            if (teclas != Vector2.zero)
            {
                input = teclas;
            }

            // Rotar solamente si existe una entrada de apuntado.
            if (input != Vector2.zero)
            {
                if (cam != null)
                {
                    // Convertir la dirección de entrada a coordenadas del mundo.
                    Vector3 camRight = cam.transform.right;
                    Vector3 camForward = cam.transform.up;

                    Vector3 moveDir =
                        camRight * input.x +
                        camForward * input.y;

                    angle = Mathf.Atan2(
                        moveDir.y,
                        moveDir.x
                    ) * Mathf.Rad2Deg;
                }
                else
                {
                    // Alternativa si no se encuentra la cámara.
                    angle = Mathf.Atan2(
                        input.y,
                        input.x
                    ) * Mathf.Rad2Deg;
                }

                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        else
        {
            // ============================
            // APUNTADO AUTOMÁTICO AL RIVAL
            // ============================

            // Buscar al rival nuevamente si todavía no existe.
            if (pointerPlayer == null ||
                !pointerPlayer.activeInHierarchy)
            {
                BuscarRival();
            }

            if (pointerPlayer != null)
            {
                Vector2 direction =
                    pointerPlayer.transform.position -
                    transform.position;

                if (direction != Vector2.zero)
                {
                    angle = Mathf.Atan2(
                        direction.y,
                        direction.x
                    ) * Mathf.Rad2Deg;

                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }
    }
}