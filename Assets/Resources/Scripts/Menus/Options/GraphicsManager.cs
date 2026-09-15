using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    [SerializeField] private int width = 1920;
    [SerializeField] private int height = 1080;

    [Tooltip("Si está tildado, fuerza la resolución al arrancar el juego. " +
             "Destildalo para respetar la resolución con la que se abrió la ventana.")]
    [SerializeField] private bool forzarAlArrancar = true;

    // Este componente está puesto en LAS 7 ESCENAS. Sin esta bandera, cada cambio
    // de escena volvía a llamar SetResolution y pisaba la resolución actual: si
    // estabas jugando en otra (por ejemplo 480x854), al pasar de escena te la
    // devolvía a 1920x1080. Es static para que valga por toda la sesión.
    private static bool yaSeAplico = false;

    void Start()
    {
        if (!forzarAlArrancar) return;
        if (yaSeAplico) return;

        yaSeAplico = true;

        // Ancho, Alto, Pantalla completa
        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
    }
}
