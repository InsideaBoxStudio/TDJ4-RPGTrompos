#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

// ============================================================================
//  SELECCIÓN DIFICULTAD BUILDER (Editor)
//  ----------------------------------------------------------------------------
//  Construye con un clic la pantalla de selección de dificultad:
//  Menú:  Trompos IA > Construir Pantalla Dificultad
//
//  Genera una escena "SeleccionDificultad" con título + 3 botones grandes
//  (Fácil / Normal / Difícil) ya cableados al script SeleccionDificultad,
//  que guarda la elección y carga la escena Practica.
//
//  IMPORTANTE: después de construir, agregá la escena a File > Build Settings
//  (o el menú al que apunta el botón "Práctica") para que cargue bien.
// ============================================================================
public static class SeleccionDificultadBuilder
{
    [MenuItem("Trompos IA/Construir Pantalla Dificultad")]
    public static void Construir()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- Cámara ----------
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = HexC("#1B2631");
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();

        // ---------- EventSystem ----------
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        // Este proyecto usa el Input System NUEVO, así que el módulo de input de la UI
        // debe ser InputSystemUIInputModule (no el StandaloneInputModule viejo, que
        // tira "You are trying to read Input using the UnityEngine.Input class...").
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ---------- Canvas ----------
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ---------- Controlador (script de selección) ----------
        var ctrlGO = new GameObject("SeleccionDificultad");
        var ctrl = ctrlGO.AddComponent<SeleccionDificultad>();

        // ---------- Título ----------
        var titulo = Texto(canvasGO.transform, "Titulo", "ELEGÍ LA DIFICULTAD", 70, TextAnchor.MiddleCenter);
        Anchor(titulo.gameObject, new Vector2(0.5f, 1f), new Vector2(0, -160), new Vector2(1400, 120));
        titulo.color = Color.white;

        // ---------- 3 botones ----------
        CrearBoton(canvasGO.transform, "BotonFacil", "FÁCIL", new Vector2(0, 120), HexC("#27AE60"), ctrl, "ElegirFacil");
        CrearBoton(canvasGO.transform, "BotonNormal", "NORMAL", new Vector2(0, -40), HexC("#E67E22"), ctrl, "ElegirNormal");
        CrearBoton(canvasGO.transform, "BotonDificil", "DIFÍCIL", new Vector2(0, -200), HexC("#C0392B"), ctrl, "ElegirDificil");

        // ---------- Guardar ----------
        if (!System.IO.Directory.Exists("Assets/Scenes"))
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SeleccionDificultad.unity");
        EditorUtility.DisplayDialog("Trompos IA",
            "Pantalla de dificultad construida.\n\n" +
            "1) Agregala a File > Build Settings.\n" +
            "2) El botón 'Práctica' del menú debe cargar la escena 'SeleccionDificultad'.\n" +
            "3) Verificá que el AIBrain del trompo IA tenga 'Usar Dificultad Elegida' tildado.",
            "Listo");
        Debug.Log("[Trompos IA] Escena SeleccionDificultad creada en Assets/Scenes/.");
    }

    // ============ helpers ============

    static void CrearBoton(Transform parent, string nombre, string label, Vector2 pos, Color color,
                           SeleccionDificultad ctrl, string metodo)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(560, 130);
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(Mathf.Min(1, color.r*1.2f), Mathf.Min(1, color.g*1.2f), Mathf.Min(1, color.b*1.2f), 1f);
        colors.pressedColor = new Color(color.r*0.8f, color.g*0.8f, color.b*0.8f, 1f);
        btn.colors = colors;

        // Cablear el onClick al método correspondiente del controlador (persistente, queda guardado en la escena).
        var action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), ctrl, metodo)
                     as UnityEngine.Events.UnityAction;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);

        var txtGO = new GameObject("Texto");
        txtGO.transform.SetParent(go.transform, false);
        var txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Font();
        txt.fontSize = 48;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        Stretch(txtGO.GetComponent<RectTransform>());
    }

    static Text Texto(Transform parent, string nombre, string contenido, int tam, TextAnchor anchor)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<Text>();
        t.text = contenido;
        t.font = Font();
        t.fontSize = tam;
        t.fontStyle = FontStyle.Bold;
        t.alignment = anchor;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static void Anchor(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static Font Font()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    static Color HexC(string hex) { Color c; ColorUtility.TryParseHtmlString(hex, out c); return c; }
}
#endif
