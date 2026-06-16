using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  ETIQUETAS DE JUGADOR (P1 / P2 sobre los trompos)
//  ----------------------------------------------------------------------------
//  Muestra una etiqueta "P1" (azul) y "P2" (rojo) que sigue a cada trompo por el
//  estadio, para distinguirlos mientras no haya personajes diferentes.
//
//  Se renderiza en un Canvas de pantalla (no world-space): por eso no gira ni se
//  escala raro. Sigue al trompo proyectando su posición a la pantalla.
//
//  NOTA: el marcador de vida en las esquinas se quitó porque un compañero ya
//  implementó esa parte. Acá quedan solo las etiquetas identificatorias.
//
//  Uso: se pega a un objeto vacío de la escena; arrastrale las dos Vida (J1 y J2)
//  en el Inspector (si las dejás vacías, las busca solo por tag "Player").
// ============================================================================
public class HudVida : MonoBehaviour
{
    [Header("Arrastrá acá la Vida de cada trompo")]
    [SerializeField] private Vida vidaJugador1;
    [SerializeField] private Vida vidaJugador2;

    [Header("Colores de identificación")]
    [SerializeField] private Color color1 = new Color(0.25f, 0.55f, 1f); // azul P1
    [SerializeField] private Color color2 = new Color(1f, 0.30f, 0.30f); // rojo P2

    // Etiquetas flotantes que siguen a cada trompo (renderizadas en el HUD de pantalla).
    private RectTransform canvasRT;
    private Camera cam;
    private Text etiqueta1, etiqueta2;
    private Transform trompo1, trompo2;

    void Start()
    {
        // Si no se asignaron las Vidas en el Inspector, intentar encontrarlas solas
        // (los dos primeros trompos con tag "Player").
        if (vidaJugador1 == null || vidaJugador2 == null) AutoBuscarVidas();

        // El trompo a seguir = el objeto que SE MUEVE. Preferimos el Rigidbody2D
        // (el cuerpo físico que realmente se desplaza/golpea); si no, el sprite;
        // y como último recurso, el objeto de la Vida.
        trompo1 = TrompoQueSeMueve(vidaJugador1);
        trompo2 = TrompoQueSeMueve(vidaJugador2);
        cam = Camera.main;

        // Crear Canvas propio (Screen Space Overlay).
        var canvasGO = new GameObject("HUD_Vida_Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500; // por encima de la UI del juego
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasRT = canvasGO.GetComponent<RectTransform>();

        // NOTA: el marcador de vida en las esquinas (PLAYER 1/2 con los puntos) se quitó
        // porque un compañero ya implementó esa parte. Acá quedan SOLO las etiquetas
        // "P1"/"P2" que identifican a cada trompo sobre el estadio.

        // Etiquetas que siguen a cada trompo (solo el nombre, sin el triángulo).
        etiqueta1 = CrearTexto(canvasGO.transform, "Etiqueta_P1", color1,
            new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.LowerCenter);
        etiqueta1.text = "P1";
        etiqueta2 = CrearTexto(canvasGO.transform, "Etiqueta_P2", color2,
            new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.LowerCenter);
        etiqueta2.text = "P2";
    }

    // El seguimiento va en LateUpdate: se ejecuta DESPUÉS de que la física movió
    // los trompos, así la etiqueta queda pegada sin retraso ("perderse en el aire").
    void LateUpdate()
    {
        SeguirTrompo(etiqueta1, trompo1);
        SeguirTrompo(etiqueta2, trompo2);
    }

    // Devuelve el Transform que realmente se mueve dentro del trompo de esa Vida.
    Transform TrompoQueSeMueve(Vida v)
    {
        if (v == null) return null;
        // El Rigidbody2D es el cuerpo físico que se desplaza y choca.
        var rb = v.GetComponentInParent<Rigidbody2D>();
        if (rb == null) rb = v.GetComponentInChildren<Rigidbody2D>(true);
        if (rb != null) return rb.transform;
        // Si no hay Rigidbody, usar el sprite visible.
        var sr = v.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null) return sr.transform;
        return v.transform;
    }

    // Proyecta la posición del trompo a la pantalla y ubica ahí la etiqueta.
    void SeguirTrompo(Text etiqueta, Transform trompo)
    {
        if (etiqueta == null || trompo == null || cam == null) return;

        // Punto apenas arriba del trompo (más pegado), llevado a coordenadas de pantalla.
        Vector3 mundo = trompo.position + Vector3.up * 0.45f;
        Vector3 pantalla = cam.WorldToScreenPoint(mundo);

        bool visible = pantalla.z > 0f; // que esté delante de la cámara
        etiqueta.enabled = visible;
        if (!visible) return;

        // Convertir el punto de pantalla a posición dentro del Canvas overlay.
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, pantalla, null, out local);
        etiqueta.rectTransform.anchoredPosition = local;
    }

    Text CrearTexto(Transform parent, string nombre, Color color, Vector2 anchor, Vector2 pos, TextAnchor align)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600, 90);

        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 46;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        // contorno negro para que se lea sobre cualquier fondo
        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        return t;
    }

    void AutoBuscarVidas()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            var v = p.GetComponentInChildren<Vida>(true);
            if (v == null) continue;
            if (vidaJugador1 == null) vidaJugador1 = v;
            else if (vidaJugador2 == null && v != vidaJugador1) { vidaJugador2 = v; break; }
        }
    }
}
