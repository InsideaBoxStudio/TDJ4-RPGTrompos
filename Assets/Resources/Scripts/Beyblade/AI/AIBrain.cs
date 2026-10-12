using UnityEngine;

// ============================================================================
//  MÁQUINA DE ESTADOS FINITOS (FSM) PARA LA IA DEL TROMPO
//  ----------------------------------------------------------------------------
//  Patrón de diseño: STATE (Estado).
//   - La IA siempre está en UN estado (Acercarse, Atacar, Alejarse, Defender).
//   - Cuando es su turno, el estado actual DECIDE una acción y a qué estado ir.
//
//  La IA ejecuta sus acciones llamando a los MISMOS scripts que usa el jugador
//  (BasicAttack, MovementOption, WaitOption). Por eso su ataque hace daño
//  exactamente igual que el del jugador.
//
//  ESPECIALES: el cerebro NO conoce personajes. Todo lo que implemente IAIAction
//  dentro del personaje se descubre solo en Awake (ver IAIAction.cs). Agregar un
//  personaje o un especial nuevo no requiere tocar este archivo.
//
//  Contiene 3 partes:
//   1) La clase abstracta AIState (plantilla de todo estado).
//   2) Las 4 clases de estados concretos (heredan de AIState).
//   3) La clase AIBrain (MonoBehaviour) que se pega al trompo de la IA.
// ============================================================================


// ----------------------------------------------------------------------------
// 1) CLASE BASE ABSTRACTA: define qué es un "estado" de la IA.
// ----------------------------------------------------------------------------
public abstract class AIState
{
    protected AIBrain brain;

    public AIState(AIBrain brain)
    {
        this.brain = brain;
    }

    // Nombre legible (para mostrar en consola y depurar).
    public abstract string Name { get; }

    // Se ejecuta una vez por turno: decide la acción y la transición.
    public abstract void Execute();
}


// ----------------------------------------------------------------------------
// 2) ESTADOS CONCRETOS
// ----------------------------------------------------------------------------

// ACERCARSE: se mueve hacia el rival para poder atacarlo.
public class ApproachState : AIState
{
    public ApproachState(AIBrain brain) : base(brain) { }
    public override string Name => "Acercarse";

    public override void Execute()
    {
        if (brain.LifeIsLow())
        {
            brain.SetState(brain.retreatState);
            brain.PerformRetreat();
            return;
        }

        if (brain.DistanceToEnemy() <= brain.attackRange && brain.HasEnergyToAttack())
        {
            brain.SetState(brain.attackState);
            brain.PerformAttack();
            return;
        }

        if (!brain.HasEnergyToMove())
        {
            brain.SetState(brain.defendState);
            brain.PerformWait();
            return;
        }

        brain.PerformApproach();
    }
}


// ATACAR: embiste al rival.
public class AttackState : AIState
{
    public AttackState(AIBrain brain) : base(brain) { }
    public override string Name => "Atacar";

    public override void Execute()
    {
        if (!brain.HasEnergyToAttack())
        {
            brain.SetState(brain.defendState);
            brain.PerformWait();
            return;
        }

        if (brain.DistanceToEnemy() > brain.attackRange)
        {
            brain.SetState(brain.approachState);
            brain.PerformApproach();
            return;
        }

        if (brain.LifeIsLow())
        {
            brain.SetState(brain.retreatState);
            brain.PerformRetreat();
            return;
        }

        // En dificultad baja, a veces la IA "duda" y espera en vez de atacar
        // (esto la hace atacar poco). En Difícil casi nunca duda.
        if (brain.DudaYEspera())
        {
            brain.PerformWait();
            return;
        }

        // A veces lanza un especial (si tiene energía); si no, embestida normal.
        if (!brain.TryPerformSpecial())
        {
            brain.PerformAttack();
        }
    }
}


// ALEJARSE: comportamiento defensivo cuando la vida está baja.
public class RetreatState : AIState
{
    public RetreatState(AIBrain brain) : base(brain) { }
    public override string Name => "Alejarse";

    public override void Execute()
    {
        if (!brain.LifeIsLow())
        {
            brain.SetState(brain.approachState);
            brain.PerformApproach();
            return;
        }

        brain.PerformRetreat();
    }
}


// DEFENDER / ESPERAR: frena y no gasta energía, para recuperarse.
public class DefendState : AIState
{
    public DefendState(AIBrain brain) : base(brain) { }
    public override string Name => "Defender";

    public override void Execute()
    {
        if (brain.HasEnergyToAttack())
        {
            brain.SetState(brain.approachState);
            brain.PerformApproach();
            return;
        }

        brain.PerformWait();
    }
}


// ----------------------------------------------------------------------------
// 3) EL CEREBRO: MonoBehaviour que se pega al trompo de la IA.
// ----------------------------------------------------------------------------
public enum AIDifficulty { Facil, Normal, Dificil }

public class AIBrain : MonoBehaviour
{
    [Header("Referencias (se autocompletan si las dejás vacías)")]
    [SerializeField] private CheckPlayerTurn check;
    [SerializeField] private EnergyCounter energyCounter;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Vida vida;
    [SerializeField] private Transform enemy;
    [SerializeField] private CountDown countDown; // cuenta regresiva inicial: la IA espera el "GO!"

    [Header("Acciones del trompo (se autocompletan)")]
    [SerializeField] private BasicAttack attackAction;
    [SerializeField] private MovementOption moveAction;
    [SerializeField] private WaitOption waitAction;

    [Header("Acciones básicas de la estructura NUEVA (si existen, tienen prioridad sobre las de arriba)")]
    [SerializeField] private BasicAttackOption attackOption;
    [SerializeField] private Move moveOption;
    [SerializeField] private Wait waitOption;

    // Especiales del personaje: NO se listan acá. La IA descubre en Awake todo lo que
    // implemente IAIAction dentro de su propio personaje. Así el cerebro no conoce
    // personajes concretos y agregar uno nuevo no obliga a tocar este archivo.
    // Cada acción declara sola su costo y si es "fuerte" (ver IAIAction.cs).
    private IAIAction[] especiales;

    // Buffer reusable para elegir especial sin generar basura para el GC en cada
    // ataque de la IA (antes se creaba una List<Action> + closures por ataque).
    private readonly System.Collections.Generic.List<IAIAction> candidatos =
        new System.Collections.Generic.List<IAIAction>();

    [Header("Índice de jugador de la CPU (jugador 1 = 0, CPU = 1)")]
    [SerializeField] private bool forzarPlayerIndex = true; // reescribe el playerIndex de los scripts del trompo
    [SerializeField] private int playerIndexCPU = 1;        // la CPU es el jugador 2 (índice 1)

    [Header("Dificultad")]
    [SerializeField] public AIDifficulty difficulty = AIDifficulty.Normal;
    [SerializeField] private bool usarDificultadElegida = true; // si true, toma la elegida en la pantalla de selección

    [Header("Parámetros de decisión (se ajustan solos según la dificultad)")]
    [SerializeField] public float attackRange = 3f;   // a qué distancia decide atacar
    [SerializeField] public float lowLifeValue = 30f; // por debajo de esto se pone defensiva
    [SerializeField] public int attackEnergyCost = 1;
    [SerializeField] public int moveEnergyCost = 1;
    [SerializeField] public float aiReadyVelocity = 1.5f; // umbral de velocidad para pedir turno (mayor que el 1 del juego)
    [Range(0f, 1f)]
    [SerializeField] public float specialChance = 0.5f; // probabilidad de usar un especial al atacar

    // Cuando es true (solo en Difícil), la IA usa también los especiales FUERTES.
    private bool permitirEspecialesFuertes = true;
    // Demora antes de actuar en su turno (segundos). Fácil = lenta, Difícil = casi instantánea.
    private float demoraReaccion = 0f;
    // Probabilidad de "dudar" y esperar en vez de atacar (Fácil ataca poco). Difícil ≈ 0.
    private float probabilidadDudar = 0f;

    [HideInInspector] public ApproachState approachState;
    [HideInInspector] public AttackState attackState;
    [HideInInspector] public RetreatState retreatState;
    [HideInInspector] public DefendState defendState;

    private AIState currentState;
    private bool hasActedThisTurn = false;

    void Awake()
    {
        approachState = new ApproachState(this);
        attackState = new AttackState(this);
        retreatState = new RetreatState(this);
        defendState = new DefendState(this);
        currentState = approachState;

        // Convertir el trompo (copiado de Jugador1) a jugador 2: reescribe el playerIndex
        // de todos sus scripts. Así no hay que tocar índices a mano en el editor.
        if (forzarPlayerIndex) ForzarPlayerIndex();

        // Con el trompo "Base" de la estructura nueva, la CPU arma sus propios menús de
        // ataques (el humano los abre apretando un botón; la CPU nunca lo hace).
        CrearMenusDeAtaque();

        // Autocompletar referencias (este objeto + hijos). Se reintenta en Update
        // por si los componentes se activan tarde (sistema de selección de personajes).
        AutocompletarReferencias();

        // Buscar al rival (puede no estar activo todavía: el sistema de selección de
        // personajes lo activa en Awake, y el orden no está garantizado). Si no lo
        // encuentra acá, se reintenta en Update hasta que aparezca.
        BuscarRival();

        // Tomar la dificultad elegida en la pantalla de selección (si corresponde) y aplicarla.
        if (usarDificultadElegida) difficulty = SeleccionDificultad.DificultadElegida;
        AplicarDificultad();
    }

    // Marca al trompo de la CPU como jugador 2. Permite copiar el trompo de Jugador1
    // (índice 0) y que la IA lo convierta sin tocar nada a mano en el editor.
    //
    // Antes esto se hacía con reflection: recorría TODOS los MonoBehaviour del trompo
    // buscando campos llamados "playerIndex"/"jugador"/"playerID" por string y se los
    // reescribía. Andaba, pero se rompía en silencio si alguien renombraba un campo.
    // Ahora se fija un único PlayerIdentity y todos los scripts lo leen desde ahí en
    // su Start (ver PlayerIdentity.cs). Corre en Awake: todos los Awake pasan antes
    // que cualquier Start, así que cuando los scripts consultan, el valor ya está.
    void ForzarPlayerIndex()
    {
        PlayerIdentity id = GetComponentInChildren<PlayerIdentity>(true);

        // Si el trompo todavía no tiene el componente (escena sin migrar), se lo
        // agregamos acá: queda por encima del personaje en la jerarquía, así que
        // GetComponentInParent lo encuentra desde cualquier script de abajo.
        if (id == null) id = gameObject.AddComponent<PlayerIdentity>();

        id.PlayerIndex = playerIndexCPU;
    }

    // ESTRUCTURA NUEVA: los ataques ya no son scripts pegados al trompo sino prefabs que
    // un AttacksMenuPreset instancia cuando el menú se abre por primera vez. El humano lo
    // abre apretando un botón; la CPU no, así que sus ataques nunca existirían.
    //
    // Se resuelve creando, dentro del propio trompo de la CPU, dos menús invisibles (uno
    // de Ataques y otro de Especiales) que arman las opciones con los prefabs que el
    // PresetCPU dejó en PlayerSettings. No hay interfaz de por medio, y las opciones
    // quedan al alcance de GetComponentsInChildren<IAIAction>(). Van "controlados por la
    // IA": no leen joystick ni teclado.
    //
    // Si el trompo es de la estructura vieja (no tiene el componente Move nuevo) no se
    // hace nada: sigue usando los scripts de siempre.
    void CrearMenusDeAtaque()
    {
        if (GetComponentInChildren<Move>(true) == null) return;

        // Por si el cuerpo ya trae sus propios menús dentro de la interfaz (el trompo Base
        // los trae) y alguien los activa: que tampoco lean el teclado.
        foreach (AttacksMenuPreset menu in GetComponentsInChildren<AttacksMenuPreset>(true))
        {
            menu.playerIndex = playerIndexCPU;
            menu.controladoPorIA = true;
        }

        CrearMenu("OpcionesIA_Ataques", false);
        CrearMenu("OpcionesIA_Especiales", true);
    }

    void CrearMenu(string nombre, bool especial)
    {
        GameObject contenedor = new GameObject(nombre);
        contenedor.transform.SetParent(transform, false);
        contenedor.AddComponent<AttacksMenuPreset>().ConfigurarParaIA(playerIndexCPU, especial);
    }

    // Busca los componentes de combate del propio trompo. Busca en este objeto y sus
    // hijos; si no los encuentra (porque el AIBrain quedó por encima en la jerarquía),
    // busca también desde el objeto raíz del personaje hacia abajo. Reintentable.
    void AutocompletarReferencias()
    {
        if (check == null) check = BuscarEnTrompo<CheckPlayerTurn>();
        // Marcar el turno de la CPU como controlado por IA, para que su cooldown entre
        // turnos siga usando el tiempo escalado original (su embestida física conecta
        // igual que antes). El fix de tiempo real es solo para el humano. >>> FIX TURNOS IA <<<
        if (check != null) check.controladoPorIA = true;
        if (energyCounter == null) energyCounter = BuscarEnTrompo<EnergyCounter>();
        if (rb == null) rb = BuscarEnTrompo<Rigidbody2D>();
        if (vida == null) vida = BuscarEnTrompo<Vida>();
        if (attackAction == null) attackAction = BuscarEnTrompo<BasicAttack>();
        if (moveAction == null) moveAction = BuscarEnTrompo<MovementOption>();
        if (waitAction == null) waitAction = BuscarEnTrompo<WaitOption>();
        // Acciones básicas de la estructura nueva. Se buscan SOLO dentro de este personaje
        // (sin caer a transform.root): la CPU tiene otros personajes hermanos y no hay que
        // agarrarle el Move o el ataque a uno que no es el suyo. El ataque básico recién
        // existe cuando el menú de ataques arma sus opciones, así que se reintenta.
        if (attackOption == null) attackOption = GetComponentInChildren<BasicAttackOption>(true);
        if (moveOption == null) moveOption = GetComponentInChildren<Move>(true);
        if (waitOption == null) waitOption = GetComponentInChildren<Wait>(true);
        // Los especiales se buscan SOLO dentro del propio personaje (sin caer al hermano
        // por transform.root): si esta IA es un Magus, encuentra los del Magus y ninguno
        // del Ninja. Antes, el Magus agarraba los del Ninja hermano y al lanzarlos se
        // cerraba el turno del NINJA (no el del Magus) -> el Magus quedaba congelado en su
        // turno hasta el countdown. >>> FIX MAGUS CONGELADO <<<
        // Se reintenta también cuando el array vino VACÍO, no solo cuando es null: si el
        // personaje todavía no estaba armado en el primer intento, un "== null" a secas
        // dejaba la lista vacía para siempre y la IA no usaba especiales nunca.
        if (especiales == null || especiales.Length == 0)
            especiales = GetComponentsInChildren<IAIAction>(true);
        if (countDown == null) countDown = FindFirstObjectByType<CountDown>();
    }

    // Busca un componente primero en este objeto+hijos; si no, desde la raíz del
    // personaje hacia abajo (cubre el caso de que el AIBrain esté por encima).
    T BuscarEnTrompo<T>() where T : Component
    {
        T c = GetComponentInChildren<T>(true);
        if (c == null) c = transform.root.GetComponentInChildren<T>(true);
        return c;
    }

    // ¿Ya tiene todas las referencias esenciales para pelear?
    // Incluye los especiales: si el array quedó vacío, hay que seguir reintentando.
    // Sin esto, el reintento de Update no se disparaba nunca (las demás referencias ya
    // estaban) y la IA se quedaba sin especiales. >>> FIX IA SOLO ATACA <<<
    bool ReferenciasCompletas()
    {
        return check != null && energyCounter != null && rb != null
            && vida != null && BasicasListas()
            && especiales != null && especiales.Length > 0;
    }

    // ¿Tiene atacar, moverse y esperar? Sirven las de la estructura nueva o las viejas.
    // Sin esto la IA no puede hacer NADA en su turno.
    bool BasicasListas()
    {
        return (attackOption != null || attackAction != null)
            && (moveOption != null || moveAction != null)
            && (waitOption != null || waitAction != null);
    }

    // Busca al rival: otro objeto activo con tag "Player" en otra layer.
    // Solo cuenta objetos ACTIVOS (el personaje no elegido está desactivado).
    void BuscarRival()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p != gameObject && p.activeInHierarchy && p.layer != gameObject.layer)
            {
                enemy = p.transform;
                return;
            }
        }
    }

    // Ajusta el comportamiento de la IA según la dificultad elegida.
    // FÁCIL: ataca poco, se queda lejos, casi no usa especiales (y nunca los fuertes),
    //        se defiende temprano, reacciona lento.
    // NORMAL: equilibrado.
    // DIFÍCIL: ataca y se defiende intensamente, usa todos los especiales (incl. fuertes)
    //          siempre que puede, aguanta con poca vida, reacciona casi instantáneo.
    void AplicarDificultad()
    {
        switch (difficulty)
        {
            case AIDifficulty.Facil:
                specialChance = 0.15f;            // casi no usa especiales
                permitirEspecialesFuertes = false; // y nunca los fuertes
                attackRange = 2.0f;               // espera a que el rival se acerque
                lowLifeValue = 50f;               // se pone defensiva temprano
                demoraReaccion = 0.8f;            // reacciona lento (da tiempo al jugador)
                probabilidadDudar = 0.45f;        // duda seguido => ataca poco
                break;

            case AIDifficulty.Dificil:
                specialChance = 0.9f;             // usa especiales casi siempre
                permitirEspecialesFuertes = true; // incluidos los fuertes
                attackRange = 4.5f;               // busca pelea de lejos
                lowLifeValue = 12f;               // aguanta con muy poca vida
                demoraReaccion = 0.1f;            // reacciona casi al instante
                probabilidadDudar = 0f;           // nunca duda => ataca siempre
                break;

            default: // Normal
                specialChance = 0.5f;
                permitirEspecialesFuertes = true;
                attackRange = 3.0f;
                lowLifeValue = 30f;
                demoraReaccion = 0.4f;
                probabilidadDudar = 0.15f;
                break;
        }
    }

    void Start()
    {
        // La IA se carga vida llena sola (modo práctica: no usa la barra de potencia).
        if (vida != null) vida.SetFullLife();

        // Además, si existe una barra de potencia (TimingBar) emparejada por Vida,
        // la frena para que no quede oscilando en pantalla. (Opcional: si no hay barra,
        // no pasa nada — la vida ya se cargó arriba.)
        if (vida != null)
        {
            TimingBar[] barras = FindObjectsByType<TimingBar>(FindObjectsSortMode.None);
            foreach (TimingBar barra in barras)
            {
                if (barra.GetVida() == vida)
                {
                    barra.StopAsAI();
                    break;
                }
            }
        }
    }

    private float tiempoEnTurno = 0f; // cuánto lleva la IA en su turno actual (para la demora de reacción)
    // Los reintentos de enganche (referencias/rival) se hacen cada tanto, no cada frame:
    // BuscarRival() aloca un array con FindGameObjectsWithTag y no vale la pena pagarlo
    // 60 veces por segundo mientras el sistema de selección termina de activar todo.
    private float reintentoTimer = 0f;
    private const float intervaloReintento = 0.25f;
    // Red de seguridad: tiempo desde que la IA actuó esperando que el turno cierre.
    // Si no cierra (alguna acción cerró el turno de otro personaje, o algo falló), lo
    // forzamos tras este límite en vez de quedar congelados hasta el countdown (~10s).
    private float tiempoTrasActuar = 0f;
    private const float maxEsperaCierreTurno = 1.0f; // >>> FIX MAGUS CONGELADO <<<
    private const float maxEsperaBasicas = 1.5f;     // tope para esperar atacar/moverse/esperar
    private bool vidaInicializada = false;

    void Update()
    {
        // Reintentar enganchar referencias/rival si algo quedó null (los componentes
        // o el personaje rival pueden activarse tarde por el sistema de selección).
        if (!ReferenciasCompletas() || enemy == null)
        {
            reintentoTimer += Time.unscaledDeltaTime;
            if (reintentoTimer >= intervaloReintento)
            {
                reintentoTimer = 0f;
                if (!ReferenciasCompletas()) AutocompletarReferencias();
                if (enemy == null) BuscarRival();
            }
        }

        // Esperar a que termine la cuenta regresiva inicial ("3,2,1,GO!") antes de
        // que la IA haga NADA. Así la pelea no arranca antes de tiempo.
        if (countDown != null && !countDown.finished) return;

        // Vida llena: Start() ya lo hace, pero con el trompo "Base" la vida máxima la lee
        // Vida.Start() de PlayerSettings, y el orden entre los Start de dos scripts no está
        // garantizado. Si el de la IA corriera primero, SetFullLife() cargaría 0 de vida y
        // la CPU perdería en el primer frame. Se repite hasta que la vida máxima exista.
        if (!vidaInicializada && vida != null && vida.VidaMaxima > 0f)
        {
            vida.SetFullLife();
            vidaInicializada = true;
        }

        if (check != null && check.isTurnActive)
        {
            if (!hasActedThisTurn)
            {
                // Demora de reacción según dificultad: la Fácil "piensa" más lento.
                // Usamos unscaledDeltaTime porque durante el turno el tiempo está congelado.
                tiempoEnTurno += Time.unscaledDeltaTime;

                // Los ataques de la estructura nueva se arman un par de frames después de
                // que aparece el trompo: si el turno empieza antes, se espera a tenerlos
                // (con un tope, para no quedar colgada si algo faltara de verdad).
                bool puedeActuar = BasicasListas() || tiempoEnTurno >= maxEsperaBasicas;

                if (puedeActuar && tiempoEnTurno >= demoraReaccion)
                {
                    hasActedThisTurn = true;
                    tiempoTrasActuar = 0f;
                    currentState.Execute();
                }
            }
            else
            {
                // RED DE SEGURIDAD >>> FIX MAGUS CONGELADO <<<
                // La IA ya actuó este turno. Normalmente la acción cierra el turno en el
                // mismo frame; si por lo que sea NO se cerró, lo forzamos acá tras un
                // instante para que la IA no quede congelada esperando el countdown.
                tiempoTrasActuar += Time.unscaledDeltaTime;
                if (tiempoTrasActuar >= maxEsperaCierreTurno)
                {
                    tiempoTrasActuar = 0f;
                    check.PlayerChoseAnAction(0f, 0f, false); // cierra el turno de la IA
                }
            }
        }
        else
        {
            hasActedThisTurn = false;
            tiempoEnTurno = 0f;
            tiempoTrasActuar = 0f;

            // La IA pide su turno activamente cuando su trompo está lo bastante lento.
            // Así no depende de la ventana de sincronización (pensada para 2 humanos)
            // ni queda trabada cuando el tiempo está congelado (timeScale=0).
            //
            // NOTA: se intentó igualar esto a la regla del humano (usar maxVelocityTurn
            // en vez de aiReadyVelocity) para que la IA no atacara de más, y ROMPIÓ el
            // sistema de turnos. Se revirtió. El motivo está explicado en
            // ANALISIS-TURNOS-IA.md: maxVelocityTurn no es un umbral estable, cada
            // acción lo reescribe (BasicAttack lo deja en 1000 = "a cualquier
            // velocidad"), así que copiarlo acá hace lo contrario de lo buscado.
            // Si se vuelve a intentar, hay que probarlo en Unity con el juego corriendo.
            if (check != null && rb != null && rb.linearVelocity.magnitude < aiReadyVelocity)
            {
                check.RequestTurnNow();
            }
        }
    }

    public void SetState(AIState next)
    {
        currentState = next;
    }

    public string CurrentStateName => currentState != null ? currentState.Name : "-";

    // ======================== SENSORES ========================
    public float DistanceToEnemy()
    {
        if (enemy == null) return Mathf.Infinity;
        return Vector2.Distance(enemy.position, rb.position);
    }

    public bool HasEnergyToAttack() => energyCounter.currentEnergy >= attackEnergyCost;
    public bool HasEnergyToMove() => energyCounter.currentEnergy >= moveEnergyCost;
    public bool LifeIsLow() => vida != null && vida.vidaActual <= lowLifeValue;

    // ======================== ACCIONES ========================
    // Reusan los scripts del trompo, así que producen el mismo efecto (y daño)
    // que cuando los dispara el jugador.

    // Cada acción usa la de la estructura NUEVA si existe y, si no, la vieja.
    public void PerformAttack()
    {
        if (attackOption == null && attackAction == null) { Debug.LogError("La IA no tiene script Attack en su trompo."); return; }
        if (!HasEnergyToAttack()) { PerformWait(); return; }
        if (attackOption != null) attackOption.DoAttack();
        else attackAction.DoAttack();
    }

    public void PerformApproach()
    {
        if (moveOption == null && moveAction == null) { Debug.LogError("La IA no tiene script MovementOption en su trompo."); return; }
        if (!HasEnergyToMove()) { PerformWait(); return; }
        if (moveOption != null) moveOption.DoMove();
        else moveAction.DoMove();
    }

    public void PerformWait()
    {
        if (waitOption == null && waitAction == null) { Debug.LogError("La IA no tiene script WaitOption en su trompo."); return; }
        if (waitOption != null) waitOption.DoWait();
        else waitAction.DoWait();
    }

    // ALEJARSE de verdad. >>> FIX RETREAT <<<
    // Antes esto era literalmente PerformWait(): con la vida baja la IA se quedaba
    // esperando turno tras turno y parecía colgada. Ahora se mueve en dirección
    // CONTRARIA al rival. Hace falta el DoMove(direccion) de MovementOption porque
    // el prompter siempre apunta al enemigo: moverse "hacia donde apunta" era
    // justamente lo que NO queremos al retroceder.
    public void PerformRetreat()
    {
        // Sin script de movimiento, sin energía o sin rival, no queda otra que esperar.
        if ((moveOption == null && moveAction == null) || !HasEnergyToMove() || enemy == null || rb == null)
        {
            PerformWait();
            return;
        }

        Vector2 haciaElRival = (Vector2)enemy.position - rb.position;
        if (moveOption != null) moveOption.DoMove(-haciaElRival); // alejarse
        else moveAction.DoMove(-haciaElRival);
    }

    // Devuelve true si la IA "duda" este turno y prefiere esperar en vez de atacar.
    // En Fácil pasa seguido (ataca poco); en Difícil nunca.
    public bool DudaYEspera()
    {
        return Random.value < probabilidadDudar;
    }

    // Intenta lanzar un especial del personaje (cualquiera que implemente IAIAction).
    // Devuelve true si lanzó alguno; false si no (entonces se hace ataque normal).
    // Sin allocations: reusa la lista 'candidatos' en vez de crear una por ataque.
    public bool TryPerformSpecial()
    {
        // Tirar el dado: a veces NO usa especial, para variar.
        if (Random.value > specialChance) return false;
        if (especiales == null || especiales.Length == 0) return false;

        candidatos.Clear();
        for (int i = 0; i < especiales.Length; i++)
        {
            IAIAction accion = especiales[i];
            if (accion == null) continue;

            // OJO: acá NO va un chequeo de isActiveAndEnabled. >>> FIX IA SOLO ATACA <<<
            // Los especiales viven en los objetos del menú radial ("Atacar", "Especial"),
            // que están INACTIVOS salvo mientras un humano mantiene apretado el botón de
            // menú. Como la IA nunca lo mantiene, un filtro por activo los descartaba a
            // todos y la CPU quedaba haciendo solo embestidas.
            // Llamar un método público sobre un componente desactivado funciona igual:
            // por eso PerformAttack() —que no filtra— siempre anduvo, aunque BasicAttack
            // vive en el MISMO GameObject que los especiales.
            // No hace falta filtrar por personaje: 'especiales' se busca solo dentro del
            // personaje de esta IA (ver AutocompletarReferencias). >>> FIX MAGUS CONGELADO <<<

            // Los especiales FUERTES solo entran si la dificultad los permite (Difícil).
            if (accion.IsStrong && !permitirEspecialesFuertes) continue;

            if (!accion.CanExecute()) continue;

            candidatos.Add(accion);
        }

        if (candidatos.Count == 0) return false; // no hay especiales disponibles

        // Elegir uno al azar y ejecutarlo.
        candidatos[Random.Range(0, candidatos.Count)].Execute();
        return true;
    }
}
