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
        if (Gamepad.all.Count > 0 && Gamepad.all[0].selectButton.wasPressedThisFrame)
        {
            OpenConfig();
        }
    }
}