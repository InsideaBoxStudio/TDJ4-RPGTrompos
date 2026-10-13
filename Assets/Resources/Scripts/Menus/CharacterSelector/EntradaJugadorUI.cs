using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

// Le da a cada MultiplayerEventSystem del selector los controles de SU jugador.
//
// Por que hace falta: los dos event systems comparten las mismas acciones de UI
// (InputSystem_Actions), y esas acciones escuchan TODOS los dispositivos. Resultado: cualquier
// joystick o tecla movia la seleccion de los dos jugadores a la vez, y el J2 sin joystick no
// tenia teclas propias. Aca cada jugador arma sus propias acciones atadas a un solo dispositivo.
//
// Reparto (igual que en la pelea, ver TeclasJugador.UsaTeclado): el primer joystick conectado
// es del J1 y el segundo del J2. Quien no tiene joystick juega con el teclado:
//   J1 = WASD + Espacio (aceptar) + Esc (volver)
//   J2 = flechas + Enter (aceptar) + Backspace (volver)
// El mouse sigue andando para los dos (no se toca el puntero ni el click).
[RequireComponent(typeof(InputSystemUIInputModule))]
public class EntradaJugadorUI : MonoBehaviour
{
    [Tooltip("0 = Jugador 1 (izquierda), 1 = Jugador 2 (derecha).")]
    [SerializeField] private int playerIndex = 0;

    private InputSystemUIInputModule modulo;
    private InputActionMap mapa;

    // Cantidad de joysticks con la que se armaron los controles: si cambia (alguien conecta
    // o desconecta uno con el selector abierto) se rearma, porque cambia quien usa el teclado.
    private int joysticksArmados = -1;

    private void Awake()
    {
        modulo = GetComponent<InputSystemUIInputModule>();
    }

    // Se arma en Start y no en Awake: el modulo engancha sus acciones en su OnEnable, y si se
    // las cambiamos antes quedarian las viejas enganchadas.
    private void Start()
    {
        Armar();
    }

    private void Update()
    {
        if (Gamepad.all.Count != joysticksArmados) Armar();
    }

    private void OnDestroy()
    {
        Soltar();
    }

    private void Armar()
    {
        joysticksArmados = Gamepad.all.Count;

        InputActionMap nuevo = new InputActionMap("Seleccion_J" + (playerIndex + 1));
        InputAction mover = nuevo.AddAction("Mover", InputActionType.Value, expectedControlLayout: "Vector2");
        InputAction aceptar = nuevo.AddAction("Aceptar", InputActionType.Button);
        InputAction volver = nuevo.AddAction("Volver", InputActionType.Button);

        if (playerIndex < Gamepad.all.Count)
        {
            // Joystick propio: solo ese dispositivo, para que el del otro jugador no lo mueva.
            mover.AddBinding("<Gamepad>/dpad");
            mover.AddBinding("<Gamepad>/leftStick", processors: "stickDeadzone");
            aceptar.AddBinding("<Gamepad>/buttonSouth");
            volver.AddBinding("<Gamepad>/buttonEast");
            nuevo.devices = new InputDevice[] { Gamepad.all[playerIndex] };
        }
        else
        {
            if (playerIndex == 0)
            {
                mover.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
                aceptar.AddBinding("<Keyboard>/space");
                volver.AddBinding("<Keyboard>/escape");
            }
            else
            {
                mover.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                    .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
                aceptar.AddBinding("<Keyboard>/enter");
                aceptar.AddBinding("<Keyboard>/numpadEnter");
                volver.AddBinding("<Keyboard>/backspace");
            }

            if (Keyboard.current != null) nuevo.devices = new InputDevice[] { Keyboard.current };
        }

        // Primero se le entregan al modulo (que suelta las acciones viejas) y recien despues se
        // destruye el mapa anterior.
        modulo.move = InputActionReference.Create(mover);
        modulo.submit = InputActionReference.Create(aceptar);
        modulo.cancel = InputActionReference.Create(volver);
        nuevo.Enable();

        Soltar();
        mapa = nuevo;
    }

    private void Soltar()
    {
        if (mapa == null) return;

        mapa.Disable();
        mapa.Dispose();
        mapa = null;
    }
}
