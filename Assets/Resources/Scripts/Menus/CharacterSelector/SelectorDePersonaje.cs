using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// ============================================================================
//  SELECTOR DE PERSONAJE POR JUGADOR (los dos eligen al mismo tiempo)
//  ----------------------------------------------------------------------------
//  PROBLEMA QUE RESUELVE
//  Antes la pantalla era por turnos: primero elegía el Jugador 1 y recién
//  después el 2. El motivo: los dos menús compartían UN solo EventSystem, y un
//  EventSystem tiene un único objeto seleccionado a la vez. O sea, un solo
//  cursor para dos personas. InitButtonSelected, además, lo re-seleccionaba en
//  cada frame, así que no había forma de que existieran dos.
//
//  CÓMO FUNCIONA
//  Cada jugador tiene su propio selector, que lee SU joystick y SUS teclas a
//  través de Controles. Los dos se mueven y confirman en paralelo, sin
//  depender uno del otro.
//
//  No reemplaza nada del flujo que ya existía: al confirmar, invoca el onClick
//  del botón, que ya llama a SelectCharacter.Player1Select / Player2Select.
//  CharacterData, MarkCharacterSelected y CharacterSelecteds siguen igual.
//
//  CONTROLES
//              Mover           Confirmar / cambiar de opinión
//   Jugador 1  A D  (o W S)    Q
//   Jugador 2  J L  (o I K)    U
//   Joystick   D-pad           Botón de abajo (A en Xbox)
//
//  Confirmar dos veces DESBLOQUEA la elección, por si alguien quiere cambiar
//  mientras el otro todavía está eligiendo. Cuando los dos confirmaron,
//  CharacterSelecteds carga la pelea.
//
//  CÓMO SE USA
//  Pegarlo en MenuP1 y en MenuP2. No hay nada que configurar: busca solo los
//  botones de personaje de su menú y detecta a qué jugador pertenece mirando a
//  qué método llaman esos botones (Player1Select o Player2Select).
// ============================================================================
public class SelectorDePersonaje : MonoBehaviour
{
    [Tooltip("0 = Jugador 1, 1 = Jugador 2. Dejalo en -1 para que lo detecte solo.")]
    [SerializeField] private int playerIndex = -1;

    [Tooltip("Botones de personaje. Si lo dejás vacío, los busca solo entre los hijos.")]
    [SerializeField] private Button[] opciones;

    [Tooltip("Cuánto se agranda la opción que tiene el cursor encima.")]
    [SerializeField] private float escalaResaltado = 1.15f;

    [Tooltip("En qué personaje arranca el cursor (0 = el primero).")]
    [SerializeField] private int opcionInicial = 0;

    private const string METODO_P1 = "Player1Select";
    private const string METODO_P2 = "Player2Select";

    private int actual;
    private bool confirmado;
    private Vector3[] escalasOriginales;

    private void Start()
    {
        if (playerIndex < 0) playerIndex = DetectarJugador();
        if (opciones == null || opciones.Length == 0) opciones = BuscarOpciones();

        if (opciones.Length == 0)
        {
            Debug.LogWarning($"SelectorDePersonaje ({name}): no encontré botones de personaje.");
            enabled = false;
            return;
        }

        escalasOriginales = new Vector3[opciones.Length];
        for (int i = 0; i < opciones.Length; i++)
        {
            escalasOriginales[i] = opciones[i].transform.localScale;

            // El cursor lo manejamos nosotros: que la navegación de Unity no mueva
            // la selección por su cuenta.
            Navigation nav = opciones[i].navigation;
            nav.mode = Navigation.Mode.None;
            opciones[i].navigation = nav;
        }

        // El EventSystem ya no maneja estos botones. Si siguiera mandando Submit,
        // el botón de abajo del joystick confirmaría DOS veces (una por acá y otra
        // por el EventSystem) y el Enter elegiría por el Jugador 1. El mouse sigue
        // funcionando: los clicks no dependen de esto.
        EventSystem es = EventSystem.current;
        if (es != null)
        {
            es.sendNavigationEvents = false;
            es.SetSelectedGameObject(null);
        }

        actual = Mathf.Clamp(opcionInicial, 0, opciones.Length - 1);
        Resaltar();
    }

    private void Update()
    {
        if (Controles.Confirmar(playerIndex))
        {
            if (confirmado) Desconfirmar();
            else Confirmar();
            return;
        }

        // Con la elección bloqueada el cursor queda quieto.
        if (confirmado) return;

        int delta = 0;
        if (Controles.MenuIzquierda(playerIndex) || Controles.MenuArriba(playerIndex)) delta = -1;
        else if (Controles.MenuDerecha(playerIndex) || Controles.MenuAbajo(playerIndex)) delta = 1;

        if (delta == 0) return;

        // Vuelta circular: desde el último se pasa al primero y viceversa.
        actual = (actual + delta + opciones.Length) % opciones.Length;
        Resaltar();
    }

    private void Confirmar()
    {
        confirmado = true;

        // Mismo camino que un click: el onClick ya llama a Player1Select/Player2Select
        // con el nombre del personaje, que es lo que se guarda en CharacterData.
        opciones[actual].onClick.Invoke();
    }

    private void Desconfirmar()
    {
        confirmado = false;

        // " " es el valor de "sin elegir" que usa todo el flujo (ver
        // SelectCharacter.Awake y CharacterSelecteds).
        if (playerIndex == 0) CharacterData.characterIndex1 = " ";
        else                  CharacterData.characterIndex2 = " ";
    }

    private void Resaltar()
    {
        for (int i = 0; i < opciones.Length; i++)
        {
            float factor = (i == actual) ? escalaResaltado : 1f;
            opciones[i].transform.localScale = escalasOriginales[i] * factor;
        }
    }

    // A qué jugador pertenece este menú, según a qué método llaman sus botones.
    private int DetectarJugador()
    {
        foreach (Button b in GetComponentsInChildren<Button>(true))
        {
            string metodo = MetodoDe(b);
            if (metodo == METODO_P2) return 1;
            if (metodo == METODO_P1) return 0;
        }
        return 0;
    }

    // Solo los botones que eligen personaje PARA ESTE jugador. Así, si en el menú
    // hay otros botones (volver, opciones), no se cuelan en el cursor.
    private Button[] BuscarOpciones()
    {
        string esperado = playerIndex == 1 ? METODO_P2 : METODO_P1;

        Button[] todos = GetComponentsInChildren<Button>(true);
        int cuantos = 0;
        foreach (Button b in todos) if (MetodoDe(b) == esperado) cuantos++;

        Button[] resultado = new Button[cuantos];
        int k = 0;
        foreach (Button b in todos) if (MetodoDe(b) == esperado) resultado[k++] = b;
        return resultado;
    }

    private static string MetodoDe(Button b)
    {
        if (b == null || b.onClick.GetPersistentEventCount() == 0) return null;
        return b.onClick.GetPersistentMethodName(0);
    }
}
