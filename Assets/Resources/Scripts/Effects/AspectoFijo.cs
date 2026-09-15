using UnityEngine;

// ============================================================================
//  MANTIENE UN ASPECTO FIJO CON BANDAS NEGRAS (LETTERBOX / PILLARBOX)
//  ----------------------------------------------------------------------------
//  PROBLEMA QUE RESUELVE
//  En resoluciones que no son 16:9 (por ejemplo 480x854, que es vertical) el
//  juego se ve partido en dos: un recuadro chico y curvo con el juego adentro, y
//  atrás una copia del menú ocupando toda la pantalla.
//
//  POR QUÉ PASA
//  El efecto CRT es post-procesado de URP (LensDistortion, intensidad 0.3). El
//  post-procesado se aplica a lo que renderiza LA CÁMARA, y su deformación
//  depende del aspecto de la pantalla: está calibrada para 16:9. En 9:16 aprieta
//  la imagen hacia el centro y deja un recuadro chico.
//
//  Encima, los Canvas en "Screen Space - Overlay" se dibujan DESPUÉS del
//  post-procesado, así que el CRT no los toca: siguen ocupando la pantalla
//  entera. Resultado: la UI de la cámara queda adentro del recuadro y la UI
//  Overlay afuera. Las dos copias que se ven.
//
//  QUÉ HACE ESTE SCRIPT
//  Recorta el viewport de la cámara para que el área dibujada SIEMPRE tenga el
//  mismo aspecto (16:9 por defecto), agregando bandas negras arriba y abajo o a
//  los costados. Así el CRT recibe siempre el aspecto para el que fue calibrado
//  y se ve igual en cualquier resolución.
//
//  CÓMO SE USA
//  Pegarlo en la cámara principal de cada escena. No hace falta configurar nada:
//  16:9 es el valor por defecto.
//
//  OJO: esto NO alcanza solo para los Canvas en Screen Space - Overlay, porque
//  esos ignoran la cámara. Para que queden dentro de las bandas hay que pasarlos
//  a "Screen Space - Camera" desde el editor. Ver FIX-RESOLUCION.md.
// ============================================================================
[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class AspectoFijo : MonoBehaviour
{
    [Tooltip("Aspecto al que está calibrado el juego y el efecto CRT. 16:9 = 1.7777")]
    [SerializeField] private float aspectoObjetivo = 16f / 9f;

    [Tooltip("Color de las bandas. Negro es lo habitual para un CRT.")]
    [SerializeField] private Color colorBandas = Color.black;

    private Camera cam;
    private int ultimoAncho = -1;
    private int ultimoAlto = -1;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        if (cam == null) cam = GetComponent<Camera>();
        Aplicar();
    }

    private void Update()
    {
        // Solo recalcular cuando cambia el tamaño de la ventana. Comparar dos ints
        // es barato; recalcular el viewport en cada frame no hace falta.
        if (Screen.width == ultimoAncho && Screen.height == ultimoAlto) return;
        Aplicar();
    }

    private void Aplicar()
    {
        if (cam == null) return;
        if (Screen.width <= 0 || Screen.height <= 0) return;
        if (aspectoObjetivo <= 0f) return;

        ultimoAncho = Screen.width;
        ultimoAlto = Screen.height;

        float aspectoPantalla = (float)Screen.width / Screen.height;
        float proporcion = aspectoPantalla / aspectoObjetivo;

        if (proporcion >= 1f)
        {
            // La pantalla es MÁS ANCHA que el objetivo -> bandas a los costados.
            float ancho = 1f / proporcion;
            cam.rect = new Rect((1f - ancho) * 0.5f, 0f, ancho, 1f);
        }
        else
        {
            // La pantalla es MÁS ALTA que el objetivo (el caso de 480x854)
            // -> bandas arriba y abajo.
            cam.rect = new Rect(0f, (1f - proporcion) * 0.5f, 1f, proporcion);
        }

        // Sin esto, lo que queda fuera del viewport muestra basura del frame
        // anterior en vez de negro.
        cam.backgroundColor = colorBandas;
    }
}
