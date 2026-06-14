using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject[] options;
    private int optionSelected = 0;
    private int playerIndex = -1;
    private bool paused = false;

    void Update()
    {
        if (paused)
        {
            PauseEnabled();
            return;
        }

        // si un jugador presiona el boton de options
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            if (!Gamepad.all[i].selectButton.wasPressedThisFrame) return;
            optionsMenu.SetActive(false);
            playerIndex = i;
            paused = true;
            break;

        }
    }

    private void PauseEnabled()
    {
        optionsMenu.SetActive(true);
        Time.timeScale = 0f;

        // si un jugador presiona el boton de options
        if (Gamepad.all[playerIndex].selectButton.wasPressedThisFrame)
        {
            paused = true;
            return;
        }

        // si un jugador presiona el boton de arriba
        if (Gamepad.all[playerIndex].dpad.up.wasPressedThisFrame)
        {
            if (optionSelected > 0)
            {
                optionSelected--;
            }
            else
            {
                optionSelected = options.Length - 1;
            }
        }
        // si un jugador presiona el boton de abajo
        else if (Gamepad.all[playerIndex].dpad.down.wasPressedThisFrame)
        {
            if (optionSelected < options.Length - 1)
            {
                optionSelected++;
            }
            else
            {
                optionSelected = 0;
            }
        }
    }
}
