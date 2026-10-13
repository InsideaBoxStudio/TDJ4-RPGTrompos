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
        // Actualizar estado del turno SIEMPRE
        bool turnActive = false;

        if (rpgTurn)
        {
            turnActive = rpgTurn.isTurnActive;
        }

        // ============================
        // INPUT DE APUNTADO
        // ============================

        // Sin joystick propio apunta con el teclado: WASD el J1, IJKL el J2 (ver TeclasJugador)
        if (TeclasJugador.UsaTeclado(playerIndex))
        {
            input = TeclasJugador.Apuntar(playerIndex).normalized;
        }
        // Si hay joystick disponible para este jugador
        else if (Gamepad.all.Count > playerIndex)
        {
            input = Gamepad.all[playerIndex].rightStick.ReadValue();
        }
        else
        {
            input = Vector2.zero;
        }

        // ============================
        // ROTACIÓN
        // ============================

        if (input != Vector2.zero)
        {
            angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}