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
        // Antes hacia "if (Gamepad.current == null) return;": sin joystick no se
        // podia volver atras nunca. Ahora tambien con teclado (Backspace / Esc).
        if (Controles.Volver())
        {
            Invoke("NextScene", time);
        }
    }

    private void NextScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
