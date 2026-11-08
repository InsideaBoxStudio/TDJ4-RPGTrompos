using UnityEngine;
using UnityEngine.InputSystem;

// ============================================================================
//  CADA PANEL DEL SELECTOR CON EL JOYSTICK DE SU JUGADOR
//  ----------------------------------------------------------------------------
//  Va en cada MultiplayerEventSystem del selector de piezas (P1 = 0, P2 = 1), junto
//  a su PlayerInput.
//
//  Sin esto, cada PlayerInput se quedaba con "el primer joystick libre" en el orden
//  en que Unity los activaba. Ese orden no es el mismo en el Editor que en el
//  ejecutable, así que en escritorio un jugador podía armar el trompo del otro.
//  Acá se le da a cada panel el MISMO joystick que después maneja su trompo en la
//  pelea (JoystickPorJugador), y si se desconecta y vuelve, se lo devuelve.
// ============================================================================
[RequireComponent(typeof(PlayerInput))]
public class EmparejarJugadorSelector : MonoBehaviour
{
    private const string EsquemaJoystick = "Gamepad";
    private const string EsquemaTeclado = "Keyboard&Mouse";

    [Tooltip("Jugador 1 = 0, Jugador 2 = 1.")]
    [SerializeField] private int playerIndex = 0;

    private PlayerInput playerInput;
    private Gamepad emparejado;
    private bool conTeclado;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        // Con dos jugadores no tiene que cambiar solo de dispositivo.
        playerInput.neverAutoSwitchControlSchemes = true;
    }

    // En Start: el PlayerInput ya hizo su emparejamiento automático en OnEnable, y
    // este lo corrige.
    private void Start()
    {
        Emparejar();
    }

    private void Update()
    {
        Gamepad pad = JoystickPorJugador.De(playerIndex);
        bool teclado = pad == null && JoystickPorJugador.UsaTeclado(playerIndex);
        if (pad != emparejado || teclado != conTeclado) Emparejar();
    }

    private void Emparejar()
    {
        emparejado = JoystickPorJugador.De(playerIndex);
        conTeclado = emparejado == null && JoystickPorJugador.UsaTeclado(playerIndex);

        if (!playerInput.user.valid) return; // no se emparejó con nada: no hay a quién cambiar

        try
        {
            if (emparejado != null)
            {
                playerInput.SwitchCurrentControlScheme(EsquemaJoystick, emparejado);
            }
            else if (conTeclado && Keyboard.current != null)
            {
                if (Mouse.current != null)
                    playerInput.SwitchCurrentControlScheme(EsquemaTeclado, Keyboard.current, Mouse.current);
                else
                    playerInput.SwitchCurrentControlScheme(EsquemaTeclado, Keyboard.current);
            }
        }
        catch (System.Exception e)
        {
            // Si el asset de acciones no tiene el esquema, se queda con lo que eligió
            // Unity en vez de romper el selector.
            Debug.LogWarning("EmparejarJugadorSelector (J" + (playerIndex + 1) + "): " + e.Message, this);
        }
    }
}
