using UnityEngine;
using UnityEngine.InputSystem;

public class AttacksMenuPreset : MonoBehaviour
{
    public int playerIndex = 0;
    [SerializeField] private bool isSpecial = false;
    public int buttonState = -1; // Estado de los botones (0 = BotonArriba; 1 = BotonDerecha; 2 = BotonIzquierda; 3 = BotonAbajo)
    [SerializeField] private GameObject[] attackOptionsPrefabs;

    private GameObject[] optionReference;
    private Gamepad gamepad;

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

        Debug.Log(attackOptionsPrefabs);
        optionReference = new GameObject[attackOptionsPrefabs.Length]; // iniciar el array de referencias de opciones con la longitud correcta

        for (int i = 0; i < optionReference.Length; i++)
        {
            optionReference[i] = Instantiate(attackOptionsPrefabs[i], transform); // Instanciar la opcion de ataque y establecerla como hija del objeto actual
        }

        gamepad = PlayerSettings.gamepad[playerIndex]; // obtener el gamepad del jugador correspondiente

        if (gamepad == null && Gamepad.all.Count > playerIndex)
        {
            gamepad = Gamepad.all[playerIndex]; // Si no hay guardado un gamepad para el jugador pero hay un joystic conectado para el, asignarselo (el primero es del J1, el segundo del J2);
        }
    }

    void Update()
    {
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
        // Flechas para el J1, IJKL para el J2 (cada jugador tiene sus teclas, ver TeclasJugador)
        buttonState = TeclasJugador.MenuDireccion(playerIndex);
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