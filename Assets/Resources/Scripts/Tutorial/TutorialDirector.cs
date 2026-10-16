using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// ============================================================================
//  DIRECTOR DEL TUTORIAL
//  ----------------------------------------------------------------------------
//  Corre encima de la escena Practica (pelea contra la computadora) y la convierte en
//  una clase guiada: va mostrando carteles con el paso a seguir y espera a que el
//  jugador lo haga de verdad antes de pasar al siguiente. Explica el PORQUE de cada
//  cosa, no solo la tecla.
//
//  NO MODIFICA NINGUN SCRIPT NI ESCENA DEL JUEGO. Todo lo hace desde afuera, leyendo
//  datos que el juego ya expone (energia gastada, vida, turno activo...) y moviendo
//  solo los campos publicos pensados para eso. Lo que cambia, y por que:
//
//   - Deja solo Ninja contra Ninja. En Practica el Caballero del jugador viene prendido
//     de fabrica y ActiveCharacter solo prende, nunca apaga: apareceria duplicado.
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
    private const float PisoVidaRival = 0.4f;     // fraccion de su vida maxima por debajo de la cual no baja
    private const float PisoVidaJugador = 0.5f;
    private const float VidaRivalFinal = 15f;     // vida con la que se deja al rival para el ultimo paso
    private const float PausaTrasCumplir = 1.5f;  // se muestra "¡Bien!" antes de pasar al siguiente
    private const float ReaccionCpu = 0.35f;      // la CPU espera un instante antes de pasar su turno
    private const float CierreForzadoCpu = 1.2f;  // si la espera no cerro el turno, se cierra a la fuerza
    private const int IntentosMaximos = 3;        // golpes fallidos antes de dar el paso por bueno

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
        public Func<Transform> resaltar;
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

    private static readonly FieldInfo campoBarraDetenida =
        typeof(TimingBar).GetField("barStopped", BindingFlags.NonPublic | BindingFlags.Instance);

    // ------------------------------------------------------------------ arranque

    private void Awake()
    {
        // Antes de que corra cualquier Start: asi EndGame y el resto arman sus listas ya
        // sin los personajes de mas.
        DejarSoloElPersonaje("Jugador1");
        DejarSoloElPersonaje("CPU");

        panel = new GameObject("TutorialPanel").AddComponent<TutorialPanel>();
        panel.transform.SetParent(transform, false);
    }

    // Se busca entre las raices de la escena (no con GameObject.Find) para no confundir el
    // "CPU" o el "Jugador1" de verdad con otro objeto que se llame igual mas adentro.
    private static void DejarSoloElPersonaje(string nombreRaiz)
    {
        foreach (GameObject raiz in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (raiz.name != nombreRaiz) continue;

            Transform elegido = raiz.transform.Find(TutorialModo.PersonajeTutorial);
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
        ConstruirPasos();

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
        barraJ = contenedorJ.GetComponentInChildren<TimingBar>(true);
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

    private void ConstruirPasos()
    {
        int costoShuriken = Costo<LaunchShuriken>(2);
        int costoPinchos = Costo<LaunchPinchos>(2);
        int costoClon = Costo<LaunchClone>(2);
        int costoSierra = Costo<Cierra>(4);
        int costoSustitucion = Costo<InvokeSustitution>(2);
        const int costoVeneno = 4; // ActivePoison no expone su costo
        const int costoBasico = 1;
        const int costoMover = 1;

        // ---- Antes de pelear ----
        pasos.Add(new Paso
        {
            titulo = "¡Bienvenido al entrenamiento!",
            texto = "Acá aprendés a jugar peleando contra un trompo que se queda quieto, así podés probar todo sin apuros. " +
                    "Vas a manejar a un <b>Ninja</b>.\n" +
                    "El objetivo: dejar la <b>VIDA</b> del rival en 0. La pelea no es en tiempo real, es por <b>TURNOS</b>: " +
                    "cada turno elegís UNA acción, y mientras elegís el juego se congela.",
            soloLeer = true
        });

        pasos.Add(new Paso
        {
            titulo = "Paso 1: el lanzamiento",
            texto = "Antes de pelear lanzás tu trompo. La barra se mueve sola y la frenás con un botón: " +
                    "<b>cuanto más llena esté, más vida inicial tiene tu trompo</b>. " +
                    "En una partida normal tenés 3 segundos; acá tenés todo el tiempo que quieras.",
            objetivo = "Frená la barra con  " + "[" + TutorialControles.Esperar + "]",
            condicion = BarraDetenida,
            alSalir = () => RetenerCuenta(false), // recien ahora corre el 3, 2, 1, GO!
            resaltar = () => barraJ != null ? barraJ.transform : null
        });

        pasos.Add(new Paso
        {
            titulo = "¡Buen lanzamiento!",
            texto = "Tu vida inicial ya quedó fijada con ese lanzamiento. Ahora viene la cuenta regresiva: " +
                    "cuando diga <b>GO!</b> empieza la pelea. Los trompos se mueven solos y se van frenando.",
            soloLeer = true,
            condicion = () => cuentaInicial == null || cuentaInicial.finished // se cierra solo al empezar la pelea
        });

        // ---- Primer turno ----
        pasos.Add(new Paso
        {
            titulo = "Tu turno",
            texto = "Cuando tu trompo baja la velocidad llega <b>TU TURNO</b>: el juego se congela y elegís UNA acción " +
                    "(atacar, moverte, esperar o un especial). Al empezar cada turno ganás <b>+1 de energía</b>.\n" +
                    "En una partida real tenés 10 segundos para decidir; <b>en el tutorial el reloj está detenido</b>.",
            soloLeer = true,
            requiereTurno = true
        });

        pasos.Add(new Paso
        {
            titulo = "La energía",
            texto = "Estas barritas son tu <b>ENERGÍA</b>. Cada acción gasta un poco y, si no te alcanza, no se puede usar. " +
                    "Ganás +1 por turno y <b>+2 cada vez que golpeás al rival</b>: atacar te devuelve energía.\n" +
                    "Para practicar, el tutorial te regala la que necesites en cada paso.",
            soloLeer = true,
            requiereTurno = true,
            resaltar = Energia
        });

        pasos.Add(new Paso
        {
            titulo = "La vida",
            texto = "El círculo de cada trompo es su <b>VIDA</b>: cuando llega a 0, ese trompo pierde. " +
                    "Cada golpe le saca vida al rival y además <b>lo hace esperar más para su próximo turno</b>, " +
                    "así que pegar también te da ventaja de tiempo.",
            soloLeer = true,
            requiereTurno = true,
            resaltar = () => VidaVisual(turnoCpu != null ? turnoCpu.transform.parent : null)
        });

        pasos.Add(new Paso
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
            resaltar = Flecha
        });

        pasos.Add(new Paso
        {
            titulo = "El menú de ataques",
            texto = "Mientras mantenés apretado el botón se despliega el menú de <b>ATAQUES</b>: ahí ves qué ataque tiene " +
                    "cada dirección y cuánta energía cuesta. El botón [" + TutorialControles.Menu2 + "] despliega los <b>ESPECIALES</b>.",
            objetivo = "Mantené apretado  [" + TutorialControles.Menu1 + "]",
            requiereTurno = true,
            condicion = () => Controles.Menu1(0) || Controles.Menu2(0)
        });

        // ---- Acciones, de la mas simple a la mas elaborada ----
        pasos.Add(new Paso
        {
            titulo = "El ataque básico  (" + costoBasico + " de energía)",
            texto = "Es tu ataque más barato: tu trompo <b>embiste hacia donde apunta la flecha</b>. " +
                    "Si impacta, el rival pierde vida y vos recuperás +2 de energía, por eso conviene acertar.",
            objetivo = "Atacá con  [" + TutorialControles.Atacar + "]  y lográ un impacto",
            requiereTurno = true,
            ataque = "BasicAttack",
            requiereGolpe = true,
            energiaMinima = costoBasico,
            pistaFallo = "No impactó. Esperá tu próximo turno, mirá que la flecha apunte al rival y volvé a intentar."
        });

        pasos.Add(new Paso
        {
            titulo = "Moverse  (" + costoMover + " de energía)",
            texto = "Tu trompo avanza hacia donde apunta la flecha. Sirve para <b>acercarte al rival</b>, alejarte de un golpe " +
                    "o acomodarte antes de atacar.",
            objetivo = "Moverte con  [" + TutorialControles.Avanzar + "]",
            requiereTurno = true,
            ataque = "Move",
            energiaMinima = costoMover
        });

        pasos.Add(new Paso
        {
            titulo = "Esperar  (gratis)",
            texto = "Esperar <b>no gasta energía</b>: pasás el turno sin hacer nada. Es útil cuando no te alcanza la energía " +
                    "o querés juntar +1 antes de gastar en un golpe fuerte.",
            objetivo = "Esperá con  [" + TutorialControles.Esperar + "]",
            requiereTurno = true,
            ataque = "Wait"
        });

        pasos.Add(new Paso
        {
            titulo = "El shuriken  (" + costoShuriken + " de energía)",
            texto = "Lanza un proyectil que <b>rebota hasta 3 veces en los bordes</b> antes de desaparecer: pega desde lejos, " +
                    "sin que tengas que acercarte. Al lanzarlo tu trompo retrocede un poco.",
            objetivo = "Lanzá el shuriken con  [" + TutorialControles.Derecha + "]  y lográ un impacto",
            requiereTurno = true,
            ataque = "LaunchShuriken",
            requiereGolpe = true,
            energiaMinima = costoShuriken,
            pistaFallo = "No impactó. Con apuntar hacia el rival alcanza; si rebotó, esperá tu turno y probá otra vez."
        });

        pasos.Add(new Paso
        {
            titulo = "Los pinchos  (" + costoPinchos + " de energía)",
            texto = "Soltás pinchos hacia adelante: <b>dañan a quien los pise y lo obligan a esperar más</b> para su próximo turno. " +
                    "Son una trampa: sirven para frenar el ritmo del rival.",
            objetivo = "Soltá los pinchos con  [" + TutorialControles.Izquierda + "]  y lográ un impacto",
            requiereTurno = true,
            ataque = "LaunchPinchos",
            requiereGolpe = true,
            energiaMinima = costoPinchos,
            pistaFallo = "Los pinchos tienen que tocar al rival. Apuntá hacia él y volvé a intentar en tu próximo turno."
        });

        pasos.Add(new Paso
        {
            titulo = "El clon  (" + costoClon + " de energía)",
            texto = "Lanzás un <b>clon</b> de tu trompo hacia un costado y vos salís despedido para el lado contrario. " +
                    "El clon choca con lo que encuentre, así que sirve para atacar desde otro ángulo.",
            objetivo = "Lanzá el clon con  [" + TutorialControles.R1 + "]",
            requiereTurno = true,
            ataque = "LaunchClone",
            energiaMinima = costoClon
        });

        pasos.Add(new Paso
        {
            titulo = "El veneno  (" + costoVeneno + " de energía)",
            texto = "Es un especial con <b>minijuego de timing</b>: apretá  [" + TutorialControles.Esperar + "]  justo cuando el indicador pase " +
                    "por la zona marcada. Si lo clavás, el veneno queda cargado en tu próximo golpe y daña al rival " +
                    "cada segundo; si fallás, te lo comés vos. Es caro, pero el daño sigue aunque no lo toques.",
            objetivo = "Usá el veneno con  [" + TutorialControles.L1 + "]",
            requiereTurno = true,
            ataque = "Poison",
            energiaMinima = costoVeneno
        });

        pasos.Add(new Paso
        {
            titulo = "Contagiar el veneno",
            texto = "El veneno viaja con tu <b>próximo ataque</b>: cuando golpeás al rival se lo pasás. " +
                    "Probalo con el ataque básico [" + TutorialControles.Atacar + "] o con un shuriken [" + TutorialControles.Derecha + "].",
            objetivo = "Golpeá al rival con cualquier ataque",
            requiereTurno = true,
            condicion = () => golpeEnPaso,
            energiaMinima = Mathf.Max(costoBasico, costoShuriken)
        });

        pasos.Add(new Paso
        {
            titulo = "La sustitución  (" + costoSustitucion + " de energía)",
            texto = "Es una jugada <b>defensiva</b>: crea un señuelo que viaja con tu próximo proyectil y te deja " +
                    "<b>cambiar de lugar con él</b> para escaparte de un golpe. Cuesta poco, pero hay que saber cuándo usarla.",
            objetivo = "Usá la sustitución con  [" + TutorialControles.L2 + "]",
            requiereTurno = true,
            ataque = "Sustitution",
            energiaMinima = costoSustitucion
        });

        pasos.Add(new Paso
        {
            titulo = "La sierra  (" + costoSierra + " de energía, la más cara)",
            texto = "Embestís al rival y, si lo alcanzás, <b>quedan trabados un instante</b>: en ese momento cada vez que apretás " +
                    "[" + TutorialControles.Esperar + "] le hacés daño. ¡Apretalo lo más rápido que puedas!",
            objetivo = "Usá la sierra con  [" + TutorialControles.Abajo + "]  y lográ un impacto",
            requiereTurno = true,
            ataque = "Sierra",
            requiereGolpe = true,
            energiaMinima = costoSierra,
            pistaFallo = "La sierra tiene que alcanzar al rival. Apuntá hacia él y probá otra vez."
        });

        // ---- Defensa y cierre ----
        pasos.Add(new Paso
        {
            titulo = "Defensa: el parry",
            texto = "Cuando <b>NO es tu turno</b> podés apretar  [" + TutorialControles.L1 + "]  justo antes de recibir un golpe: " +
                    "tu trompo queda inmune un instante y rechaza el choque. Cuesta 1 de energía y, si fallás, tenés que " +
                    "esperar un segundo para poder intentarlo de nuevo.\n" +
                    "Hoy el rival no ataca, así que no se puede practicar acá, pero se usa mucho en el 1 contra 1.",
            soloLeer = true
        });

        pasos.Add(new Paso
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

        if (actual.alEntrar != null) actual.alEntrar();

        // El cartel y el marco se arman una sola vez por paso: lo que cambia cuadro a cuadro
        // es solo la linea de estado (ver Dibujar).
        estadoMostrado = null;
        string objetivo = string.IsNullOrEmpty(actual.objetivo) ? "" : "► " + actual.objetivo;
        string pie = actual.soloLeer && actual.condicion == null
            ? TutorialControles.Continuar + ": continuar     " + TutorialControles.Saltar + ": saltar"
            : TutorialControles.Saltar + ": saltar este paso";

        panel.Mostrar("Paso " + (indice + 1) + " de " + pasos.Count, actual.titulo, actual.texto, objetivo, pie);
        panel.Resaltar(actual.resaltar != null ? actual.resaltar() : null);
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
            "", "Para volver al menú:  ENTER / ESPACIO  o  cruz / A");
    }

    private void SiguientePaso()
    {
        Entrar(indice + 1);
    }

    // ------------------------------------------------------------------ bucle principal

    private void Update()
    {
        if (!listo) return;

        ActualizarSensores();
        MoverCpu();

        if (termino)
        {
            // Normalmente EndGame ya se ocupa de volver al menu; esto cubre que no exista.
            if (fin == null && Controles.Confirmar()) SceneManager.LoadScene("InitMenu");
            return;
        }

        if (actual == null) return;

        AsegurarEnergia();
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

            return actual.condicion != null ? actual.condicion() : PidioContinuar();
        }

        if (actual.ataque != null)
        {
            if (usosPaso <= 0) return false;
            if (!actual.requiereGolpe) return true;
            return golpeTrasUso || usosPaso >= IntentosMaximos;
        }

        return actual.condicion != null && actual.condicion();
    }

    private bool PidioContinuar()
    {
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
             Keyboard.current.spaceKey.wasPressedThisFrame))
            return true;

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad pad = Gamepad.all[i];
            if (pad != null && pad.startButton.wasPressedThisFrame) return true;
        }
        return false;
    }

    private bool PidioSaltar()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame) return true;

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad pad = Gamepad.all[i];
            if (pad != null && pad.rightStickButton.wasPressedThisFrame) return true;
        }
        return false;
    }

    private bool BarraDetenida()
    {
        if (barraJ != null && campoBarraDetenida != null && (bool)campoBarraDetenida.GetValue(barraJ)) return true;
        return Controles.Esperar(0);
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
        else if (actual.requiereTurno && !turnoJ.isTurnActive)
        {
            estado = "Esperando tu turno… empieza cuando tu trompo baja la velocidad.";
        }
        else if (actual.requiereGolpe && usosPaso > 0 && !golpeTrasUso && !string.IsNullOrEmpty(actual.pistaFallo))
        {
            estado = actual.pistaFallo + "  (intento " + usosPaso + " de " + IntentosMaximos + ")";
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
