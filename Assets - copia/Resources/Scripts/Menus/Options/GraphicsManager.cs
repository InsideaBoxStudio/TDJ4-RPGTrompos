using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    [SerializeField] private int width = 1920;
    [SerializeField] private int height = 1080;

    void Start()
    {
        // Ancho, Alto, Pantalla completa
        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
    }
}
