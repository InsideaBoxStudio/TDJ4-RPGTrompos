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
    [SerializeField] private Attack attackAction;
    [SerializeField] private MovementOption moveAction;
    [SerializeField] private WaitOption waitAction;

    [Header("Especiales del Ninja (se autocompletan si el trompo los tiene)")]
    [SerializeField] private LaunchShuriken shurikenAction;
    [SerializeField] private LaunchPinchos pinchosAction;
    [SerializeField] private LaunchClone cloneAction;

    [Header("Dificultad")]
    [SerializeField] public AIDifficulty difficulty = AIDifficulty.Normal;

    [Header("Parámetros de decisión")]
    [SerializeField] public float attackRange = 3f;   // a qué distancia decide atacar
    [SerializeField] public float lowLifeValue = 30f; // por debajo de esto se pone defensiva
    [SerializeField] public int attackEnergyCost = 1;
    [SerializeField] public int moveEnergyCost = 1;
    [SerializeField] public float aiReadyVelocity = 1.5f; // umbral de velocidad para pedir turno (mayor que el 1 del juego)
    [Range(0f, 1f)]
    [SerializeField] public float specialChance = 0.5f; // probabilidad de usar un especial al atacar

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

        // Autocompletar referencias buscando DENTRO del personaje donde está el AIBrain
        // (este objeto + sus hijos). IMPORTANTE: poné el AIBrain en el personaje ACTIVO
        // (ej: Ninja), NO en el padre "Jugadores2", para no agarrar por error los scripts
        // del otro personaje desactivado (ej: Mago).
        if (check == null) check = GetComponentInChildren<CheckPlayerTurn>(true);
        if (energyCounter == null) energyCounter = GetComponentInChildren<EnergyCounter>(true);
        if (rb == null) rb = GetComponentInChildren<Rigidbody2D>(true);
        if (vida == null) vida = GetComponentInChildren<Vida>(true);
        if (attackAction == null) attackAction = GetComponentInChildren<Attack>(true);
        if (moveAction == null) moveAction = GetComponentInChildren<MovementOption>(true);
        if (waitAction == null) waitAction = GetComponentInChildren<WaitOption>(true);
        if (shurikenAction == null) shurikenAction = GetComponentInChildren<LaunchShuriken>(true);
        if (pinchosAction == null) pinchosAction = GetComponentInChildren<LaunchPinchos>(true);
        if (cloneAction == null) cloneAction = GetComponentInChildren<LaunchClone>(true);
        if (countDown == null) countDown = FindObjectOfType<CountDown>(); // cuenta regresiva inicial de la escena

        // Buscar al rival: otro objeto con tag "Player" en otra layer.
        if (enemy == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                if (p != gameObject && p.layer != gameObject.layer)
                {
                    enemy = p.transform;
                    break;
                }
            }
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

        // Mensaje de confirmación: si ves esto en consola, el AIBrain está bien puesto.
        Debug.Log("=== AIBrain ACTIVO en '" + gameObject.name + "' ===  check=" + (check != null) +
                  " | attack=" + (attackAction != null) + " | move=" + (moveAction != null) +
                  " | wait=" + (waitAction != null) + " | enemy=" + (enemy != null) +
                  " | vida=" + (vida != null));
    }

    private float monitorTimer = 0f;

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

        // Esperar a que termine la cuenta regresiva inicial ("3,2,1,GO!") antes de
        // que la IA haga NADA. Así la pelea no arranca antes de tiempo.
        if (countDown != null && !countDown.finished) return;

        if (check != null && check.isTurnActive)
        {
            if (!hasActedThisTurn)
            {
                hasActedThisTurn = true;
                currentState.Execute();
            }
        }
        else
        {
            hasActedThisTurn = false;

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

        if (shurikenAction != null && energyCounter.currentEnergy >= shurikenAction.EnergyCost)
            opciones.Add(() => { Debug.Log("IA -> SHURIKEN"); shurikenAction.DoLaunch(); });

        if (pinchosAction != null && energyCounter.currentEnergy >= pinchosAction.EnergyCost)
            opciones.Add(() => { Debug.Log("IA -> PINCHOS"); pinchosAction.DoLaunch(); });

        if (cloneAction != null && energyCounter.currentEnergy >= cloneAction.EnergyCost)
            opciones.Add(() => { Debug.Log("IA -> CLON"); cloneAction.DoLaunch(); });

        if (opciones.Count == 0) return false; // no hay especiales disponibles

        // Elegir uno al azar y ejecutarlo.
        int i = Random.Range(0, opciones.Count);
        opciones[i].Invoke();
        return true;
    }
}
