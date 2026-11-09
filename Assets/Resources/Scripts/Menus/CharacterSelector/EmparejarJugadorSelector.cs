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
    private float proximoReintento;

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
        Gamepad pad = JoystickPorJugador.De(playerIndex);
        bool teclado = pad == null && JoystickPorJugador.UsaTeclado(playerIndex);

        // Si al activarse no había ningún dispositivo libre, el PlayerInput se quedó
        // sin usuario. Apagarlo y prenderlo lo hace emparejarse de nuevo; después se
        // corrige abajo con el dispositivo que corresponde.
        if (!playerInput.user.valid && (pad != null || teclado))
        {
            if (Time.unscaledTime < proximoReintento) return;
            proximoReintento = Time.unscaledTime + 1f; // como mucho una vez por segundo
            playerInput.enabled = false;
            playerInput.enabled = true;
        }

        // Sin usuario todavía: no se guarda el estado, así el Update lo reintenta.
        if (!playerInput.user.valid) return;

        emparejado = pad;
        conTeclado = teclado;

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
            else
            {
                // Ni joystick ni teclado (por ejemplo, el J2 cuando se desconecta el
                // único joystick y el teclado pasa al J1): se le saca lo que tenía,
                // para que una tecla no mueva los dos cursores.
                playerInput.user.UnpairDevices();
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
