using UnityEngine;
using UnityEngine.InputSystem;

public class AttacksMenuPreset : MonoBehaviour
{
    public int playerIndex = 0;
    [SerializeField] private bool isSpecial = false;
    public int buttonState = -1; // Estado de los botones (0 = BotonArriba; 1 = BotonDerecha; 2 = BotonIzquierda; 3 = BotonAbajo)
    [SerializeField] private GameObject[] attackOptionsPrefabs;

    // La CPU (AIBrain) crea sus propios menús de ataques y los marca con esto: el menú
    // arma sus opciones igual, pero NO lee ni el joystick ni el teclado. Sin esto, la CPU
    // (jugador 2) tomaría las flechas del teclado y los ataques se dispararían cuando el
    // jugador humano mueve su propio menú.
    [HideInInspector] public bool controladoPorIA = false;

    private GameObject[] optionReference;
    private Gamepad gamepad;

    // Lo llama la IA justo después de crear el componente (antes de su Start): le dice de
    // qué jugador es y si es el menú de Especiales o el de Ataques.
    public void ConfigurarParaIA(int jugador, bool especial)
    {
        playerIndex = jugador;
        isSpecial = especial;
        controladoPorIA = true;
    }

    void Start()
    {
        if (!isSpecial)
        {
            attackOptionsPrefabs = PlayerSettings.attackOptionsPrefabs[playerIndex]; // Obtener las opciones de ataque del jugador correspondiente
        }
        else
        {
            attackOptionsPrefabs = PlayerSettings.specialOptionsPrefabs[playerIndex]; // Obtener las opciones de ataque del jugador correspondiente
        }

        if (attackOptionsPrefabs == null)
        {
            Debug.LogWarning($"AttacksMenuPreset ({name}): el jugador {playerIndex} no tiene opciones de " +
                             (isSpecial ? "especiales" : "ataque") + " cargadas en PlayerSettings.");
            enabled = false;
            return;
        }

        Debug.Log(attackOptionsPrefabs);
        optionReference = new GameObject[attackOptionsPrefabs.Length]; // iniciar el array de referencias de opciones con la longitud correcta

        for (int i = 0; i < optionReference.Length; i++)
        {
            optionReference[i] = Instantiate(attackOptionsPrefabs[i], transform); // Instanciar la opcion de ataque y establecerla como hija del objeto actual
        }

        // Oscurece las opciones que el jugador no puede pagar con su energia actual
        gameObject.AddComponent<ColorSinEnergia>().Iniciar(optionReference, playerIndex);

        gamepad = PlayerSettings.gamepad[playerIndex]; // obtener el gamepad del jugador correspondiente

        if (gamepad == null && Gamepad.all.Count > 0 && playerIndex == 0)
        {
            gamepad = Gamepad.all[0]; // Si no hay guardado un gamepad para el jugador pero hay un joystic conectado, asignarle el primer joystic conectado;
        }
    }

    void Update()
    {
        if (controladoPorIA)
        {
            buttonState = -1; // la IA llama a DoAttack() directo; nadie "aprieta" el menú
            return;
        }

        if (gamepad != null) // si hay un gamepad asignado, usar la entrada del joystick
        {
            joystickInput();
        }
        else // si no hay un gamepad asignado, usar la entrada del teclado
        {
            keyboardInput();
        }
    }

    private void keyboardInput()
    {
        if (Keyboard.current.upArrowKey.isPressed) // Arriba
        {
            buttonState = 0;
        }
        else if (Keyboard.current.rightArrowKey.isPressed) // Derecha
        {
            buttonState = 1;
        }
        else if (Keyboard.current.leftArrowKey.isPressed) // Izquierda
        {
            buttonState = 2;
        }
        else if (Keyboard.current.downArrowKey.isPressed) // Abajo
        {
            buttonState = 3;
        }
        else // Ninguno
        {
            buttonState = -1;
        }
    }

    private void joystickInput()
    {
        if (gamepad.dpad.up.isPressed) // Arriba
        {
            buttonState = 0;
        }
        else if (gamepad.dpad.right.isPressed) // Derecha
        {
            buttonState = 1;
        }
        else if (gamepad.dpad.left.isPressed) // Izquierda
        {
            buttonState = 2;
        }
        else if (gamepad.dpad.down.isPressed) // Abajo
        {
            buttonState = 3;
        }
        else // Ninguno
        {
            buttonState = -1;
        }
    }
}