using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ============================================================================
//  BOTON "TUTORIAL" EN LA ENTRADA A PRACTICA
//  ----------------------------------------------------------------------------
//  Lo crea TutorialArranque al cargar SeleccionDificultad. Se puede usar con el mouse
//  (click), con el teclado (tecla T) o con el joystick (boton superior: triangulo / Y).
//
//  POR QUE NO ENTRA EN LA NAVEGACION DE LOS MENUS: si fuera un boton mas de la pantalla,
//  el stick o las flechas podrian caer en el sin querer. Por eso la navegacion esta en
//  None y se maneja con su propio atajo, que ademas se muestra escrito en el boton.
// ============================================================================
public class TutorialBoton : MonoBehaviour
{
    // Tiempo antes de aceptar el atajo: evita que un boton que se venia apretando en la
    // pantalla anterior active el tutorial sin querer.
    private const float demoraAtajo = 0.5f;

    private float nacio;
    private bool lanzado;

    private void Awake()
    {
        nacio = Time.unscaledTime;
        Armar();
    }

    private void Armar()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 29000;

        CanvasScaler escala = gameObject.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject objetoBoton = new GameObject("Boton", typeof(RectTransform));
        objetoBoton.transform.SetParent(transform, false);

        RectTransform rect = (RectTransform)objetoBoton.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-40f, -40f);
        rect.sizeDelta = new Vector2(430f, 130f);

        Image fondo = objetoBoton.AddComponent<Image>();
        fondo.color = new Color(0.10f, 0.45f, 0.75f, 0.95f);

        Button boton = objetoBoton.AddComponent<Button>();
        boton.targetGraphic = fondo;
        boton.onClick.AddListener(Lanzar);

        Navigation navegacion = boton.navigation;
        navegacion.mode = Navigation.Mode.None;
        boton.navigation = navegacion;

        ColorBlock colores = boton.colors;
        colores.highlightedColor = new Color(0.55f, 0.85f, 1f, 1f);
        colores.pressedColor = new Color(0.30f, 0.60f, 0.90f, 1f);
        boton.colors = colores;

        Text texto = NuevoTexto(rect, "TUTORIAL: ¿Cómo se juega?", 36, FontStyle.Bold, new Vector2(0f, 22f));
        texto.color = Color.white;

        Text atajo = NuevoTexto(rect, "[T]  o  triángulo / Y del joystick", 25, FontStyle.Normal, new Vector2(0f, -30f));
        atajo.color = new Color(0.85f, 0.95f, 1f);
    }

    private static Text NuevoTexto(RectTransform padre, string contenido, int tamano, FontStyle estilo, Vector2 desplazamiento)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform));
        go.transform.SetParent(padre, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10f, 0f);
        rect.offsetMax = new Vector2(-10f, 0f);
        rect.anchoredPosition += desplazamiento;

        Text texto = go.AddComponent<Text>();
        texto.font = TutorialPanel.Fuente();
        texto.fontSize = tamano;
        texto.fontStyle = estilo;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;
        texto.raycastTarget = false;
        texto.text = contenido;
        return texto;
    }

    private void Update()
    {
        if (lanzado || Time.unscaledTime - nacio < demoraAtajo) return;

        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Lanzar();
            return;
        }

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad pad = Gamepad.all[i];
            if (pad != null && pad.buttonNorth.wasPressedThisFrame)
            {
                Lanzar();
                return;
            }
        }
    }

    private void Lanzar()
    {
        if (lanzado) return;
        lanzado = true;
        TutorialModo.Iniciar();
    }
}
