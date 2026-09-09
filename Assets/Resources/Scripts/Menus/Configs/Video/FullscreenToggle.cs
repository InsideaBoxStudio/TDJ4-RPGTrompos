using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggle : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        // Estado inicial del Toggle
        fullscreenToggle.isOn = Screen.fullScreen;

        // Escuchar cambios
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    private void OnDestroy()
    {
        fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
    }
}