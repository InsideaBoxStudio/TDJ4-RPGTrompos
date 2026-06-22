using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ReturnScene : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private float time;

    void Update()
    {
        if (Gamepad.current == null) return;
        if (Gamepad.all[0].buttonEast.wasPressedThisFrame)
        {
            Invoke("NextScene", time);
        }
    }

    private void NextScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
