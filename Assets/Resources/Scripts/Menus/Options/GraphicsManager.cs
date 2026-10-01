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

#if !UNITY_WEBGL
        // Ancho, Alto, Pantalla completa
        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
#endif
        // En la versión web NO se fuerza nada: el tamaño del juego lo manda la
        // página (el canvas se ajusta a la ventana del navegador) y la pantalla
        // completa solo se puede pedir desde un clic del jugador (ver
        // FullscreenToggle). Forzarla al arrancar, sin que nadie haya hecho clic,
        // el navegador la bloquea y puede dejar el canvas con un tamaño raro.
    }
}
