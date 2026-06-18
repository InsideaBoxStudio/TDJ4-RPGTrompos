using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CloseOptionsMenu : MonoBehaviour
{
    void Update()
    {
        if ( // si el jugador preciona el boton de opciones o el de retroceder
            Gamepad.all[0].selectButton.wasPressedThisFrame ||
            Gamepad.all[0].buttonEast.wasPressedThisFrame
            )
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
