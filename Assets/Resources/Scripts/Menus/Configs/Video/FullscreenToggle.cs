using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Casillero de "Pantalla completa".
//
// EN LA VERSIÓN WEB: el navegador solo concede la pantalla completa si el pedido
// viene de un gesto real del jugador (clic, toque o tecla). El botón de un
// joystick NO cuenta. Antes, tildar el casillero con el joystick pedía la pantalla
// completa sin gesto y el navegador lo ignoraba en silencio. Ahora, en la web, si
// el casillero se cambió sin gesto, el pedido espera al próximo clic o tecla.
// En Windows no cambia nada: se aplica al instante.
public class FullscreenToggle : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle;

    // Pedido que todavía no se pudo aplicar por falta de gesto (solo web).
    private bool hayPedidoPendiente;
    private bool pedidoPendiente;

    private void Start()
    {
        // Estado inicial del Toggle
        fullscreenToggle.isOn = Screen.fullScreen;

        // Escuchar cambios
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void Update()
    {
        if (hayPedidoPendiente)
        {
            if (HayGestoEsteFrame()) AplicarPendiente();
            return;
        }

        // Mantener el casillero fiel a la realidad: si el jugador salió con Esc
        // (o con Alt+Enter) la pantalla ya no es completa y el casillero no se enteraba.
        if (fullscreenToggle.isOn != Screen.fullScreen)
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    private void SetFullscreen(bool fullscreen)
    {
        // Application.platform vale WebGLPlayer solo en el juego publicado en web
        // (en el editor vale el de la PC), así que Windows y el editor no cambian.
        bool esWeb = Application.platform == RuntimePlatform.WebGLPlayer;
        if (esWeb && !HayGestoEsteFrame())
        {
            hayPedidoPendiente = true;
            pedidoPendiente = fullscreen;
            return;
        }

        Screen.fullScreen = fullscreen;
        Debug.Log("Fullscreen cambiado a: " + fullscreen);
    }

    private void AplicarPendiente()
    {
        hayPedidoPendiente = false;
        Screen.fullScreen = pedidoPendiente;
        Debug.Log("Fullscreen cambiado a: " + pedidoPendiente + " (esperó un gesto del jugador)");
    }

    // ¿Hubo un clic, un toque o una tecla en este frame? Se miran tanto el apretar
    // como el soltar: un casillero con el mouse se activa al SOLTAR el botón.
    private static bool HayGestoEsteFrame()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.leftButton.wasReleasedThisFrame))
            return true;

        Keyboard teclado = Keyboard.current;
        if (teclado != null && teclado.anyKey.wasPressedThisFrame)
            return true;

        Touchscreen toque = Touchscreen.current;
        if (toque != null && (toque.primaryTouch.press.wasPressedThisFrame || toque.primaryTouch.press.wasReleasedThisFrame))
            return true;

        return false;
    }

    private void OnDestroy()
    {
        fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
    }
}
