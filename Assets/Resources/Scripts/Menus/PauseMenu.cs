using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject[] options;
    private int optionSelected = 0;
    private bool paused = false;

    void Update()
    {
        if (paused)
        {
            PauseEnabled();
            return;
        }

        // Antes esto recorria los gamepads con un "if (!...) return;" adentro del
        // for: con el primer joystick que NO estuviera apretando el boton cortaba
        // el metodo entero, y con CERO joysticks el bucle no corria nunca -> no
        // se podia pausar. Ahora lo maneja cualquiera de los dos, con joystick o
        // con Esc.
        if (Controles.Pausa())
        {
            optionsMenu.SetActive(false);
            paused = true;
        }
    }

    private void PauseEnabled()
    {
        optionsMenu.SetActive(true);
        Time.timeScale = 0f;

        // Salir de la pausa
        if (Controles.Pausa())
        {
            paused = false;
            optionsMenu.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        if (options == null || options.Length == 0) return;

        // Navegar el menu (flechas o dpad)
        if (Controles.MenuArriba())
        {
            optionSelected = (optionSelected > 0) ? optionSelected - 1 : options.Length - 1;
        }
        else if (Controles.MenuAbajo())
        {
            optionSelected = (optionSelected < options.Length - 1) ? optionSelected + 1 : 0;
        }
    }
}
