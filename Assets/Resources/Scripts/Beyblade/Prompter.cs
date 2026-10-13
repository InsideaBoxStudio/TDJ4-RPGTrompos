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

        // Player 1 puede usar WASD si hay menos de 2 joysticks
        if (Gamepad.all.Count == playerIndex)
        {
            input = ApuntarWASD();
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

    private Vector2 ApuntarWASD()
    {
        Vector2 inputTeclado = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            inputTeclado.y += 1;

        if (Keyboard.current.sKey.isPressed)
            inputTeclado.y -= 1;

        if (Keyboard.current.dKey.isPressed)
            inputTeclado.x += 1;

        if (Keyboard.current.aKey.isPressed)
            inputTeclado.x -= 1;

        return inputTeclado.normalized;
    }
}