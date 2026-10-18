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
    private const float EsperaTurnoLibre = 4f;    // en la pelea final: segundos de juego sin turno de nadie antes de abrirle uno al jugador
    private const float RetrasoTrasChoque = 0.7f; // segundos que se deja ver el choque antes de abrir el menu
    private const float EsperaTurnoCpu = 1f;      // segundos de juego antes de darle el turno a la CPU para que ataque
    private const float PausaAntesDelGolpeCpu = 1.2f; // segundos reales, con todo quieto, antes de que la CPU embista
    private const float EsperaTurno = 2.6f;       // segundos de juego sin turno antes de pedirselo al juego (ver ForzarTurno)
    private const float DistanciaPractica = 3f;  // distancia entre los trompos cuando se acomodan para apuntar
    private const float VidaRival = 100f;        // el juego deja a la CPU con muy poca vida en esta escena (ver PrepararCpu)

    // Un paso del tutorial. Es solo datos: la logica comun esta en Actualizar().
    private class Paso
    {
        public string titulo;
        public string texto;
        public string objetivo;          // lo que hay que hacer (vacio = solo leer)
        public bool soloLeer;            // avanza con Continuar
        public bool requiereTurno;       // el paso se cumple en el turno del jugador (y se le pide el turno al juego si tarda)
        public bool ocultoHastaTurno;    // el cartel no se ve mientras no sea el turno (ej: durante el choque)
        public string ataque;            // clave con la que el juego registra el uso (ver UsoRegistrado)
        public bool requiereGolpe;       // ademas de usarlo, tiene que impactar al rival
        public Func<bool> condicion;     // condicion propia (si no hay "ataque")
        public Func<string> estadoExtra; // linea de estado propia del paso (vacia = la comun)
        public Func<Transform> resaltar;
        public bool resaltarConCirculo;  // el resaltado es un circulo alrededor del objeto (si no, un marco)
        public bool cpuAtaca;            // en este paso la CPU toma su turno y ataca al jugador
        public bool separar;             // al llegar el turno, se acomodan los trompos para que se vea la flecha
        public Type[] permitidos;        // scripts del jugador (menus y acciones) que responden en este paso (null = todos)
        public bool cartelBreve;         // ENTER / R3 esconden el cartel en vez de saltar el paso (la pelea sigue sin el)
        public bool avanzaSolo;          // al cumplirse pasa al siguiente sin esperar ENTER / R3
        public Type menuFijo;            // ExpandMenu o ExpandMenu2: ese menu queda abierto (y resaltado) hasta que se cumple el paso
        public bool menuAbiertoAlEntrar; // el menu fijo se abre al entrar al paso (si no, cuando el jugador aprieta su boton)
        public bool demoEnergia;         // mientras se lee, la barra de energia baja y sube para mostrar como funciona
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
    private Transform contenedorCpu;
    private LaunchShuriken ataqueCpu; // el golpe con el que la CPU le pega al jugador en su paso
    private EnergyCounter energiaCpu;

    // Scripts de accion del jugador (ataque, mover, esperar, especiales): se prenden y apagan segun
    // lo que permite cada paso. Los menus (ExpandMenu) no estan aca: siempre se pueden abrir.
    private MonoBehaviour[] accionesJ;
    private static readonly Type[] Ninguna = new Type[0];
    private bool cartelCerrado; // en el paso final: el jugador ya escondio el cartel con ENTER / R3

    // Menu fijo del paso (ver Paso.menuFijo): el menu que el director mantiene abierto a mano.
    private GameObject menuFijado;

    // Demo de la energia: fases de un ciclo que se repite mientras se lee el cartel.
    private const float FaseDemoEnergia = 1.4f; // segundos por fase
    private const int GastoDemoEnergia = 4;     // lo que "gasta" el golpe de la demo (un especial)
    private const int RecuperoDemoEnergia = 2;  // lo que devuelve un impacto
    private float tiempoDemoEnergia;
    private int faseDemoEnergia = -1;

    // Pelea final: si el jugador se queda sin actuar en su turno, vuelve el cartel con una ayuda.
    private const float AyudaTrasSegundos = 6f;
    private float tiempoSinAccionJ;
    private bool mostrandoAyuda;

    // Minijuego del veneno: si el indicador esta en la zona (campo privado de Indicator).
    private static readonly FieldInfo campoEnZona =
        typeof(Indicator).GetField("isTrigger", BindingFlags.NonPublic | BindingFlags.Instance);

    // El menu que abre cada ExpandMenu (campo privado): hay que cerrarlo si se apaga el script con el
    // boton apretado, porque es el script el que lo esconde al soltar.
    private static readonly FieldInfo campoMenu1 =
        typeof(ExpandMenu).GetField("menuToExpand", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo campoMenu2 =
        typeof(ExpandMenu2).GetField("menuToExpand", BindingFlags.NonPublic | BindingFlags.Instance);

    // El veneno: si el jugador falla el minijuego de timing, el veneno se le activa a el mismo y no
    // queda nada para contagiar; el paso de contagio se conforma entonces con cualquier golpe.
    private bool venenoPerdido;
    private bool golpeAlRivalEnPaso; // el rival perdio vida durante este paso (sin importar con que)

    // Ultimo paso: la pelea sigue sola, con la CPU pensando y sin ayudas del director.
    private bool peleaLibre;

    // Estado de los sensores.
    private readonly Dictionary<string, int> conteoPrevio = new Dictionary<string, int>();
    private int energiaPrevia;
    private float vidaCpuPrevia;

    // Estado del paso actual.
    private int usosPaso;
    private bool golpeTrasUso;
    private bool cumplido;
    private float tiempoCumplido;
    private bool avisoEnergia;
    private bool separadoEnPaso;
    private bool cartelVisible = true;
    private float tiempoSinTurno;
    private bool choqueDetectado;
    private float tiempoDelChoque;

    // Contraataque del rival (el paso en que la CPU ataca al jugador).
    private bool cpuAtacaPendiente;
    private bool cpuAtaco;
    private bool cpuAcomodado;
    private float tiempoCpuAcomodado;
    private bool golpeAlJugador;
    private float tiempoCpuAtaco;
    private float tiempoSinTurnoCpu;
    private float vidaJPrevia;

    // Solo para el registro de la Console (diagnostico).
    private bool turnoJPrevio;
    private bool turnoCpuPrevio;
    private float proximoRegistro;
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

    // Bandera con la que CheckPlayerTurn dice "todavia no podes tener turno" (se pone en false con
    // cada golpe y hasta que pasa el enfriamiento). Los choques seguidos la mantienen apagada y el
    // turno no llegaba nunca; el director la levanta cuando le pide el turno al juego.
    private static readonly FieldInfo campoTurnoPosible =
        typeof(CheckPlayerTurn).GetField("isTurnPosible", BindingFlags.NonPublic | BindingFlags.Instance);

    // Angulo de la flecha de apuntar (campo privado de Prompter).
    private static readonly FieldInfo campoAnguloFlecha =
        typeof(Prompter).GetField("angle", BindingFlags.NonPublic | BindingFlags.Instance);

    // Duracion del reloj de turno (campo privado de CountDownRPG). Hace falta repararlo porque en esta
    // escena nace en 0 (ver RepararRelojDeTurno).
    private static readonly FieldInfo campoDuracionTurno =
        typeof(CountDownRPG).GetField("initialCountDownTime", BindingFlags.NonPublic | BindingFlags.Instance);
    private const float DuracionTurnoRespaldo = 10f; // la del juego, por si no se pudo leer a tiempo
    private float duracionTurno;

    // El arbitro de turnos del juego: con el se abre el turno para que quede anotado como un turno real.
    private RPGTurn arbitro;

    // ------------------------------------------------------------------ arranque

    private void Awake()
    {
        // Se lee ANTES de prender al Ninja: prenderlo dispara su CheckPlayerTurn.Awake, que pone el
        // reloj en cero (ver RepararRelojDeTurno).
        LeerDuracionDelTurno();

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
        RepararRelojDeTurno();
        pasos.AddRange(CrearPasos());
        firmaControles = TutorialControles.Firma;
        Debug.Log("Tutorial: control detectado: " + TutorialControles.Detectado);

        energiaPrevia = energiaJ.currentEnergy;
        vidaCpuPrevia = vidaCpu.vidaActual;
        vidaJPrevia = vidaJ.vidaActual;

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
        contenedorCpu = turnoCpu.transform.parent;
        if (vidaJ == null || vidaCpu == null || contenedorJ == null || contenedorCpu == null) return false;

        energiaJ = contenedorJ.GetComponentInChildren<EnergyCounter>(true);
        if (barraJ == null) barraJ = contenedorJ.GetComponentInChildren<TimingBar>(true);
        if (energiaJ == null) return false;

        cerebroCpu = contenedorCpu.GetComponent<AIBrain>();
        if (cerebroCpu == null) cerebroCpu = contenedorCpu.GetComponentInChildren<AIBrain>(true);
        esperaCpu = contenedorCpu.GetComponentInChildren<WaitOption>(true);
        ataqueCpu = contenedorCpu.GetComponentInChildren<LaunchShuriken>(true);
        energiaCpu = contenedorCpu.GetComponentInChildren<EnergyCounter>(true);
        accionesJ = BuscarAccionesDelJugador();

        cuentaInicial = FindFirstObjectByType<CountDown>();
        relojTurno = FindFirstObjectByType<CountDownRPG>();
        // Los dos viven en un objeto que se prende al "GO!".
        fin = FindFirstObjectByType<EndGame>(FindObjectsInactive.Include);
        arbitro = FindFirstObjectByType<RPGTurn>(FindObjectsInactive.Include);

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

    private void LeerDuracionDelTurno()
    {
        CountDownRPG reloj = FindFirstObjectByType<CountDownRPG>();
        duracionTurno = reloj != null && reloj.countDownTime > 0f ? reloj.countDownTime : DuracionTurnoRespaldo;
    }

    // POR QUE: CountDownRPG guarda su duracion en Awake, pero CheckPlayerTurn.Awake llama a
    // PlayerChoseAnAction -> CountDownRPG.ResetCountDown(), que copia esa duracion (todavia 0) sobre
    // countDownTime. Si el Awake del trompo corre primero -y aca corre primero SIEMPRE, porque el
    // director prende al Ninja desde su propio Awake- el reloj queda en 0 para toda la partida y
    // CheckPlayerTurn.Update cierra cualquier turno con reloj en 0 en el mismo cuadro en que empieza:
    // nunca aparece el menu. Se repone la duracion leida antes de prender al Ninja, tambien en el
    // campo privado, porque ResetCountDown la vuelve a copiar en cada cierre de turno.
    private void RepararRelojDeTurno()
    {
        if (relojTurno == null) return;

        relojTurno.countDownTime = duracionTurno;
        if (campoDuracionTurno != null) campoDuracionTurno.SetValue(relojTurno, duracionTurno);
    }

    // ------------------------------------------------------------------ pasos

    private Transform Energia() { return energiaJ != null ? energiaJ.transform : null; }

    private Transform VidaVisual(Transform contenedor)
    {
        if (contenedor == null) return null;
        CircularBar barra = contenedor.GetComponentInChildren<CircularBar>(true);
        return barra != null ? barra.transform : null;
    }

    // El lanzador en si (el dibujo donde entra el cordon), no toda la barra: el circulo lo rodea a el.
    private Transform Lanzador()
    {
        if (barraJ == null) return null;

        Transform dibujo = barraJ.transform.Find("Image (1)");
        return dibujo != null ? dibujo : barraJ.transform;
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
    //
    // Siguen el hilo de una pelea real, en 8 pasos: lanzamiento, choque, el menu de golpes que
    // aparece al llegar el turno, la energia, un primer golpe, como impacta, el contraataque del
    // rival y un ultimo golpe para ganar.
    private List<Paso> CrearPasos()
    {
        List<Paso> lista = new List<Paso>();

        const int costoBasico = 1;
        const int costoVeneno = 4; // ActivePoison no expone su costo
        int costoShuriken = Costo<LaunchShuriken>(2);

        // 1. Bienvenida
        lista.Add(new Paso
        {
            titulo = "¡Bienvenido!",
            texto = "Acá aprendés a jugar.\n" +
                    "Tu objetivo: dejar la <b>VIDA</b> del rival en 0.",
            soloLeer = true
        });

        // 2. Lanzamiento (el juego esta pausado hasta que el jugador suelta)
        lista.Add(new Paso
        {
            titulo = "Paso 1: el lanzamiento",
            texto = "Antes de pelear lanzás tu trompo. El cordón entra en el lanzador y vos elegís el momento de soltar: " +
                    "<b>cuanto más adentro esté el cordón, más vida inicial tiene tu trompo</b>.",
            objetivo = "Cuando el cordón esté adentro, apretá  <b>" + TutorialControles.Esperar + "</b>",
            condicion = () => cordonListo && Controles.Esperar(0),
            avanzaSolo = true, // al soltar arranca la cuenta regresiva: no tiene sentido pedir otro boton
            alEntrar = IniciarCordon,
            alSalir = SoltarLanzamiento, // recien ahora corre el 3, 2, 1, GO! y los trompos chocan
            estadoExtra = () => cordonListo
                ? "El cordón está completamente adentro del lanzador: ¡ahora!"
                : "Mirá cómo el cordón entra en el lanzador…",
            resaltar = Lanzador,
            resaltarConCirculo = true
        });

        // Mientras se ensena el menu, ningun otro boton responde: solo el del menu (y ENTER / R3 para
        // saltar). Despues, en cada paso de golpe, responde unicamente el golpe que se esta ensenando.

        // 3. El choque, y recien al llegar el turno aparece el menu de golpes (y este cartel). El paso se
        //    cumple cuando el jugador MANTIENE el boton del menu: ahi el juego despliega los golpes.
        lista.Add(new Paso
        {
            titulo = "Tu turno: el menú de golpes",
            texto = "Los trompos chocaron y el tuyo bajó la velocidad: <b>ahora es tu turno</b>. El juego se detiene y aparece tu menú. " +
                    "En cada turno elegís <b>UNA</b> acción. Mientras mantenés apretado <b>" + TutorialControles.Menu1 + "</b> " +
                    "se despliegan tus golpes: cada dirección de la " + (TutorialControles.ConJoystick ? "cruceta" : "W A S D") + " es uno.",
            objetivo = "Apretá  <b>" + TutorialControles.Menu1 + "</b>  para abrir el menú de golpes",
            requiereTurno = true,
            ocultoHastaTurno = true,
            separar = true,
            condicion = () => turnoJ.isTurnActive && Controles.Menu1(0),
            avanzaSolo = true, // abrir el menu ya es la accion: se sigue directo con el golpe
            menuFijo = typeof(ExpandMenu), // queda abierto para que se vean los golpes
            permitidos = Ninguna,
            estadoExtra = () => menuFijado != null ? "Esos son tus golpes: cada dirección es uno." : "",
            resaltar = MenuFijoOBotonera
        });

        // 4. El primer golpe, con el basico (es el unico que responde)
        lista.Add(new Paso
        {
            titulo = "Tu primer golpe",
            texto = "Empezá por el ataque básico: tu trompo <b>embiste hacia donde apunta la flecha</b>. " +
                    (TutorialControles.ConJoystick
                        ? "La flecha ya apunta al rival; si querés, la movés con <b>el STICK DERECHO</b>."
                        : "La flecha ya apunta al rival."),
            objetivo = "Atacá con  <b>" + TutorialControles.Atacar + "</b>  y lográ un impacto",
            requiereTurno = true,
            ataque = "BasicAttack",
            requiereGolpe = true,
            energiaMinima = costoBasico,
            menuFijo = typeof(ExpandMenu),
            menuAbiertoAlEntrar = true, // sigue abierto desde el paso anterior y se cierra con el golpe
            permitidos = new[] { typeof(BasicAttack) },
            resaltar = MenuFijoOBotonera,
            pistaFallo = "No impactó. Esperá tu próximo turno, mirá que la flecha apunte al rival y volvé a intentar."
        });

        // 5. La energia, ahora que la barra se movio (gasto el golpe y devolvio el impacto)
        lista.Add(new Paso
        {
            titulo = "La energía",
            texto = "Estas barritas son tu <b>ENERGÍA</b>. Cada golpe gasta un poco (los especiales, más) y, si no te alcanza, no se puede usar. " +
                    "Ganás <b>+1</b> al empezar cada turno y <b>+2</b> por cada impacto. Mirá la barra: te lo mostramos.\n" +
                    "Para practicar, el tutorial te regala la que necesites.",
            soloLeer = true,
            requiereTurno = true,
            demoEnergia = true,
            estadoExtra = EstadoDemoEnergia,
            alSalir = TerminarDemoEnergia,
            permitidos = Ninguna,
            resaltar = Energia
        });

        // 6. Como impacta
        lista.Add(new Paso
        {
            titulo = "El impacto",
            texto = "Mirá la <b>VIDA</b> del rival (el círculo): bajó con tu golpe, y cuando llegue a 0 pierde. " +
                    "Cada golpe además <b>lo hace esperar más para su próximo turno</b>.",
            soloLeer = true,
            requiereTurno = true,
            permitidos = Ninguna,
            resaltar = () => VidaVisual(contenedorCpu)
        });

        // 7. El menu de especiales, con el otro boton (mismo protocolo que el de golpes)
        lista.Add(new Paso
        {
            titulo = "Los especiales",
            texto = "Tu trompo también tiene <b>ESPECIALES</b>: cuestan más energía pero hacen más. " +
                    "Mientras mantenés apretado <b>" + TutorialControles.Menu2 + "</b> se despliegan.",
            objetivo = "Apretá  <b>" + TutorialControles.Menu2 + "</b>  para abrir el menú de especiales",
            requiereTurno = true,
            condicion = () => turnoJ.isTurnActive && Controles.Menu2(0),
            avanzaSolo = true, // idem menu de golpes: directo al especial
            menuFijo = typeof(ExpandMenu2),
            permitidos = Ninguna,
            estadoExtra = () => menuFijado != null ? "Esos son tus especiales." : "",
            resaltar = MenuFijoOBotonera
        });

        // 8. El veneno: se activa con un minijuego de timing y queda cargado para el proximo golpe
        lista.Add(new Paso
        {
            titulo = "El veneno  (" + costoVeneno + " de energía)",
            texto = "Es un especial con <b>minijuego</b>: al activarlo aparece un indicador, y tenés que apretar " +
                    "<b>" + TutorialControles.Esperar + "</b> justo cuando pasa por la zona marcada. Si acertás, el veneno queda " +
                    "<b>cargado en tu próximo golpe</b>; si fallás, te lo comés vos.",
            objetivo = "Activá el veneno con  <b>" + TutorialControles.L1 + "</b>  y apretá  <b>" + TutorialControles.Esperar + "</b>  cuando diga ¡AHORA!",
            requiereTurno = true,
            ataque = "Poison",
            condicion = VenenoResuelto,
            estadoExtra = EstadoDelVeneno,
            energiaMinima = costoVeneno,
            menuFijo = typeof(ExpandMenu2),
            menuAbiertoAlEntrar = true,
            permitidos = new[] { typeof(ActivePoison) },
            resaltar = ResaltarVeneno
        });

        // 9. Contagiarlo: con el proximo golpe el veneno pasa al rival y le baja la vida cada segundo
        lista.Add(new Paso
        {
            titulo = "Contagiá el veneno",
            texto = "El veneno viaja con tu <b>próximo golpe</b>: pegale al rival con el ataque básico " +
                    "(<b>" + TutorialControles.Atacar + "</b>) o con el shuriken (<b>" + TutorialControles.Derecha + "</b>, pega de lejos) " +
                    "y mirá cómo <b>pierde vida cada segundo</b>.",
            objetivo = "Golpeá al rival para contagiarlo",
            requiereTurno = true,
            separar = true,
            condicion = () => venenoPerdido ? golpeAlRivalEnPaso : RivalEnvenenado(),
            estadoExtra = () => RivalEnvenenado() ? "¡Envenenado! Mirá cómo le baja la vida." : "",
            energiaMinima = costoShuriken,
            menuFijo = typeof(ExpandMenu),
            menuAbiertoAlEntrar = true,
            permitidos = new[] { typeof(BasicAttack), typeof(LaunchShuriken) },
            resaltar = () => VidaVisual(contenedorCpu)
        });

        // 10. El rival ataca: toma su turno y te tira un shuriken
        lista.Add(new Paso
        {
            titulo = "Ahora ataca el rival",
            texto = "Acá los dos se pegan. Ahora le toca al rival: te va a tirar un <b>shuriken</b> y vas a ver cómo baja <b>tu VIDA</b>. " +
                    "Cuando recibís un golpe también <b>tenés que esperar más</b> para tu próximo turno.",
            cpuAtaca = true,
            alEntrar = PrepararContraataque,
            condicion = () => cpuAtaco && (golpeAlJugador || tiempoCpuAtaco > 4f),
            estadoExtra = () => !cpuAtaco
                ? "El rival está por atacar…"
                : (golpeAlJugador ? "¡Te pegó! Mirá cómo bajó tu vida." : "Mirá el ataque del rival…"),
            permitidos = Ninguna,
            resaltar = () => VidaVisual(contenedorJ)
        });

        // 11. La pelea sigue sola, con todas las acciones y la CPU jugando. El cartel se cierra con
        //     ENTER / R3 y recien al caer el rival aparece el de "ganaste".
        lista.Add(new Paso
        {
            titulo = "Ahora, la pelea de verdad",
            texto = "Ya sabés lo básico. Desde acá la pelea sigue <b>como siempre</b>: el rival también piensa y ataca, tenés todos tus " +
                    "golpes y especiales, y cada turno dura 10 segundos. Al rival le queda <b>poca vida</b>: ¡dejala en 0 y ganá!\n" +
                    "Recordá: en tu turno mantené <b>" + TutorialControles.Menu1 + "</b> o <b>" + TutorialControles.Menu2 + "</b> para ver " +
                    "qué podés hacer, y apretá la dirección o el botón del golpe.",
            objetivo = "Cerrá este cartel y dejá la vida del rival en 0",
            condicion = () => RivalDerrotado(),
            avanzaSolo = true, // al caer el rival aparece el "ganaste" sin pedir otro boton
            alEntrar = PelearLibre,
            estadoExtra = () => mostrandoAyuda
                ? "¿Te colgaste? Es tu turno: mantené " + TutorialControles.Menu1 + " y apretá " + TutorialControles.Atacar + " para embestir."
                : "",
            cartelBreve = true
        });

        return lista;
    }

    // El rival ataca en cuanto le toque el turno (ver MoverCpu); el director se lo da si tarda. Si el
    // jugador tenia su turno abierto (juego congelado) se le cierra: en este paso no tiene botones y
    // nadie mas podria destrabarlo.
    private void PrepararContraataque()
    {
        cpuAtacaPendiente = true;
        if (turnoJ.isTurnActive) turnoJ.PlayerChoseAnAction(0.5f, 1000f, false);
    }

    // Mientras el rival ataca, el jugador no puede recibir turno: si lo recibiera, el juego se
    // congelaria con el golpe de la CPU a medio camino.
    private void DormirJugador()
    {
        if (actual == null || !actual.cpuAtaca || cumplido || campoTurnoPosible == null) return;
        campoTurnoPosible.SetValue(turnoJ, false);
    }

    // ---- Veneno ----
    // El veneno del juego es un objeto "Poison" que cuelga del trompo que lo activo (y abre un
    // minijuego de timing, PressButtomTime). Si el timing sale bien queda inactivo esperando el proximo
    // golpe, que lo cuelga del rival y lo activa; si sale mal, se activa sobre el propio jugador.

    private static Poison VenenoEn(Transform trompo)
    {
        return trompo != null ? trompo.GetComponentInChildren<Poison>() : null;
    }

    private bool MinijuegoDeVenenoEnCurso()
    {
        return FindFirstObjectByType<PressButtomTime>() != null;
    }

    // El paso del veneno termina cuando se uso y el minijuego cerro. Ahi se sabe si quedo cargado o si
    // el jugador se envenemo a si mismo.
    private bool VenenoResuelto()
    {
        if (usosPaso <= 0 || MinijuegoDeVenenoEnCurso()) return false;

        Poison veneno = VenenoEn(turnoJ.transform);
        venenoPerdido = veneno == null || veneno.isActive;
        return true;
    }

    private bool RivalEnvenenado()
    {
        Poison veneno = VenenoEn(turnoCpu.transform);
        return veneno != null && veneno.isActive;
    }

    private string EstadoDelVeneno()
    {
        if (usosPaso <= 0) return "";
        if (MinijuegoDeVenenoEnCurso())
        {
            return IndicadorEnZona()
                ? "¡AHORA!  Apretá " + TutorialControles.Esperar
                : "Esperá… apretá " + TutorialControles.Esperar + " cuando el indicador entre en la zona marcada";
        }
        return venenoPerdido
            ? "Se te escapó el timing y te envenenaste vos: en el juego también pasa. Seguimos igual."
            : "¡Cargado! Tu próximo golpe se lo contagia al rival.";
    }

    private static Indicator IndicadorDelVeneno()
    {
        return FindFirstObjectByType<Indicator>();
    }

    private static bool IndicadorEnZona()
    {
        Indicator indicador = IndicadorDelVeneno();
        return indicador != null && campoEnZona != null && (bool)campoEnZona.GetValue(indicador);
    }

    // Mientras corre el minijuego se resalta el indicador; antes, el menu de especiales.
    private Transform ResaltarVeneno()
    {
        Indicator indicador = IndicadorDelVeneno();
        return indicador != null ? indicador.transform : MenuFijoOBotonera();
    }

    // ---- Demo de la energia ----
    // Con el juego congelado (es el turno del jugador) la barra baja y sube sola, en ciclo:
    // llena -> gasta un especial -> recupera por un impacto -> llena otra vez.
    private void DemoEnergia()
    {
        if (!actual.demoEnergia || energiaJ == null) return;

        tiempoDemoEnergia += Time.unscaledDeltaTime;
        int fase = Mathf.FloorToInt(tiempoDemoEnergia / FaseDemoEnergia) % 3;
        if (fase == faseDemoEnergia) return;
        faseDemoEnergia = fase;

        int maximo = energiaJ.energias.Count;
        int objetivo = fase == 0 ? maximo : (fase == 1 ? maximo - GastoDemoEnergia : maximo - GastoDemoEnergia + RecuperoDemoEnergia);
        energiaJ.ChangeEnergy(objetivo - energiaJ.currentEnergy, "Ninguno");
        energiaPrevia = energiaJ.currentEnergy; // que el sensor no lo tome por un ataque
    }

    private string EstadoDemoEnergia()
    {
        switch (faseDemoEnergia)
        {
            case 1: return "Un especial gasta " + GastoDemoEnergia + "…";
            case 2: return "…y cada impacto te devuelve " + RecuperoDemoEnergia + ".";
            default: return "Así se ve la energía llena.";
        }
    }

    private void TerminarDemoEnergia()
    {
        if (energiaJ == null) return;
        energiaJ.ChangeEnergy(energiaJ.energias.Count - energiaJ.currentEnergy, "Ninguno");
        energiaPrevia = energiaJ.currentEnergy;
    }

    // ---- Ayuda en la pelea final ----
    // Si el jugador tiene el turno y no hace nada por un rato, el cartel vuelve con un recordatorio.
    private void AyudarSiSeColgo()
    {
        if (!actual.cartelBreve) return;

        if (!turnoJ.isTurnActive)
        {
            tiempoSinAccionJ = 0f;
            return;
        }

        tiempoSinAccionJ += Time.unscaledDeltaTime;
        if (tiempoSinAccionJ >= AyudaTrasSegundos && cartelCerrado)
        {
            cartelCerrado = false;
            mostrandoAyuda = true;
        }
    }

    // Todos los scripts del juego que cuelgan de la botonera del jugador: los dos menus y las acciones.
    // Se reconocen porque los scripts del proyecto no tienen namespace (los de UI y TextMesh Pro si).
    // Se apagan y prenden con 'enabled': un script apagado no lee el boton y no actua.
    private MonoBehaviour[] BuscarAccionesDelJugador()
    {
        Transform botonera = Botonera();
        if (botonera == null) return new MonoBehaviour[0];

        List<MonoBehaviour> lista = new List<MonoBehaviour>();
        foreach (MonoBehaviour script in botonera.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (string.IsNullOrEmpty(script.GetType().Namespace)) lista.Add(script);
        }
        return lista.ToArray();
    }

    // Deja respondiendo solo lo que el paso permite. Asi, mientras se explica el menu de golpes, el de
    // especiales no se abre y ningun golpe sale (ni gasta turno ni energia).
    private void AplicarPermisos(Type[] permitidos)
    {
        if (accionesJ == null) return;

        for (int i = 0; i < accionesJ.Length; i++)
        {
            MonoBehaviour script = accionesJ[i];
            if (script == null) continue;

            bool permitido = permitidos == null || Array.IndexOf(permitidos, script.GetType()) >= 0;
            if (!permitido) CerrarMenuDe(script);
            script.enabled = permitido;
        }
    }

    private static GameObject MenuDe(MonoBehaviour script)
    {
        FieldInfo campo = script is ExpandMenu ? campoMenu1 : (script is ExpandMenu2 ? campoMenu2 : null);
        return campo != null ? campo.GetValue(script) as GameObject : null;
    }

    private static void CerrarMenuDe(MonoBehaviour script)
    {
        GameObject menu = MenuDe(script);
        if (menu != null) menu.SetActive(false);
    }

    // El menu de golpes (o el de especiales) del jugador, por el tipo de script que lo abre.
    private GameObject BuscarMenu(Type tipoMenu)
    {
        if (accionesJ == null) return null;
        for (int i = 0; i < accionesJ.Length; i++)
        {
            MonoBehaviour script = accionesJ[i];
            if (script != null && script.GetType() == tipoMenu) return MenuDe(script);
        }
        return null;
    }

    // En el juego el menu solo se ve mientras se mantiene el boton. En la clase conviene que quede
    // abierto: se prende a mano (el script que lo cierra al soltar esta apagado por los permisos) y
    // se vuelve a prender en cada cuadro por si algo lo apaga, hasta que el paso se cumple.
    private void FijarMenu(Type tipoMenu)
    {
        menuFijado = BuscarMenu(tipoMenu);
        if (menuFijado != null) menuFijado.SetActive(true);
    }

    private void SoltarMenu()
    {
        if (menuFijado != null) menuFijado.SetActive(false);
        menuFijado = null;
    }

    private void MantenerMenuFijo()
    {
        if (actual.menuFijo == null) return;

        // En los pasos de accion (el menu ya venia abierto) el golpe cierra el menu, como en el juego.
        // En los pasos que solo ensenan a abrirlo, queda abierto hasta el paso siguiente.
        if (cumplido && actual.menuAbiertoAlEntrar)
        {
            SoltarMenu();
            return;
        }

        if (menuFijado == null)
        {
            bool apreto = actual.menuFijo == typeof(ExpandMenu) ? Controles.Menu1(0) : Controles.Menu2(0);
            if (apreto && turnoJ.isTurnActive) FijarMenu(actual.menuFijo);
            return;
        }

        if (!menuFijado.activeSelf && turnoJ.isTurnActive) menuFijado.SetActive(true);
    }

    // Para resaltar: el menu fijo si esta abierto; si no, la botonera.
    private Transform MenuFijoOBotonera()
    {
        return menuFijado != null && menuFijado.activeInHierarchy ? menuFijado.transform : Botonera();
    }

    // La botonera del menu de golpes del jugador (el panel con los botones de accion).
    private Transform Botonera()
    {
        if (contenedorJ == null) return null;

        foreach (Transform hijo in contenedorJ.GetComponentsInChildren<Transform>(true))
        {
            if (hijo.name == "Buttoms") return hijo;
        }
        return null;
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
        cumplido = false;
        tiempoCumplido = 0f;
        avisoEnergia = false;
        separadoEnPaso = false;
        tiempoSinTurno = 0f;
        choqueDetectado = false;
        tiempoDelChoque = 0f;
        cartelVisible = true;
        cpuAtacaPendiente = false;
        cpuAtaco = false;
        cpuAcomodado = false;
        golpeAlJugador = false;
        tiempoCpuAtaco = 0f;
        tiempoSinTurnoCpu = 0f;
        cartelCerrado = false;
        golpeAlRivalEnPaso = false;
        tiempoDemoEnergia = 0f;
        faseDemoEnergia = -1;
        tiempoSinAccionJ = 0f;
        mostrandoAyuda = false;
        Registrar("paso " + (indice + 1) + " de " + pasos.Count + ": " + actual.titulo);

        AplicarPermisos(actual.permitidos);
        SoltarMenu();
        if (actual.menuFijo != null && actual.menuAbiertoAlEntrar) FijarMenu(actual.menuFijo);
        if (actual.alEntrar != null) actual.alEntrar();

        // El cartel y el marco se arman una sola vez por paso: lo que cambia cuadro a cuadro
        // es solo la linea de estado (ver Dibujar).
        AplicarVisibilidad(true);
    }

    // Los pasos "ocultoHastaTurno" no muestran el cartel (ni el marco) mientras no sea el turno del
    // jugador: asi, despues del lanzamiento se ve el choque limpio y el cartel aparece junto con el
    // menu de golpes. En el "cartelBreve" (la pelea final) el cartel se esconde cuando el jugador lo
    // cierra con ENTER / R3, y la pelea sigue sin el.
    private void AplicarVisibilidad(bool forzar)
    {
        bool debeVerse = !(actual.ocultoHastaTurno && !cumplido && !turnoJ.isTurnActive)
                      && !(actual.cartelBreve && cartelCerrado);
        if (!forzar && debeVerse == cartelVisible) return;

        cartelVisible = debeVerse;
        if (debeVerse)
        {
            MostrarCartel();
            panel.Resaltar(actual.resaltar != null ? actual.resaltar() : null, actual.resaltarConCirculo);
        }
        else
        {
            panel.OcultarCartel();
            panel.Resaltar(null);
        }
    }

    private void MostrarCartel()
    {
        estadoMostrado = null;
        string objetivo = string.IsNullOrEmpty(actual.objetivo) ? "" : "► " + actual.objetivo;

        // Un solo boton hace de "continuar" y de "saltar" (ver PidioSaltar). Ningun cartel se va solo.
        string pie = actual.cartelBreve ? "ENTER: cerrar     R3: cerrar"
                   : cumplido ? "ENTER: continuar     R3: continuar"
                   : "ENTER: saltar     R3: saltar";
        panel.Mostrar("", actual.titulo, actual.texto, objetivo, pie);
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

        if (actual != null && !termino && cartelVisible) MostrarCartel();
    }

    private void Terminar()
    {
        if (termino) return;
        termino = true;

        PlayerPrefs.SetInt("TutorialCompletado", 1);
        PlayerPrefs.Save();

        bool gano = vidaCpu == null || vidaCpu.vidaActual <= 0f || (vidaJ != null && vidaJ.vidaActual > 0f);
        panel.Resaltar(null);
        panel.Mostrar("", gano ? "¡Ganaste! Tutorial completo" : "Tutorial completo",
            "Aprendiste cómo se lanza, el menú de golpes, la energía, los especiales y cómo se pelea por turnos: pegás, el rival te pega y gana el que se queda sin vida. " +
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
        RegistrarTurnos();
        MoverCpu();
        AnimarCordon();

        if (termino)
        {
            // EndGame vuelve al menu con el boton de abajo (X) o ENTER/ESPACIO; el director suma R3. Si no
            // hubiera EndGame, el director atiende todo.
            bool volver = PidioR3() || (fin == null && (Controles.Confirmar() || PidioSaltar()));
            if (volver)
            {
                // Igual que al terminar cualquier pelea: EndGame deja el juego congelado y asi llega al
                // menu (ChangeMap lo descongela al elegir un modo). Si llegara corriendo, los trompos del
                // fondo del menu se disparan entre si.
                Time.timeScale = 0f;
                SceneManager.LoadScene("InitMenu");
            }
            return;
        }

        if (actual == null) return;

        ForzarTurno();
        AsegurarEnergia();
        AcomodarTrompos();
        MantenerMenuFijo();
        DemoEnergia();
        AyudarSiSeColgo();
        ActualizarPaso();
    }

    // Despues del lanzamiento los trompos chocan y siguen rebotando: el del jugador casi nunca
    // baja de la velocidad que el juego pide para empezar el turno, y la clase se quedaria
    // esperando sin fin. Pasado un rato de juego sin turno, se le PIDE el turno al juego con el
    // mismo metodo que usa la CPU (RequestTurnNow): se congela todo y aparece el menu de golpes.
    private void ForzarTurno()
    {
        bool corriendo = cuentaInicial == null || cuentaInicial.finished;

        // Pelea final: el juego juega solo, pero si los trompos quedan rebotando y nadie consigue turno
        // (cada golpe contra la pared lo posterga), se le abre uno al jugador para que no se trabe.
        if (peleaLibre)
        {
            if (turnoJ.isTurnActive || turnoCpu.isTurnActive || RivalDerrotado())
            {
                tiempoSinTurno = 0f;
                return;
            }

            tiempoSinTurno += Time.deltaTime;
            if (tiempoSinTurno >= EsperaTurnoLibre) PedirTurno(turnoJ);
            return;
        }

        // Turno de la CPU, solo en el paso en que tiene que atacar al jugador.
        if (actual.cpuAtaca && !cpuAtaco && !turnoCpu.isTurnActive && corriendo)
        {
            tiempoSinTurnoCpu += Time.deltaTime;
            if (tiempoSinTurnoCpu >= EsperaTurnoCpu) PedirTurno(turnoCpu);
        }
        else
        {
            tiempoSinTurnoCpu = 0f;
        }

        // Turno del jugador. Con el paso ya cumplido no se abre ninguno: quedaria un turno congelado
        // colgado al entrar al paso siguiente.
        if (!actual.requiereTurno || cumplido || turnoJ.isTurnActive || !corriendo)
        {
            tiempoSinTurno = 0f;
            return;
        }

        tiempoSinTurno += Time.deltaTime; // tiempo de juego: si el juego esta frenado, no cuenta

        // El primer turno llega con el choque: un instante despues del primer golpe entre los
        // trompos (para que se vea) se frena todo y aparece el menu. Si no hubiera golpe, vale el
        // tiempo de espera de siempre.
        bool llegoElMomento = actual.ocultoHastaTurno && choqueDetectado
            ? tiempoSinTurno - tiempoDelChoque >= RetrasoTrasChoque
            : tiempoSinTurno >= EsperaTurno;
        if (llegoElMomento) PedirTurno(turnoJ);

        // Si algo impide el turno, que quede escrito por que (se lee en la Console).
        if (Time.unscaledTime >= proximoRegistro)
        {
            proximoRegistro = Time.unscaledTime + 2f;
            Registrar("esperando el turno: " + tiempoSinTurno.ToString("0.0") + " s sin turno, timeScale = " +
                      Time.timeScale + ", cuenta inicial terminada = " + corriendo);
        }
    }

    // Abre el turno por la misma puerta que usa la CPU (RPGTurn.NotifyReadyImmediate): asi queda
    // anotado en el arbitro y, cuando el jugador actua, el cierre del turno destraba el juego como en
    // una partida normal. Antes se levanta la bandera de enfriamiento de CheckPlayerTurn: los choques
    // seguidos (cada golpe la apaga) impedirian que el trompo acepte el turno.
    // No se usa RequestTurnNow porque ademas exige que el reloj de turno no haya expirado, y en esta
    // escena habia que repararlo primero (ver RepararRelojDeTurno).
    private void PedirTurno(CheckPlayerTurn turno)
    {
        if (turno.isTurnActive) return;

        if (campoTurnoPosible != null) campoTurnoPosible.SetValue(turno, true);

        if (arbitro != null)
        {
            arbitro.NotifyReadyImmediate(turno);
        }
        else
        {
            // Sin arbitro no hay quien congele el juego: se hace a mano.
            turno.Ready();
            Time.timeScale = 0f;
        }
        Registrar("turno abierto por el tutorial");
    }

    // Diagnostico: deja en la Console cuando empieza y termina cada turno.
    private void Registrar(string mensaje)
    {
        Debug.Log("Tutorial: " + mensaje);
    }

    private void RegistrarTurnos()
    {
        if (turnoJ.isTurnActive != turnoJPrevio)
        {
            turnoJPrevio = turnoJ.isTurnActive;
            Registrar("turno del jugador " + (turnoJPrevio ? "EMPIEZA" : "termina") + " (timeScale = " + Time.timeScale + ")");
        }

        if (turnoCpu.isTurnActive != turnoCpuPrevio)
        {
            turnoCpuPrevio = turnoCpu.isTurnActive;
            Registrar("turno de la CPU " + (turnoCpuPrevio ? "EMPIEZA" : "termina") + " (timeScale = " + Time.timeScale + ")");
        }
    }

    private void LateUpdate()
    {
        if (!listo) return;

        if (retenerCuenta && cuentaInicial != null) cuentaInicial.countDownTime = cuentaOriginal;

        // En la pelea libre el juego corre con sus reglas: reloj de turno, CPU pensando y vidas reales.
        // Solo se siguen anotando las vidas, para que el sensor de golpes no registre el mismo golpe en
        // cada cuadro.
        if (peleaLibre)
        {
            vidaCpuPrevia = vidaCpu.vidaActual;
            vidaJPrevia = vidaJ.vidaActual;
            return;
        }

        DormirJugador();

        // Se hace despues de que el juego corrio su cuadro, asi lo que el juego escribe
        // (por ejemplo "isCountingDown = true" al empezar el turno) se pisa en el mismo cuadro.
        if (relojTurno != null) relojTurno.isCountingDown = false;

        DormirCpu();
        ProtegerVidas();
    }

    // La CPU no pide turno por su cuenta: si lo hiciera, el juego se congelaria y descongelaria a cada
    // rato y el trompo rival pareceria estar actuando. Se le baja la bandera de "puede tener turno" en
    // cada cuadro (el juego la vuelve a levantar tras cada enfriamiento), salvo en el paso en que el
    // director le da el turno para que ataque.
    private void DormirCpu()
    {
        if (cpuAtacaPendiente || turnoCpu == null || campoTurnoPosible == null) return;
        campoTurnoPosible.SetValue(turnoCpu, false);
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
        bool gastoEnergia = energia < energiaPrevia;
        if (gastoEnergia && !huboUso) RegistrarUso("BasicAttack");
        energiaPrevia = energia;

        // Esperar es gratis, no deja rastro en la energia: se reconoce por el boton.
        if (turnoJ.isTurnActive && Controles.Esperar(0)) RegistrarUso("Wait");

        // Cualquier gasto de energia o uso anotado cuenta como "el jugador hizo algo" (pelea final).
        if (huboUso || gastoEnergia) tiempoSinAccionJ = 0f;

        if (!choqueDetectado && (vidaCpu.vidaActual < vidaCpuPrevia - 0.01f || vidaJ.vidaActual < vidaJPrevia - 0.01f))
        {
            choqueDetectado = true;
            tiempoDelChoque = tiempoSinTurno;
            Registrar("primer choque detectado");
        }

        if (vidaCpu.vidaActual < vidaCpuPrevia - 0.01f)
        {
            golpeAlRivalEnPaso = true;
            if (usosPaso > 0) golpeTrasUso = true;
            Registrar("el rival recibio un golpe (vida " + vidaCpu.vidaActual + ")");
        }

        if (vidaJ.vidaActual < vidaJPrevia - 0.01f)
        {
            golpeAlJugador = true;
            Registrar("el jugador recibio un golpe (vida " + vidaJ.vidaActual + ")");
        }

        if (cpuAtaco) tiempoCpuAtaco += Time.unscaledDeltaTime;
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
        if (peleaLibre) return; // en la pelea libre decide su AIBrain

        if (turnoCpu == null || !turnoCpu.isTurnActive)
        {
            tiempoTurnoCpu = 0f;
            tiempoCierreCpu = 0f;
            return;
        }

        tiempoTurnoCpu += Time.unscaledDeltaTime;
        if (tiempoTurnoCpu < ReaccionCpu) return;

        if (cpuAtacaPendiente && ataqueCpu != null)
        {
            AtacarConLaCpu();
            return;
        }

        if (esperaCpu != null && tiempoCierreCpu <= 0f) esperaCpu.DoWait();

        tiempoCierreCpu += Time.unscaledDeltaTime;
        if (tiempoCierreCpu >= CierreForzadoCpu)
        {
            turnoCpu.PlayerChoseAnAction(0.5f, 1000f, false);
            tiempoCierreCpu = 0f;
        }
    }

    // El contraataque: los trompos se ubican frente a frente (misma distancia que al practicar), la
    // flecha de la CPU mira al jugador (a 180 grados) y la CPU le tira un shuriken con los mismos
    // metodos del juego.
    private void AtacarConLaCpu()
    {
        if (!cpuAcomodado)
        {
            cpuAcomodado = true;
            tiempoCpuAcomodado = 0f;
            Rigidbody2D cuerpoJ = turnoJ.GetComponent<Rigidbody2D>();
            Rigidbody2D cuerpoCpu = turnoCpu.GetComponent<Rigidbody2D>();
            if (cuerpoJ != null && cuerpoCpu != null)
            {
                Colocar(cuerpoJ, new Vector2(-DistanciaPractica * 0.5f, 0f));
                Colocar(cuerpoCpu, new Vector2(DistanciaPractica * 0.5f, 0f));
            }
            GirarFlecha(contenedorCpu, 180f);
            return;
        }

        // Un instante con el juego congelado (es el turno de la CPU) y el cartel "esta por atacar": si
        // embistiera en el mismo cuadro en que se acomodan los trompos, no se llegaria a ver.
        tiempoCpuAcomodado += Time.unscaledDeltaTime;
        if (tiempoCpuAcomodado < PausaAntesDelGolpeCpu) return;

        // La energia de la CPU no se usa en el tutorial: se le asegura la del ataque.
        int costo = ataqueCpu.EnergyCost;
        if (energiaCpu != null && energiaCpu.currentEnergy < costo) energiaCpu.ChangeEnergy(costo - energiaCpu.currentEnergy, "Ninguno");

        cpuAtacaPendiente = false;
        cpuAtaco = true;
        tiempoCpuAtaco = 0f;
        Registrar("la CPU lanza un shuriken");
        ataqueCpu.DoLaunch();
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
        vidaJPrevia = vidaJ.vidaActual;
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

    // Desde aca la pelea es la de siempre: la CPU vuelve a pensar, las vidas ya no tienen piso, el
    // reloj de turno corre y el director solo mira quien gana. Al rival se le deja poca vida para que
    // el cierre sea corto.
    private void PelearLibre()
    {
        peleaLibre = true;
        rivalProtegido = false;
        if (vidaCpu.vidaActual > VidaRivalFinal) vidaCpu.vidaActual = VidaRivalFinal;
        vidaCpuPrevia = vidaCpu.vidaActual;

        // Se le devuelve a la CPU el derecho a pedir turno (DormirCpu se lo venia negando) y se prende
        // su cerebro.
        if (campoTurnoPosible != null) campoTurnoPosible.SetValue(turnoCpu, true);
        if (cerebroCpu != null) cerebroCpu.enabled = true;
    }

    // ------------------------------------------------------------------ logica del paso

    private void ActualizarPaso()
    {
        // Saltar el paso (por si algo no sale o ya lo conoce). En el paso final no hay nada que saltar:
        // el boton solo esconde el cartel y la pelea sigue.
        if (PidioSaltar())
        {
            if (actual.cartelBreve)
            {
                cartelCerrado = true;
                mostrandoAyuda = false;
                tiempoSinAccionJ = 0f;
            }
            else
            {
                SiguientePaso();
                return;
            }
        }

        AplicarVisibilidad(false);

        if (!cumplido)
        {
            bool cumple = EvaluarCumplido();
            if (cumple)
            {
                cumplido = true;
                tiempoCumplido = 0f;
                if (cartelVisible) MostrarCartel(); // el pie pasa a decir "continuar"
            }
        }

        // Cumplido el paso, el cartel se queda ("¡Bien hecho!") hasta que el jugador aprieta ENTER / R3
        // (ver PidioSaltar arriba). Solo los pasos "avanzaSolo" siguen por su cuenta, tras un instante.
        if (cumplido && actual.avanzaSolo)
        {
            tiempoCumplido += Time.unscaledDeltaTime;
            if (tiempoCumplido >= PausaTrasCumplir)
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

        return PidioR3();
    }

    private static bool PidioR3()
    {
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
        if (barraCordon != null)
        {
            // La barra del lanzador va de a 11 escalones (pensada para frenarla con el boton): al
            // moverla por codigo se veia trabada y a saltos. En el tutorial se mueve sin escalones.
            barraCordon.numberOfSteps = 0;
            barraCordon.value = 0f;
        }
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
