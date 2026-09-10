using UnityEngine;

// ============================================================================
//  IDENTIDAD DEL JUGADOR: UN SOLO LUGAR PARA EL playerIndex
//  ----------------------------------------------------------------------------
//  PROBLEMA QUE RESUELVE
//  Hasta ahora cada script del trompo tenía su PROPIO [SerializeField] int
//  playerIndex: 26 campos que había que poner a mano, uno por uno, en cada copia
//  del personaje. Con 2 escenas x 2 jugadores x 2 personajes eso son 8 armados
//  manuales del mismo trompo. Si te olvidabas de uno, ese trompo respondía al
//  joystick equivocado y no te enterabas hasta jugarlo.
//
//  Ese es justamente el motivo por el que NO se podía hacer un prefab del trompo:
//  el índice estaba horneado en cada componente, así que las dos instancias
//  (jugador 1 y jugador 2) no podían salir del mismo prefab.
//
//  CÓMO FUNCIONA
//  Este componente va UNA vez en la raíz del trompo. Los scripts de abajo lo
//  buscan hacia arriba en la jerarquía y toman el índice de acá. Colocás el
//  prefab, ponés el índice en un solo lugar, y todo el trompo queda configurado.
//
//  MIGRACIÓN SIN ROMPER NADA
//  Resolve() devuelve el valor serializado del propio script cuando no encuentra
//  un PlayerIdentity arriba. O sea: las escenas actuales, que todavía no lo
//  tienen, siguen andando exactamente igual. Se puede migrar de a un trompo por
//  vez en vez de tener que convertir todo de una.
// ============================================================================
public class PlayerIdentity : MonoBehaviour
{
    [Tooltip("Jugador 1 = 0, Jugador 2 = 1. Es el único lugar donde se define.")]
    [SerializeField] private int playerIndex = 0;

    public int PlayerIndex
    {
        get => playerIndex;
        set => playerIndex = value;
    }

    // Busca el PlayerIdentity del trompo al que pertenece este componente.
    // Si no hay ninguno (trompo todavía sin migrar), devuelve 'fallback', que es
    // el playerIndex que el script ya traía serializado -> comportamiento idéntico
    // al de antes. Se llama UNA vez, desde Start, y se cachea: no va en Update.
    //
    // Se llama desde Start y no desde Awake a propósito: el AIBrain fija la
    // identidad de la CPU durante SU Awake, y el orden entre Awakes no está
    // garantizado. Todos los Awake corren antes que cualquier Start, así que acá
    // el valor ya es el definitivo.
    public static int Resolve(Component c, int fallback)
    {
        if (c == null) return fallback;

        PlayerIdentity id = c.GetComponentInParent<PlayerIdentity>(true);
        return id != null ? id.playerIndex : fallback;
    }
}
