using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ============================================================================
//  DIRECTOR DEL TUTORIAL
//  ----------------------------------------------------------------------------
//  Vive DENTRO de la escena Tutorial (una copia de Practica, pegado al objeto "Configs") y
//  la convierte en una clase COREOGRAFIADA: el juego se pausa cada vez que el jugador tiene
//  que hacer algo, se le ensena UNA mecanica a la vez, y recien cuando la hace de verdad
//  se sigue. Explica el PORQUE de cada cosa, no solo la tecla.
//
//  La escena es independiente: no hay ningun boton ni enganche en otras escenas y no
//  depende de que se llegue desde un menu. Abrirla y darle Play alcanza.
//
//  Los dos momentos de pausa:
//   - LANZAMIENTO: el cordon entra solo en el lanzador y todo queda quieto hasta que el
//     jugador aprieta el boton.
//   - CADA TURNO: el juego ya se congela solo cuando llega el turno; ahi se muestra el
//     cartel de la mecanica del paso y se espera la accion.
//
//  NO MODIFICA NINGUN SCRIPT DEL JUEGO NI LA ESCENA DE PRACTICA. Todo lo hace desde
//  afuera, leyendo datos que el juego ya expone (energia gastada, vida, turno activo...)
//  y moviendo solo los campos publicos pensados para eso. Lo que cambia, y por que:
//
//   - Deja solo Ninja contra Ninja. El Caballero del jugador viene prendido de fabrica
//     (la escena es copia de Practica) y ActiveCharacter solo prende, nunca apaga.
//   - La CPU se queda quieta ("maniqui de practica"): se apaga su AIBrain y, cuando le
//     toca su turno, solo espera. Asi el jugador puede probar todo sin apuros.
//   - El rival no baja de cierta vida hasta el ultimo paso, y despues le queda poca
//     para que el jugador GANE. El jugador tampoco puede morir.
//   - Se regala la energia que pide cada paso: sin eso el jugador no llegaria a los
//     ataques caros (la energia sube de a 1 por turno).
//   - El reloj de 10 segundos del turno y la cuenta regresiva del lanzamiento se
//     congelan mientras se lee: nadie pierde un turno por estar leyendo.
//
//  Todo se mide en tiempo REAL (unscaledDeltaTime): durante el turno el juego esta
//  congelado (Time.timeScale = 0).
// ============================================================================
public class TutorialDirector : MonoBehaviour
{
    private const string Personaje = "Ninja";     // el que tiene mas ataques distintos para probar

    private const float PisoVidaRival = 0.4f;     // fraccion de su vida maxima por debajo de la cual no baja
    private const float PisoVidaJugador = 0.5f;
    private const float VidaRivalFinal = 15f;     // vida con la que se deja al rival para el ultimo paso
    private const float PausaTrasCumplir = 1.5f;  // se muestra "¡Bien!" antes de pasar al siguiente
    private const float ReaccionCpu = 0.35f;      // la CPU espera un instante antes de pasar su turno
    private const float CierreForzadoCpu = 1.2f;  // si la espera no cerro el turno, se cierra a la fuerza
    private const int IntentosMaximos = 3;        // golpes fallidos antes de dar el paso por bueno
    private const float DuracionCordon = 2f;      // segundos que tarda el cordon en entrar al lanzador
    private const float PosicionCordonAdentro = 1f; // valor de la barra con el cordon completamente adentro (y vida maxima)
    private const float DistanciaPractica = 5f;   // distancia entre los trompos cuando se acomodan para apuntar
    private const float VidaRival = 100f;        // el juego deja a la CPU con muy poca vida en esta escena (ver PrepararCpu)

    // Un paso del tutorial. Es solo datos: la logica comun esta en Actualizar().
    private class Paso
    {
        public string titulo;
        public string texto;
        public string objetivo;          // lo que hay que hacer (vacio = solo leer)
        public bool soloLeer;            // avanza con Continuar
        public bool requiereTurno;       // el cartel espera a que sea el turno del jugador
        public string ataque;            // clave con la que el juego registra el uso (ver UsoRegistrado)
        public bool requiereGolpe;       // ademas de usarlo, tiene que impactar al rival
        public Func<bool> condicion;     // condicion propia (si no hay "ataque")
        public Func<string> estadoExtra; // linea de estado propia del paso (vacia = la comun)
        public Func<Transform> resaltar;
        public bool separar;             // al llegar el turno, se acomodan los trompos para que se vea la flecha
        public int energiaMinima;        // energia que se asegura mientras el paso esta activo
        public Action alEntrar;
        public Action alSalir;
        public string pistaFallo;
    }

    private TutorialPanel panel;
    private readonly List<Paso> pasos = new List<Paso>();
    private int indice = -1;
    private Paso actual;
    private bool listo;
    private bool termino;

    // Para detectar que se conecto o desconecto un joystick y rehacer los textos.
    private string firmaControles;
    private float proximoChequeoControles;

    // Actores (se buscan una vez que la escena termino de arrancar).
    private CheckPlayerTurn turnoJ, turnoCpu;
    private Vida vidaJ, vidaCpu;
    private EnergyCounter energiaJ;
    private AIBrain cerebroCpu;
    private WaitOption esperaCpu;
    private CountDown cuentaInicial;
    private CountDownRPG relojTurno;
    private EndGame fin;
    private TimingBar barraJ;
    private Transform contenedorJ;

    // Estado de los sensores.
    private readonly Dictionary<string, int> conteoPrevio = new Dictionary<string, int>();
    private int energiaPrevia;
    private float vidaCpuPrevia;

    // Estado del paso actual.
    private int usosPaso;
    private bool golpeTrasUso;
    private bool golpeEnPaso;
    private bool cumplido;
    private float tiempoCumplido;
    private bool avisoEnergia;
    private bool separadoEnPaso;
    private string estadoMostrado;

    // Retencion de la cuenta regresiva del lanzamiento.
    private bool retenerCuenta;
    private int cuentaOriginal = 3;
    private TextMeshProUGUI textoCuenta;
    private AudioSource sonidoCuenta;

    // Comportamiento de la CPU y proteccion de vidas.
    private bool rivalProtegido = true;
    private float tiempoTurnoCpu;
    private float tiempoCierreCpu;

    // Coreografia del lanzamiento: el cordon (la barra del lanzador) entra solo y se queda
    // quieto, con todo pausado, hasta que el jugador aprieta el boton.
    private Scrollbar barraCordon;
    private bool animandoCordon;
    private bool cordonListo;
    private float tiempoCordon;

    // Vida maxima de Vida (campo privado). No hay forma publica de devolverle la vida a la CPU:
    // el juego la baja al 10% al "frenar" su barra de lanzamiento.
    private static readonly FieldInfo campoVidaMaxima =
        typeof(Vida).GetField("maxLife", BindingFlags.NonPublic | BindingFlags.Instance);

    // Angulo de la flecha de apuntar (campo privado de Prompter).
    private static readonly FieldInfo campoAnguloFlecha =
        typeof(Prompter).GetField("angle", BindingFlags.NonPublic | BindingFlags.Instance);

    // ------------------------------------------------------------------ arranque

    private void Awake()
    {
        // Antes de que corra cualquier Start: asi EndGame y el resto arman sus listas ya
        // sin los personajes de mas.
        DejarSoloElPersonaje("Jugador1");
        DejarSoloElPersonaje("CPU");

        PrepararBarraDelJugador();

        panel = new GameObject("TutorialPanel").AddComponent<TutorialPanel>();
        panel.transform.SetParent(transform, false);
    }

    // La barra del lanzador del jugador se apaga ANTES de su primer Update: en el tutorial no
    // oscila sola sino que la mueve el director (ver Coreografia del lanzamiento). Si no se
    // apagara, frenaria sola a la mitad cuando termine la cuenta regresiva.
    private void PrepararBarraDelJugador()
    {
        foreach (TimingBar barra in FindObjectsByType<TimingBar>(FindObjectsSortMode.None))
        {
            if (barra.transform.root.name != "Jugador1") continue;

            barraJ = barra;
            barraCordon = barra.GetComponent<Scrollbar>();
            barra.enabled = false;
            return;
        }
    }

    // Se busca entre las raices de la escena (no con GameObject.Find) para no confundir el
    // "CPU" o el "Jugador1" de verdad con otro objeto que se llame igual mas adentro.
    private static void DejarSoloElPersonaje(string nombreRaiz)
    {
        foreach (GameObject raiz in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (raiz.name != nombreRaiz) continue;

            Transform elegido = raiz.transform.Find(Personaje);
            foreach (Transform hijo in raiz.transform)
            {
                hijo.gameObject.SetActive(hijo == elegido);
            }
            return;
        }
    }

    private IEnumerator Start()
    {
        // Dos cuadros: los Start de la escena (por ejemplo el del AIBrain, que se carga la
        // vida llena) ya corrieron y recien ahora es seguro apagarlo.
        yield return null;
        yield return null;

        if (!BuscarActores())
        {
            Debug.LogWarning("Tutorial: no se encontraron los trompos del jugador y la CPU. Se cancela.");
            Destroy(gameObject);
            yield break;
        }

        PrepararCpu();
        PrepararCuentaInicial();
        pasos.AddRange(CrearPasos());
        firmaControles = TutorialControles.Firma;
        Debug.Log("Tutorial: control detectado: " + TutorialControles.Detectado);

        energiaPrevia = energiaJ.currentEnergy;
        vidaCpuPrevia = vidaCpu.vidaActual;

        // La cuenta regresiva se retiene desde el principio, no recien en el paso del
        // lanzamiento: si no, los carteles de bienvenida se leerian mientras corre y la
        // pelea arrancaria con la barra sin frenar (el juego la frena solo a la mitad).
        RetenerCuenta(true);

        listo = true;
        Entrar(0);
    }

    private bool BuscarActores()
    {
        foreach (CheckPlayerTurn turno in FindObjectsByType<CheckPlayerTurn>(FindObjectsSortMode.None))
        {
            string raiz = turno.transform.root.name;
            if (raiz == "Jugador1") turnoJ = turno;
            else if (raiz == "CPU") turnoCpu = turno;
        }

        if (turnoJ == null || turnoCpu == null) return false;

        vidaJ = turnoJ.GetComponent<Vida>();
        vidaCpu = turnoCpu.GetComponent<Vida>();
        contenedorJ = turnoJ.transform.parent;
        Transform contenedorCpu = turnoCpu.transform.parent;
        if (vidaJ == null || vidaCpu == null || contenedorJ == null || contenedorCpu == null) return false;

        energiaJ = contenedorJ.GetComponentInChildren<EnergyCounter>(true);
        if (barraJ == null) barraJ = contenedorJ.GetComponentInChildren<TimingBar>(true);
        if (energiaJ == null) return false;

        cerebroCpu = contenedorCpu.GetComponent<AIBrain>();
        if (cerebroCpu == null) cerebroCpu = contenedorCpu.GetComponentInChildren<AIBrain>(true);
        esperaCpu = contenedorCpu.GetComponentInChildren<WaitOption>(true);

        cuentaInicial = FindFirstObjectByType<CountDown>();
        relojTurno = FindFirstObjectByType<CountDownRPG>();
        fin = FindFirstObjectByType<EndGame>(FindObjectsInactive.Include); // vive en un objeto que se prende al "GO!"

        return true;
    }

    // La CPU deja de pensar: queda como un maniqui que solo pasa su turno.
    private void PrepararCpu()
    {
        if (cerebroCpu != null) cerebroCpu.enabled = false;

        // En esta escena la barra de lanzamiento de la CPU la frena el AIBrain "casi centrada" y
        // Vida.WaitForInfo la interpreta como un 10% de la vida: la CPU quedaria con 10 de vida y
        // un solo shuriken la mataria antes de terminar la clase. Se le devuelve la vida entera.
        if (campoVidaMaxima != null) campoVidaMaxima.SetValue(vidaCpu, VidaRival);
        vidaCpu.SetFullLife();
    }

    private void PrepararCuentaInicial()
    {
        if (cuentaInicial == null) return;

        cuentaOriginal = cuentaInicial.countDownTime;
        textoCuenta = cuentaInicial.GetComponent<TextMeshProUGUI>();
        sonidoCuenta = cuentaInicial.GetComponent<AudioSource>();
    }

    // ------------------------------------------------------------------ pasos

    private Transform Energia() { return energiaJ != null ? energiaJ.transform : null; }

    private Transform VidaVisual(Transform contenedor)
    {
        if (contenedor == null) return null;
        CircularBar barra = contenedor.GetComponentInChildren<CircularBar>(true);
        return barra != null ? barra.transform : null;
    }

    private Transform Flecha()
    {
        Prompter flecha = contenedorJ != null ? contenedorJ.GetComponentInChildren<Prompter>(true) : null;
        return flecha != null ? flecha.transform : null;
    }

    private int Costo<T>(int respaldo) where T : MonoBehaviour
    {
        T accion = contenedorJ != null ? contenedorJ.GetComponentInChildren<T>(true) : null;
        IAIAction contrato = accion as IAIAction;
        return contrato != null ? contrato.EnergyCost : respaldo;
    }

    // Arma la lista de pasos con los nombres de botones del control que HAY AHORA. Se vuelve a
    // llamar si cambia el control (ver ActualizarControles) y se copian solo los textos.
    private List<Paso> CrearPasos()
    {
        List<Paso> lista = new List<Paso>();

        int costoShuriken = Costo<LaunchShuriken>(2);
        int costoPinchos = Costo<LaunchPinchos>(2);
        int costoClon = Costo<LaunchClone>(2);
        int costoSierra = Costo<Cierra>(4);
        int costoSustitucion = Costo<InvokeSustitution>(2);
        const int costoVeneno = 4; // ActivePoison no expone su costo
        const int costoBasico = 1;
        const int costoMover = 1;

        // ---- Antes de pelear ----
        lista.Add(new Paso
        {
            titulo = "¡Bienvenido!",
            texto = "Acá aprendés a jugar.\n" +
                    "Tu objetivo: dejar la <b>VIDA</b> del rival en 0.",
            soloLeer = true
        });

        lista.Add(new Paso
        {
            titulo = "Paso 1: el lanzamiento",
            texto = "Antes de pelear lanzás tu trompo. El cordón entra en el lanzador y vos elegís el momento de soltar: " +
                    "<b>cuanto más adentro esté el cordón, más vida inicial tiene tu trompo</b>. " +
                    "En una partida normal la barra se mueve sola y tenés 3 segundos para frenarla; acá el juego espera a que estés listo.",
            objetivo = "Cuando el cordón esté adentro, apretá  <b>" + TutorialControles.Esperar + "</b>",
            condicion = () => cordonListo && Controles.Esperar(0),
            alEntrar = IniciarCordon,
            alSalir = SoltarLanzamiento, // recien ahora corre el 3, 2, 1, GO!
            estadoExtra = () => cordonListo
                ? "El cordón está completamente adentro del lanzador: ¡ahora!"
                : "Mirá cómo el cordón entra en el lanzador…",
            resaltar = () => barraJ != null ? barraJ.transform : null
        });

        lista.Add(new Paso
        {
            titulo = "¡Buen lanzamiento!",
            texto = "Tu vida inicial ya quedó fijada con ese lanzamiento. Ahora viene la cuenta regresiva: " +
                    "cuando diga <b>GO!</b> empieza la pelea. Los trompos se mueven solos y se van frenando.",
            soloLeer = true,
            condicion = () => cuentaInicial == null || cuentaInicial.finished // se cierra solo al empezar la pelea
        });

        // ---- Primer turno ----
        lista.Add(new Paso
        {
            titulo = "Tu turno",
            texto = "Cuando tu trompo baja la velocidad llega <b>TU TURNO</b>: el juego se congela y elegís UNA acción " +
                    "(atacar, moverte, esperar o un especial). Al empezar cada turno ganás <b>+1 de energía</b>.\n" +
                    "En una partida real tenés 10 segundos para decidir; <b>en el tutorial el reloj está detenido</b>.",
            soloLeer = true,
            requiereTurno = true
        });

        lista.Add(new Paso
        {
            titulo = "La energía",
            texto = "Estas barritas son tu <b>ENERGÍA</b>. Cada acción gasta un poco y, si no te alcanza, no se puede usar. " +
                    "Ganás +1 por turno y <b>+2 cada vez que golpeás al rival</b>: atacar te devuelve energía.\n" +
                    "Para practicar, el tutorial te regala la que necesites en cada paso.",
            soloLeer = true,
            requiereTurno = true,
            resaltar = Energia
        });

        lista.Add(new Paso
        {
            titulo = "La vida",
            texto = "El círculo de cada trompo es su <b>VIDA</b>: cuando llega a 0, ese trompo pierde. " +
                    "Cada golpe le saca vida al rival y además <b>lo hace esperar más para su próximo turno</b>, " +
                    "así que pegar también te da ventaja de tiempo.",
            soloLeer = true,
            requiereTurno = true,
            resaltar = () => VidaVisual(turnoCpu != null ? turnoCpu.transform.parent : null)
        });

        lista.Add(new Paso
        {
            titulo = "Apuntar",
            texto = TutorialControles.ConJoystick
                ? "Con <b>el STICK DERECHO</b> apuntás: la flecha de tu trompo marca hacia dónde irá tu acción. " +
                  "Si no lo tocás, apunta sola hacia el rival."
                : "La flecha de tu trompo marca hacia dónde irá tu acción y apunta sola hacia el rival. " +
                  "Con teclado podés girarla con <b>W A S D</b>, pero ojo: esas teclas también lanzan ataques " +
                  "(W básico, A pinchos, S sierra, D shuriken), así que conviene dejar que apunte sola.",
            soloLeer = true,
            requiereTurno = true,
            separar = true,
            resaltar = Flecha
        });

        lista.Add(new Paso
        {
            titulo = "El menú de ataques",
            texto = "Mientras mantenés apretado el botón se despliega el menú de <b>ATAQUES</b>: ahí ves qué ataque tiene " +
                    "cada dirección y cuánta energía cuesta. El botón <b>" + TutorialControles.Menu2 + "</b> despliega los <b>ESPECIALES</b>.",
            objetivo = "Mantené apretado  <b>" + TutorialControles.Menu1 + "</b>",
            requiereTurno = true,
            condicion = () => Controles.Menu1(0) || Controles.Menu2(0)
        });

        // ---- Acciones, de la mas simple a la mas elaborada ----
        lista.Add(new Paso
        {
            titulo = "El ataque básico  (" + costoBasico + " de energía)",
            texto = "Es tu ataque más barato: tu trompo <b>embiste hacia donde apunta la flecha</b>. " +
                    "Si impacta, el rival pierde vida y vos recuperás +2 de energía, por eso conviene acertar.",
            objetivo = "Atacá con  <b>" + TutorialControles.Atacar + "</b>  y lográ un impacto",
            requiereTurno = true,
            ataque = "BasicAttack",
            separar = true,
            requiereGolpe = true,
            energiaMinima = costoBasico,
            pistaFallo = "No impactó. Esperá tu próximo turno, mirá que la flecha apunte al rival y volvé a intentar."
        });

        lista.Add(new Paso
        {
            titulo = "Moverse  (" + costoMover + " de energía)",
            texto = "Tu trompo avanza hacia donde apunta la flecha. Sirve para <b>acercarte al rival</b>, alejarte de un golpe " +
                    "o acomodarte antes de atacar.",
            objetivo = "Moverte con  <b>" + TutorialControles.Avanzar + "</b>",
            requiereTurno = true,
            ataque = "Move",
            energiaMinima = costoMover
        });

        lista.Add(new Paso
        {
            titulo = "Esperar  (gratis)",
            texto = "Esperar <b>no gasta energía</b>: pasás el turno sin hacer nada. Es útil cuando no te alcanza la energía " +
                    "o querés juntar +1 antes de gastar en un golpe fuerte.",
            objetivo = "Esperá con  <b>" + TutorialControles.Esperar + "</b>",
            requiereTurno = true,
            ataque = "Wait"
        });

        lista.Add(new Paso
        {
            titulo = "El shuriken  (" + costoShuriken + " de energía)",
            texto = "Lanza un proyectil que <b>rebota hasta 3 veces en los bordes</b> antes de desaparecer: pega desde lejos, " +
                    "sin que tengas que acercarte. Al lanzarlo tu trompo retrocede un poco.",
            objetivo = "Lanzá el shuriken con  <b>" + TutorialControles.Derecha + "</b>  y lográ un impacto",
            requiereTurno = true,
            ataque = "LaunchShuriken",
            separar = true,
            requiereGolpe = true,
            energiaMinima = costoShuriken,
            pistaFallo = "No impactó. Con apuntar hacia el rival alcanza; si rebotó, esperá tu turno y probá otra vez."
        });

        lista.Add(new Paso
        {
            titulo = "Los pinchos  (" + costoPinchos + " de energía)",
            texto = "Soltás pinchos hacia adelante: <b>dañan a quien los pise y lo obligan a esperar más</b> para su próximo turno. " +
                    "Son una trampa: sirven para frenar el ritmo del rival.",
            objetivo = "Soltá los pinchos con  <b>" + TutorialControles.Izquierda + "</b>  y lográ un impacto",
            requiereTurno = true,
            ataque = "LaunchPinchos",
            separar = true,
            requiereGolpe = true,
            energiaMinima = costoPinchos,
            pistaFallo = "Los pinchos tienen que tocar al rival. Apuntá hacia él y volvé a intentar en tu próximo turno."
        });

        lista.Add(new Paso
        {
            titulo = "El clon  (" + costoClon + " de energía)",
            texto = "Lanzás un <b>clon</b> de tu trompo hacia un costado y vos salís despedido para el lado contrario. " +
                    "El clon choca con lo que encuentre, así que sirve para atacar desde otro ángulo.",
            objetivo = "Lanzá el clon con  <b>" + TutorialControles.R1 + "</b>",
            requiereTurno = true,
            ataque = "LaunchClone",
            energiaMinima = costoClon
        });

        lista.Add(new Paso
        {
            titulo = "El veneno  (" + costoVeneno + " de energía)",
            texto = "Es un especial con <b>minijuego de timing</b>: apretá  <b>" + TutorialControles.Esperar + "</b>  justo cuando el indicador pase " +
                    "por la zona marcada. Si lo clavás, el veneno queda cargado en tu próximo golpe y daña al rival " +
                    "cada segundo; si fallás, te lo comés vos. Es caro, pero el daño sigue aunque no lo toques.",
            objetivo = "Usá el veneno con  <b>" + TutorialControles.L1 + "</b>",
            requiereTurno = true,
            ataque = "Poison",
            energiaMinima = costoVeneno
        });

        lista.Add(new Paso
        {
            titulo = "Contagiar el veneno",
            texto = "El veneno viaja con tu <b>próximo ataque</b>: cuando golpeás al rival se lo pasás. " +
                    "Probalo con el ataque básico <b>" + TutorialControles.Atacar + "</b> o con un shuriken <b>" + TutorialControles.Derecha + "</b>.",
            objetivo = "Golpeá al rival con cualquier ataque",
            requiereTurno = true,
            condicion = () => golpeEnPaso,
            separar = true,
            energiaMinima = Mathf.Max(costoBasico, costoShuriken)
        });

        lista.Add(new Paso
        {
            titulo = "La sustitución  (" + costoSustitucion + " de energía)",
            texto = "Es una jugada <b>defensiva</b>: crea un señuelo que viaja con tu próximo proyectil y te deja " +
                    "<b>cambiar de lugar con él</b> para escaparte de un golpe. Cuesta poco, pero hay que saber cuándo usarla.",
            objetivo = "Usá la sustitución con  <b>" + TutorialControles.L2 + "</b>",
            requiereTurno = true,
            ataque = "Sustitution",
            energiaMinima = costoSustitucion
        });

        lista.Add(new Paso
        {
            titulo = "La sierra  (" + costoSierra + " de energía, la más cara)",
            texto = "Embestís al rival y, si lo alcanzás, <b>quedan trabados un instante</b>: en ese momento cada vez que apretás " +
                    "<b>" + TutorialControles.Esperar + "</b> le hacés daño. ¡Apretalo lo más rápido que puedas!",
            objetivo = "Usá la sierra con  <b>" + TutorialControles.Abajo + "</b>  y lográ un impacto",
            requiereTurno = true,
            ataque = "Sierra",
            separar = true,
            requiereGolpe = true,
            energiaMinima = costoSierra,
            pistaFallo = "La sierra tiene que alcanzar al rival. Apuntá hacia él y probá otra vez."
        });

        // ---- Defensa y cierre ----
        lista.Add(new Paso
        {
            titulo = "Defensa: el parry",
            texto = "Cuando <b>NO es tu turno</b> podés apretar  <b>" + TutorialControles.L1 + "</b>  justo antes de recibir un golpe: " +
                    "tu trompo queda inmune un instante y rechaza el choque. Cuesta 1 de energía y, si fallás, tenés que " +
                    "esperar un segundo para poder intentarlo de nuevo.\n" +
                    "Hoy el rival no ataca, así que no se puede practicar acá, pero se usa mucho en el 1 contra 1.",
            soloLeer = true
        });

        lista.Add(new Paso
        {
            titulo = "¡A ganar!",
            texto = "Ya sabés lo básico. Al rival le queda <b>muy poca vida</b>: usá lo que quieras para dejarla en 0. " +
                    "Gana el primero que se quede sin vida.",
            objetivo = "Dejá la vida del rival en 0",
            requiereTurno = true,
            condicion = () => RivalDerrotado(),
            energiaMinima = costoShuriken,
            alEntrar = PrepararFinal,
            resaltar = () => VidaVisual(turnoCpu != null ? turnoCpu.transform.parent : null)
        });

        return lista;
    }

    // ------------------------------------------------------------------ flujo de pasos

    private void Entrar(int nuevo)
    {
        if (actual != null && actual.alSalir != null) actual.alSalir();

        indice = nuevo;
        if (indice >= pasos.Count)
        {
            actual = null;
            Terminar();
            return;
        }

        actual = pasos[indice];
        usosPaso = 0;
        golpeTrasUso = false;
        golpeEnPaso = false;
        cumplido = false;
        tiempoCumplido = 0f;
        avisoEnergia = false;
        separadoEnPaso = false;

        if (actual.alEntrar != null) actual.alEntrar();

        // El cartel y el marco se arman una sola vez por paso: lo que cambia cuadro a cuadro
        // es solo la linea de estado (ver Dibujar).
        MostrarCartel();
        panel.Resaltar(actual.resaltar != null ? actual.resaltar() : null);
    }

    private void MostrarCartel()
    {
        estadoMostrado = null;
        string objetivo = string.IsNullOrEmpty(actual.objetivo) ? "" : "► " + actual.objetivo;

        // Un solo boton hace de "continuar" y de "saltar" (ver PidioSaltar).
        panel.Mostrar("Paso " + (indice + 1) + " de " + pasos.Count, actual.titulo, actual.texto, objetivo,
            "ENTER: saltar     R3: saltar");
    }

    // Si se conecta o desconecta un joystick con el tutorial abierto (los inalambricos a veces
    // recien aparecen al apretar un boton), los nombres de botones de los pasos se rehacen.
    private void ActualizarControles()
    {
        if (Time.unscaledTime < proximoChequeoControles) return;
        proximoChequeoControles = Time.unscaledTime + 0.5f;

        string firma = TutorialControles.Firma;
        if (firma == firmaControles) return;
        firmaControles = firma;
        Debug.Log("Tutorial: control detectado: " + TutorialControles.Detectado); // para diagnosticar si los nombres no coinciden

        List<Paso> nuevos = CrearPasos();
        for (int i = 0; i < pasos.Count && i < nuevos.Count; i++)
        {
            pasos[i].titulo = nuevos[i].titulo;
            pasos[i].texto = nuevos[i].texto;
            pasos[i].objetivo = nuevos[i].objetivo;
            pasos[i].pistaFallo = nuevos[i].pistaFallo;
        }

        if (actual != null && !termino) MostrarCartel();
    }

    private void Terminar()
    {
        if (termino) return;
        termino = true;

        PlayerPrefs.SetInt("TutorialCompletado", 1);
        PlayerPrefs.Save();

        panel.Resaltar(null);
        panel.Mostrar("", "¡Ganaste! Tutorial completo",
            "Ya probaste el ataque básico, el movimiento, el shuriken, los pinchos, el clon, el veneno, la sustitución y la sierra. " +
            "Cada personaje tiene sus propios ataques para descubrir. ¡Ahora probá un 1 contra 1!",
            "", "Para volver al menú:  " + TutorialControles.ConfirmarFinal);
    }

    private void SiguientePaso()
    {
        Entrar(indice + 1);
    }

    // ------------------------------------------------------------------ bucle principal

    private void Update()
    {
        if (!listo) return;

        ActualizarControles();
        ActualizarSensores();
        MoverCpu();
        AnimarCordon();

        if (termino)
        {
            // Normalmente EndGame ya se ocupa de volver al menu; esto cubre que no exista.
            if (fin == null && Controles.Confirmar()) SceneManager.LoadScene("InitMenu");
            return;
        }

        if (actual == null) return;

        AsegurarEnergia();
        AcomodarTrompos();
        ActualizarPaso();
    }

    private void LateUpdate()
    {
        if (!listo) return;

        // Se hace despues de que el juego corrio su cuadro, asi lo que el juego escribe
        // (por ejemplo "isCountingDown = true" al empezar el turno) se pisa en el mismo cuadro.
        if (relojTurno != null) relojTurno.isCountingDown = false;

        if (retenerCuenta && cuentaInicial != null) cuentaInicial.countDownTime = cuentaOriginal;

        ProtegerVidas();
    }

    // ------------------------------------------------------------------ sensores

    // Lee cambios que el juego ya expone, sin tocarlo: que ataque se gasto (el contador de
    // energia anota cada uso por nombre), si el rival perdio vida y si el jugador espero.
    private void ActualizarSensores()
    {
        bool huboUso = false;
        foreach (KeyValuePair<string, int> uso in energiaJ.ataquesUsados)
        {
            int previo;
            conteoPrevio.TryGetValue(uso.Key, out previo);
            if (uso.Value > previo)
            {
                conteoPrevio[uso.Key] = uso.Value;
                RegistrarUso(uso.Key);
                huboUso = true;
            }
        }

        // El ataque basico no queda anotado en el contador: se reconoce porque baja la energia
        // sin que otro ataque la haya gastado.
        int energia = energiaJ.currentEnergy;
        if (energia < energiaPrevia && !huboUso) RegistrarUso("BasicAttack");
        energiaPrevia = energia;

        // Esperar es gratis, no deja rastro en la energia: se reconoce por el boton.
        if (turnoJ.isTurnActive && Controles.Esperar(0)) RegistrarUso("Wait");

        if (vidaCpu.vidaActual < vidaCpuPrevia - 0.01f)
        {
            golpeEnPaso = true;
            if (usosPaso > 0) golpeTrasUso = true;
        }
    }

    private void RegistrarUso(string clave)
    {
        if (actual != null && actual.ataque == clave) usosPaso++;
    }

    // ------------------------------------------------------------------ CPU y proteccion

    // La CPU es un maniqui: cuando le toca el turno, espera. Se la deja un instante para no
    // cerrarle el turno en el mismo cuadro en que el juego se lo da.
    private void MoverCpu()
    {
        if (turnoCpu == null || !turnoCpu.isTurnActive)
        {
            tiempoTurnoCpu = 0f;
            tiempoCierreCpu = 0f;
            return;
        }

        tiempoTurnoCpu += Time.unscaledDeltaTime;
        if (tiempoTurnoCpu < ReaccionCpu) return;

        if (esperaCpu != null && tiempoCierreCpu <= 0f) esperaCpu.DoWait();

        tiempoCierreCpu += Time.unscaledDeltaTime;
        if (tiempoCierreCpu >= CierreForzadoCpu)
        {
            turnoCpu.PlayerChoseAnAction(0.5f, 1000f, false);
            tiempoCierreCpu = 0f;
        }
    }

    private void ProtegerVidas()
    {
        if (rivalProtegido)
        {
            float piso = vidaCpu.VidaMaxima * PisoVidaRival;
            if (vidaCpu.vidaActual < piso) vidaCpu.vidaActual = piso;
        }

        float pisoJugador = Mathf.Max(1f, vidaJ.VidaMaxima * PisoVidaJugador);
        if (vidaJ.vidaActual < pisoJugador) vidaJ.vidaActual = pisoJugador;

        // Se guarda DESPUES de proteger: el sensor de golpes compara contra la vida ya corregida.
        vidaCpuPrevia = vidaCpu.vidaActual;
    }

    private void AsegurarEnergia()
    {
        if (actual.energiaMinima <= 0 || !turnoJ.isTurnActive) return;
        if (energiaJ.currentEnergy >= actual.energiaMinima) return;

        // "Ninguno" es el nombre que el contador no anota como ataque usado.
        energiaJ.ChangeEnergy(actual.energiaMinima - energiaJ.currentEnergy, "Ninguno");
        energiaPrevia = energiaJ.currentEnergy;
        avisoEnergia = true;
    }

    // Los trompos arrancan a unas 3,5 unidades y la gravedad los junta: para el momento del turno
    // estan pegados y la flecha de apuntar queda tapada. Cuando llega el turno de un paso que
    // necesita verla, el director los acomoda a ambos lados del centro y gira la flecha hacia el
    // rival. Se hace con el juego congelado (turno), asi que no se ve ningun salto.
    private void AcomodarTrompos()
    {
        if (!actual.separar || separadoEnPaso || !turnoJ.isTurnActive) return;
        separadoEnPaso = true;

        Rigidbody2D cuerpoJ = turnoJ.GetComponent<Rigidbody2D>();
        Rigidbody2D cuerpoCpu = turnoCpu.GetComponent<Rigidbody2D>();
        if (cuerpoJ == null || cuerpoCpu == null) return;

        // El estadio esta centrado en el origen del mundo.
        Colocar(cuerpoJ, new Vector2(-DistanciaPractica * 0.5f, 0f));
        Colocar(cuerpoCpu, new Vector2(DistanciaPractica * 0.5f, 0f));

        // El rival queda a la derecha del jugador: la flecha del jugador mira a 0 grados.
        GirarFlecha(contenedorJ, 0f);
    }

    private static void Colocar(Rigidbody2D cuerpo, Vector2 posicion)
    {
        cuerpo.position = posicion;
        cuerpo.transform.position = posicion;
        cuerpo.linearVelocity = Vector2.zero; // el giro (angularVelocity) se deja: es el trompo girando
    }

    // El Prompter guarda su angulo y lo reaplica en cada cuadro mientras es el turno; mover solo
    // la rotacion no alcanza.
    private static void GirarFlecha(Transform contenedor, float grados)
    {
        Prompter flecha = contenedor != null ? contenedor.GetComponentInChildren<Prompter>(true) : null;
        if (flecha == null) return;

        if (campoAnguloFlecha != null) campoAnguloFlecha.SetValue(flecha, grados);
        flecha.transform.rotation = Quaternion.Euler(0f, 0f, grados);
    }

    private void PrepararFinal()
    {
        rivalProtegido = false;
        if (vidaCpu.vidaActual > VidaRivalFinal) vidaCpu.vidaActual = VidaRivalFinal;
        vidaCpuPrevia = vidaCpu.vidaActual;
    }

    // ------------------------------------------------------------------ logica del paso

    private void ActualizarPaso()
    {
        // Saltar el paso (por si algo no sale o ya lo conoce).
        if (PidioSaltar())
        {
            SiguientePaso();
            return;
        }

        if (!cumplido)
        {
            bool cumple = EvaluarCumplido();
            if (cumple)
            {
                cumplido = true;
                tiempoCumplido = 0f;
            }
        }

        if (cumplido)
        {
            // Los carteles que solo se leen avanzan en el momento en que el jugador aprieta
            // continuar; a los que piden una accion se les da un instante para ver el "¡Bien!".
            tiempoCumplido += Time.unscaledDeltaTime;
            float espera = actual.soloLeer && actual.condicion == null ? 0f : PausaTrasCumplir;
            if (tiempoCumplido >= espera)
            {
                SiguientePaso();
                return;
            }
        }

        Dibujar();
    }

    private bool EvaluarCumplido()
    {
        if (actual.soloLeer)
        {
            // Un cartel que habla de la pantalla de turno no se puede cerrar antes de que
            // llegue el turno: se estaria explicando algo que todavia no se ve.
            if (actual.requiereTurno && !turnoJ.isTurnActive) return false;

            return actual.condicion != null && actual.condicion(); // los que solo se leen avanzan por PidioSaltar
        }

        if (actual.ataque != null)
        {
            if (usosPaso <= 0) return false;
            if (!actual.requiereGolpe) return true;
            return golpeTrasUso || usosPaso >= IntentosMaximos;
        }

        return actual.condicion != null && actual.condicion();
    }

    // Pasar un cartel que solo se lee y saltar un paso es lo mismo: avanzar. Por eso hay UN solo
    // boton para las dos cosas: ENTER en el teclado, R3 (apretar el stick derecho) en el joystick.
    // Ninguno de los dos es una accion de combate, asi que no dispara nada del juego.
    private bool PidioSaltar()
    {
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            return true;

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad pad = Gamepad.all[i];
            if (pad != null && pad.rightStickButton.wasPressedThisFrame) return true;
        }
        return false;
    }

    // ---- Coreografia del lanzamiento ----

    private void IniciarCordon()
    {
        tiempoCordon = 0f;
        animandoCordon = barraCordon != null;
        cordonListo = barraCordon == null; // sin barra que animar no hay nada que esperar
        if (barraCordon != null) barraCordon.value = 0f;
    }

    // El cordon entra solo en el lanzador y se queda quieto: el juego sigue pausado
    // (timeScale = 0 desde que arranca la escena) hasta que el jugador aprieta el boton.
    private void AnimarCordon()
    {
        if (!animandoCordon) return;

        tiempoCordon += Time.unscaledDeltaTime;
        float avance = Mathf.Clamp01(tiempoCordon / DuracionCordon);
        barraCordon.value = Mathf.SmoothStep(0f, PosicionCordonAdentro, avance);

        if (avance >= 1f)
        {
            animandoCordon = false;
            cordonListo = true;
        }
    }

    // El jugador solto el lanzador: la vida inicial se fija con la posicion del cordon (en el
    // juego lo hace TimingBar, que aca esta apagado) y se libera la cuenta regresiva.
    private void SoltarLanzamiento()
    {
        animandoCordon = false;
        if (barraCordon != null) barraCordon.value = PosicionCordonAdentro;

        vidaJ.WaitForInfo(PosicionCordonAdentro);
        RetenerCuenta(false);
    }

    private bool RivalDerrotado()
    {
        return vidaCpu.vidaActual <= 0f || (fin != null && fin.gameFinished);
    }

    private void RetenerCuenta(bool retener)
    {
        retenerCuenta = retener;

        // Mientras se retiene no se muestra el "3" ni suenan los pitidos.
        if (textoCuenta != null) textoCuenta.enabled = !retener;
        if (sonidoCuenta != null) sonidoCuenta.mute = retener;
        if (!retener && cuentaInicial != null && !cuentaInicial.finished) cuentaInicial.countDownTime = cuentaOriginal;
    }

    // ------------------------------------------------------------------ cartel

    // Solo la linea de estado cambia mientras el paso esta abierto, y solo se reescribe si
    // cambio (reescribirla igual obligaria a recalcular el layout del cartel en cada cuadro).
    private void Dibujar()
    {
        string estado;

        if (cumplido)
        {
            estado = actual.ataque != null && actual.requiereGolpe && !golpeTrasUso
                ? "Seguimos: a veces no impacta, es parte del juego."
                : "¡Bien hecho!";
        }
        else if (actual.estadoExtra != null)
        {
            estado = actual.estadoExtra();
        }
        else if (actual.requiereTurno && !turnoJ.isTurnActive)
        {
            estado = "Esperando tu turno… empieza cuando tu trompo baja la velocidad.";
        }
        else if (actual.requiereGolpe && usosPaso > 0 && !golpeTrasUso && !string.IsNullOrEmpty(actual.pistaFallo))
        {
            estado = actual.pistaFallo + "  (intento " + usosPaso + " de " + IntentosMaximos + ")";
        }
        else if (separadoEnPaso)
        {
            estado = "Acomodamos los trompos para que se vea bien la flecha de apuntar.";
        }
        else if (avisoEnergia && actual.energiaMinima > 0)
        {
            estado = "Te regalamos la energía necesaria para practicar este ataque.";
        }
        else
        {
            estado = "";
        }

        if (estado == estadoMostrado) return;
        estadoMostrado = estado;
        panel.Estado(estado);
    }
}
