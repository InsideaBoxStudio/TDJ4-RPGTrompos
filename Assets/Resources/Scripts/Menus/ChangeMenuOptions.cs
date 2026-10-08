using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ChangeMenuOptions : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private GameObject[] nextOptionMenu;
    [SerializeField] private GameObject[] previousOptionMenu;
    [SerializeField] private GameObject[] thisOptionMenu;

    [SerializeField] private string changeScene = "";

    void Awake()
    {
        if (thisOptionMenu == null || thisOptionMenu.Length == 0)
        {
            thisOptionMenu = new GameObject[]
            {
                transform.parent.transform.parent.gameObject
            };
        }
    }

    void Update()
    {
        //si el jugador preciona esc
        if (Keyboard.current.escapeKey.wasPressedThisFrame &&
            Gamepad.all.Count <= playerIndex)
        {
            PreviousMenu();
        }

        if (Gamepad.all.Count > playerIndex &&
            Gamepad.all[playerIndex].buttonEast.wasPressedThisFrame)
        {
            PreviousMenu();
        }
    }

    private void PreviousMenu()
    {
        // Desactivar menú actual
        if (thisOptionMenu != null)
        {
            foreach (GameObject menu in thisOptionMenu)
            {
                if (menu != null)
                    menu.SetActive(false);
            }
        }

        // Si no hay menú anterior, cambiar de escena
        if (previousOptionMenu == null || previousOptionMenu.Length == 0)
        {
            SceneManager.LoadScene(changeScene);
            return;
        }

        // Activar menú anterior
        foreach (GameObject menu in previousOptionMenu)
        {
            if (menu != null)
                menu.SetActive(true);
        }
    }

    public void ChangeMenu()
    {
        // Desactivar menú actual
        if (thisOptionMenu != null)
        {
            foreach (GameObject menu in thisOptionMenu)
            {
                if (menu != null)
                    menu.SetActive(false);
            }
        }

        // Si no hay menú siguiente, cambiar de escena
        if (nextOptionMenu == null || nextOptionMenu.Length == 0)
        {
            SceneManager.LoadScene(changeScene);
            return;
        }

        // Activar menú siguiente
        foreach (GameObject menu in nextOptionMenu)
        {
            if (menu != null)
                menu.SetActive(true);
        }
    }
}