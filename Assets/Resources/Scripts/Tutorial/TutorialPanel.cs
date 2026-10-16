using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ============================================================================
//  CARTEL DEL TUTORIAL
//  ----------------------------------------------------------------------------
//  Arma por codigo (sin escena ni prefab) un canvas "overlay" con:
//    - un cartel abajo con titulo, explicacion, objetivo y una linea de estado;
//    - un marco que resalta la parte de la pantalla de la que se esta hablando
//      (la barra de energia, la vida, la flecha de apuntar...).
//
//  POR QUE UN CANVAS OVERLAY PROPIO: tiene que verse siempre encima de la pelea y por
//  encima del efecto CRT, y no puede depender de los canvas de la escena (que se prenden
//  y apagan con los turnos). Todo corre en tiempo REAL (unscaledTime): durante el turno
//  el juego esta congelado (timeScale = 0) y el cartel tiene que seguir animandose.
//
//  Usa el Text clasico de Unity UI con la fuente interna: los TextMeshPro del proyecto no
//  tienen los recursos por defecto importados y las fuentes propias podrian no tener
//  tildes ni la enie.
// ============================================================================
public class TutorialPanel : MonoBehaviour
{
    private static readonly Color FondoCartel = new Color(0.05f, 0.06f, 0.10f, 0.92f);
    private static readonly Color ColorTitulo = new Color(1f, 0.85f, 0.25f);
    private static readonly Color ColorObjetivo = new Color(0.45f, 0.95f, 1f);
    private static readonly Color ColorEstado = new Color(0.55f, 1f, 0.55f);
    private static readonly Color ColorPie = new Color(0.7f, 0.7f, 0.75f);
    private static readonly Color ColorMarco = new Color(1f, 0.85f, 0.25f);

    private static readonly Vector3[] esquinas = new Vector3[4];
    private static readonly List<Renderer> renderers = new List<Renderer>(8);

    private RectTransform canvasRect;
    private RectTransform cartel;
    private Text txtContador;
    private Text txtTitulo;
    private Text txtCuerpo;
    private Text txtObjetivo;
    private Text txtEstado;
    private Text txtPie;

    private RectTransform marco;
    private Image[] bordes;
    private Transform resaltado;

    // ---------------- armado ----------------

    private void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        CanvasScaler escala = gameObject.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;

        canvasRect = (RectTransform)transform;

        ArmarMarco();
        ArmarCartel();
        Ocultar();
    }

    private void ArmarMarco()
    {
        marco = NuevoRect("Marco", canvasRect);
        marco.anchorMin = marco.anchorMax = marco.pivot = new Vector2(0.5f, 0.5f);

        bordes = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            RectTransform borde = NuevoRect("Borde" + i, marco);
            bordes[i] = borde.gameObject.AddComponent<Image>();
            bordes[i].color = ColorMarco;
            bordes[i].raycastTarget = false;
        }

        marco.gameObject.SetActive(false);
    }

    private void ArmarCartel()
    {
        cartel = NuevoRect("Cartel", canvasRect);
        cartel.anchorMin = cartel.anchorMax = new Vector2(0.5f, 0f);
        cartel.pivot = new Vector2(0.5f, 0f);
        cartel.anchoredPosition = new Vector2(0f, 36f);
        cartel.sizeDelta = new Vector2(1500f, 0f);

        Image fondo = cartel.gameObject.AddComponent<Image>();
        fondo.color = FondoCartel;
        fondo.raycastTarget = false;

        VerticalLayoutGroup columna = cartel.gameObject.AddComponent<VerticalLayoutGroup>();
        columna.padding = new RectOffset(32, 32, 20, 18);
        columna.spacing = 8f;
        columna.childControlWidth = true;
        columna.childControlHeight = true;
        columna.childForceExpandWidth = true;
        columna.childForceExpandHeight = false;

        ContentSizeFitter ajuste = cartel.gameObject.AddComponent<ContentSizeFitter>();
        ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        txtContador = NuevoTexto("Contador", cartel, 24, ColorPie, FontStyle.Normal);
        txtTitulo = NuevoTexto("Titulo", cartel, 42, ColorTitulo, FontStyle.Bold);
        txtCuerpo = NuevoTexto("Cuerpo", cartel, 31, Color.white, FontStyle.Normal);
        txtObjetivo = NuevoTexto("Objetivo", cartel, 33, ColorObjetivo, FontStyle.Bold);
        txtEstado = NuevoTexto("Estado", cartel, 30, ColorEstado, FontStyle.Bold);
        txtPie = NuevoTexto("Pie", cartel, 24, ColorPie, FontStyle.Italic);
    }

    private static RectTransform NuevoRect(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return (RectTransform)go.transform;
    }

    private static Text NuevoTexto(string nombre, Transform padre, int tamano, Color color, FontStyle estilo)
    {
        RectTransform rect = NuevoRect(nombre, padre);
        Text texto = rect.gameObject.AddComponent<Text>();
        texto.font = Fuente();
        texto.fontSize = tamano;
        texto.fontStyle = estilo;
        texto.color = color;
        texto.alignment = TextAnchor.UpperLeft;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;
        texto.supportRichText = true;
        texto.raycastTarget = false;
        return texto;
    }

    public static Font Fuente()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    // ---------------- API ----------------

    // Los textos vacios ocultan su renglon (el cartel se achica solo).
    public void Mostrar(string contador, string titulo, string cuerpo, string objetivo, string pie)
    {
        Poner(txtContador, contador);
        Poner(txtTitulo, titulo);
        Poner(txtCuerpo, cuerpo);
        Poner(txtObjetivo, objetivo);
        Poner(txtPie, pie);
        Estado("");
        cartel.gameObject.SetActive(true);
    }

    public void Estado(string linea)
    {
        Poner(txtEstado, linea);
    }

    public void Ocultar()
    {
        cartel.gameObject.SetActive(false);
        Resaltar(null);
    }

    public void Resaltar(Transform objetivo)
    {
        resaltado = objetivo;
        if (objetivo == null) marco.gameObject.SetActive(false);
    }

    private static void Poner(Text texto, string valor)
    {
        bool hay = !string.IsNullOrEmpty(valor);
        texto.text = hay ? valor : "";
        texto.gameObject.SetActive(hay);
    }

    // ---------------- marco que sigue al objeto ----------------

    private void LateUpdate()
    {
        if (resaltado == null || !resaltado.gameObject.activeInHierarchy)
        {
            marco.gameObject.SetActive(false);
            return;
        }

        Vector2 min, max;
        if (!RectaEnPantalla(resaltado, out min, out max))
        {
            marco.gameObject.SetActive(false);
            return;
        }

        Vector2 localMin, localMax;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, min, null, out localMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, max, null, out localMax);

        const float margen = 14f;
        Vector2 tamano = (localMax - localMin) + new Vector2(margen * 2f, margen * 2f);
        tamano = new Vector2(Mathf.Max(tamano.x, 40f), Mathf.Max(tamano.y, 40f));

        marco.anchoredPosition = (localMin + localMax) * 0.5f;
        marco.sizeDelta = tamano;
        PonerBordes(tamano);

        // Late: el marco parpadea para llamar la atencion sin tapar lo que resalta.
        float pulso = 0.55f + 0.45f * Mathf.PingPong(Time.unscaledTime * 1.6f, 1f);
        Color c = ColorMarco;
        c.a = pulso;
        for (int i = 0; i < bordes.Length; i++) bordes[i].color = c;

        marco.gameObject.SetActive(true);
    }

    private void PonerBordes(Vector2 tamano)
    {
        const float grosor = 6f;
        // 0 = arriba, 1 = abajo, 2 = izquierda, 3 = derecha
        Colocar(bordes[0].rectTransform, new Vector2(0f, tamano.y * 0.5f), new Vector2(tamano.x, grosor));
        Colocar(bordes[1].rectTransform, new Vector2(0f, -tamano.y * 0.5f), new Vector2(tamano.x, grosor));
        Colocar(bordes[2].rectTransform, new Vector2(-tamano.x * 0.5f, 0f), new Vector2(grosor, tamano.y));
        Colocar(bordes[3].rectTransform, new Vector2(tamano.x * 0.5f, 0f), new Vector2(grosor, tamano.y));
    }

    private static void Colocar(RectTransform rect, Vector2 posicion, Vector2 tamano)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
    }

    // Rectangulo en pixeles de pantalla que ocupa un objeto: sirve tanto para UI (en cualquier
    // tipo de canvas, incluso los que flotan sobre el trompo) como para sprites del mundo.
    private static bool RectaEnPantalla(Transform objeto, out Vector2 min, out Vector2 max)
    {
        min = new Vector2(float.MaxValue, float.MaxValue);
        max = new Vector2(float.MinValue, float.MinValue);

        Camera camaraJuego = Camera.main;
        RectTransform rect = objeto as RectTransform;

        if (rect != null)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            if (canvas != null) canvas = canvas.rootCanvas;

            bool overlay = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay;
            Camera camara = overlay ? null : (canvas != null && canvas.worldCamera != null ? canvas.worldCamera : camaraJuego);
            if (!overlay && camara == null) return false;

            rect.GetWorldCorners(esquinas);
            for (int i = 0; i < 4; i++)
            {
                Vector3 punto = overlay ? esquinas[i] : camara.WorldToScreenPoint(esquinas[i]);
                if (!overlay && punto.z <= 0f) return false; // detras de la camara
                Ampliar(punto, ref min, ref max);
            }
        }
        else
        {
            if (camaraJuego == null) return false;

            objeto.GetComponentsInChildren(false, renderers);
            if (renderers.Count == 0) return false;

            Bounds caja = renderers[0].bounds;
            for (int i = 1; i < renderers.Count; i++) caja.Encapsulate(renderers[i].bounds);

            Vector3 c = caja.center;
            Vector3 e = caja.extents;
            for (int i = 0; i < 8; i++)
            {
                Vector3 esquina = c + new Vector3(
                    (i & 1) == 0 ? -e.x : e.x,
                    (i & 2) == 0 ? -e.y : e.y,
                    (i & 4) == 0 ? -e.z : e.z);
                Vector3 punto = camaraJuego.WorldToScreenPoint(esquina);
                if (punto.z <= 0f) return false;
                Ampliar(punto, ref min, ref max);
            }
        }

        return max.x - min.x > 2f && max.y - min.y > 2f;
    }

    private static void Ampliar(Vector3 punto, ref Vector2 min, ref Vector2 max)
    {
        if (punto.x < min.x) min.x = punto.x;
        if (punto.y < min.y) min.y = punto.y;
        if (punto.x > max.x) max.x = punto.x;
        if (punto.y > max.y) max.y = punto.y;
    }
}
