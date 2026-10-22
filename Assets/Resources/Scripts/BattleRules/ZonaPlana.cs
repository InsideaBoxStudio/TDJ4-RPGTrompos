using UnityEngine;

// ============================================================================
//  ZONA PLANA DEL ESTADIO
//  ----------------------------------------------------------------------------
//  Va en el objeto que atrae a los trompos (el que tiene el tag "GravityPoint"). Sin esto, la
//  gravedad (GravityToPoint) empuja hacia el centro con fuerza constante, como un embudo en V:
//  los trompos terminan siempre peleando en el centro. Con esto el fondo pasa a ser una U: dentro
//  de "radio" la atraccion se multiplica por "gravedadAdentro" (0 = plana), y entre "radio" y
//  "radio + transicion" sube suavemente hasta la gravedad normal, para que el borde no sea un
//  escalon. Se calibra desde el Inspector; el circulo se dibuja en la Scene view.
//
//  GravityToPoint la busca sola en su punto de gravedad: si no esta, todo sigue como antes.
// ============================================================================
public class ZonaPlana : MonoBehaviour
{
    [Tooltip("Radio de la zona plana, en unidades del mundo.")]
    [Min(0f)] public float radio = 1.2f;

    [Tooltip("Ancho del anillo donde la gravedad pasa de 'adentro' a normal. 0 = borde seco.")]
    [Min(0f)] public float transicion = 0.6f;

    [Tooltip("Cuanta gravedad queda dentro de la zona: 0 = ninguna (plana), 1 = la normal.")]
    [Range(0f, 1f)] public float gravedadAdentro = 0f;

    // Multiplicador de la gravedad para un trompo a esta distancia del centro.
    public float Factor(float distancia)
    {
        if (distancia <= radio) return gravedadAdentro;
        if (transicion <= 0f || distancia >= radio + transicion) return 1f;

        float t = (distancia - radio) / transicion;
        return Mathf.Lerp(gravedadAdentro, 1f, Mathf.SmoothStep(0f, 1f, t));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radio);
        if (transicion > 0f)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, radio + transicion);
        }
    }
}
