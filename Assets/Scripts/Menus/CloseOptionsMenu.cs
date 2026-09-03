using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CloseOptionsMenu : MonoBehaviour
{
    void Update()
    {
        // Si el jugador presiona el boton de opciones o el de retroceder.
        // Antes esto hacia Gamepad.all[0] sin chequear que hubiera un joystick
        // conectado: con cero controles tiraba excepcion en CADA frame.
        // Ahora pasa por Controles, que ademas suma teclado (Esc / Backspace).
        if (Controles.Pausa() || Controles.Volver())
        {
            CloseOptions();
        }
    }

    public void CloseOptions()
    {
        Time.timeScale = 1;
        MenuSceneManager.Instance.CloseConfigMenu();
    }
}
