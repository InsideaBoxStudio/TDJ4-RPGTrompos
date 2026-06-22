using UnityEngine;

// ============================================================================
//  MÁQUINA DE ESTADOS FINITOS (FSM) PARA LA IA DEL TROMPO
//  ----------------------------------------------------------------------------
//  Patrón de diseño: STATE (Estado).
//   - La IA siempre está en UN estado (Acercarse, Atacar, Alejarse, Defender).
//   - Cuando es su turno, el estado actual DECIDE una acción y a qué estado ir.
//
//  La IA ejecuta sus acciones llamando a los MISMOS scripts que usa el jugador
//  (Attack, MovementOption, WaitOption). Por eso su ataque hace daño exactamente
//  igual que el del jugador.
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

    [Header("Especiales del Ninja (se autocompletan si el trompo los tiene)")]
    [SerializeField] private LaunchShuriken shurikenAction;
    [SerializeField] private LaunchPinchos pinchosAction;
    [SerializeField] private LaunchClone cloneAction;

    [Header("¿Cuáles especiales son FUERTES? (solo la dificultad Difícil los usa siempre)")]
    [SerializeField] private bool shurikenEsFuerte = false; // shuriken = básico por defecto
    [SerializeField] private bool pinchosEsFuerte = true;   // pinchos = fuerte
    [SerializeField] private bool cloneEsFuerte = true;     // clon = fuerte

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

    // Fuerza el índice de jugador (playerIndex/jugador/playerID) en TODOS los scripts
    // del trompo de la CPU. Permite copiar el trompo de Jugador1 (índice 0) y que la
    // IA lo convierta a jugador 2 sin tocar nada a mano en el editor.
    void ForzarPlayerIndex()
    {
        var comps = GetComponentsInChildren<MonoBehaviour>(true);
        string[] campos = { "playerIndex", "jugador", "playerID", "playerIndexForKeyboard" };
        foreach (var comp in comps)
        {
            if (comp == null || comp == this) continue;
            var tipo = comp.GetType();
            foreach (var nombre in campos)
            {
                var f = tipo.GetField(nombre,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (f != null && f.FieldType == typeof(int))
                    f.SetValue(comp, playerIndexCPU);
            }
        }
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
        if (shurikenAction == null) shurikenAction = BuscarEnTrompo<LaunchShuriken>();
        if (pinchosAction == null) pinchosAction = BuscarEnTrompo<LaunchPinchos>();
        if (cloneAction == null) cloneAction = BuscarEnTrompo<LaunchClone>();
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
    bool ReferenciasCompletas()
    {
        return check != null && energyCounter != null && rb != null
            && vida != null && attackAction != null && moveAction != null && waitAction != null;
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
        Debug.Log("[IA] Dificultad: " + difficulty + " | specialChance=" + specialChance +
                  " | attackRange=" + attackRange + " | lowLife=" + lowLifeValue);
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

        // Mensaje de confirmación: si ves esto en consola, el AIBrain está bien puesto.
        Debug.Log("=== AIBrain ACTIVO en '" + gameObject.name + "' ===  check=" + (check != null) +
                  " | attack=" + (attackAction != null) + " | move=" + (moveAction != null) +
                  " | wait=" + (waitAction != null) + " | enemy=" + (enemy != null) +
                  " | vida=" + (vida != null));
    }

    private float monitorTimer = 0f;
    private float tiempoEnTurno = 0f; // cuánto lleva la IA en su turno actual (para la demora de reacción)

    void Update()
    {
        // ---- MONITOR DE DIAGNÓSTICO: reporta cada 1 seg qué está pasando ----
        monitorTimer += Time.unscaledDeltaTime;
        if (monitorTimer >= 1f)
        {
            monitorTimer = 0f;
            float vel = rb != null ? rb.linearVelocity.magnitude : -1f;
            bool turno = check != null && check.isTurnActive;
            int ener = energyCounter != null ? energyCounter.currentEnergy : -1;
            Debug.Log("[MONITOR IA] velocidad=" + vel.ToString("F2") +
                      " | turnoActivo=" + turno +
                      " | energia=" + ener +
                      " | timeScale=" + Time.timeScale);
        }
        // ---------------------------------------------------------------

        // Reintentar enganchar referencias/rival si algo quedó null (los componentes
        // o el personaje rival pueden activarse tarde por el sistema de selección).
        if (!ReferenciasCompletas()) AutocompletarReferencias();
        if (enemy == null) BuscarRival();

        // Esperar a que termine la cuenta regresiva inicial ("3,2,1,GO!") antes de
        // que la IA haga NADA. Así la pelea no arranca antes de tiempo.
        if (countDown != null && !countDown.finished) return;

        if (check != null && check.isTurnActive)
        {
            if (!hasActedThisTurn)
            {
                // Demora de reacción según dificultad: la Fácil "piensa" más lento.
                // Usamos unscaledDeltaTime porque durante el turno el tiempo está congelado.
                tiempoEnTurno += Time.unscaledDeltaTime;
                if (tiempoEnTurno >= demoraReaccion)
                {
                    hasActedThisTurn = true;
                    currentState.Execute();
                }
            }
        }
        else
        {
            hasActedThisTurn = false;
            tiempoEnTurno = 0f;

            // La IA pide su turno activamente cuando su trompo está lo bastante lento.
            // Así no depende de la ventana de sincronización (pensada para 2 humanos)
            // ni queda trabada cuando el tiempo está congelado (timeScale=0).
            if (check != null && rb != null && rb.linearVelocity.magnitude < aiReadyVelocity)
            {
                check.RequestTurnNow();
            }
        }
    }

    public void SetState(AIState next)
    {
        currentState = next;
        Debug.Log("IA cambia a estado: " + next.Name);
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

    public void PerformAttack()
    {
        if (attackAction == null) { Debug.LogError("La IA no tiene script Attack en su trompo."); return; }
        if (!HasEnergyToAttack()) { PerformWait(); return; }
        Debug.Log("IA -> ATACA");
        attackAction.DoAttack();
    }

    public void PerformApproach()
    {
        if (moveAction == null) { Debug.LogError("La IA no tiene script MovementOption en su trompo."); return; }
        if (!HasEnergyToMove()) { PerformWait(); return; }
        Debug.Log("IA -> avanza");
        moveAction.DoMove();
    }

    public void PerformWait()
    {
        if (waitAction == null) { Debug.LogError("La IA no tiene script WaitOption en su trompo."); return; }
        Debug.Log("IA -> espera/defiende");
        waitAction.DoWait();
    }

    public void PerformRetreat()
    {
        // El alejarse real (retroceder) se afinará al implementar las dificultades.
        // Por ahora se comporta como defensa: frena y recupera.
        PerformWait();
    }

    // Devuelve true si la IA "duda" este turno y prefiere esperar en vez de atacar.
    // En Fácil pasa seguido (ataca poco); en Difícil nunca.
    public bool DudaYEspera()
    {
        return Random.value < probabilidadDudar;
    }

    // Intenta lanzar un especial del Ninja (shuriken / pinchos / clon).
    // Devuelve true si lanzó alguno; false si no (entonces se hace ataque normal).
    public bool TryPerformSpecial()
    {
        // Tirar el dado: a veces NO usa especial, para variar.
        if (Random.value > specialChance) return false;

        // Armar la lista de especiales disponibles (que existan y tengan energía).
        // Se mezcla el orden para que no use siempre el mismo.
        System.Collections.Generic.List<System.Action> opciones =
            new System.Collections.Generic.List<System.Action>();

        // Un especial entra en la lista solo si: existe, hay energía, y —si es FUERTE—
        // la dificultad actual permite usar especiales fuertes (solo Difícil).
        if (shurikenAction != null && energyCounter.currentEnergy >= shurikenAction.EnergyCost
            && (!shurikenEsFuerte || permitirEspecialesFuertes))
            opciones.Add(() => { Debug.Log("IA -> SHURIKEN"); shurikenAction.DoLaunch(); });

        if (pinchosAction != null && energyCounter.currentEnergy >= pinchosAction.EnergyCost
            && (!pinchosEsFuerte || permitirEspecialesFuertes))
            opciones.Add(() => { Debug.Log("IA -> PINCHOS"); pinchosAction.DoLaunch(); });

        if (cloneAction != null && energyCounter.currentEnergy >= cloneAction.EnergyCost
            && (!cloneEsFuerte || permitirEspecialesFuertes))
            opciones.Add(() => { Debug.Log("IA -> CLON"); cloneAction.DoLaunch(); });

        if (opciones.Count == 0) return false; // no hay especiales disponibles

        // Elegir uno al azar y ejecutarlo.
        int i = Random.Range(0, opciones.Count);
        opciones[i].Invoke();
        return true;
    }
}
