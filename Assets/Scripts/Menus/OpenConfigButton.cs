using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class OpenConfigButton : MonoBehaviour
{
    private string sceneName;
    private void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    public void OpenConfig()
    {
        Time.timeScale = 0;
        MenuSceneManager.Instance.OpenConfigMenu(sceneName);
    }

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && gamepad.selectButton.wasPressedThisFrame)
        {
            OpenConfig();
        }
    }
}