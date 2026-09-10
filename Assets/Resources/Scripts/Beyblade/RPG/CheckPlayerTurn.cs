using UnityEngine;
using System.Collections;

public class CheckPlayerTurn : MonoBehaviour
{
    // titulo para SerializeField
    [Header("Player")]
    [SerializeField] private Rigidbody2D playersRb;
    [SerializeField] public GameObject playersOptions;
    [SerializeField] private EnergyCounter energyCounter;
    [SerializeField] private RPGTurn rpgTurn;

    [Header("")]

    [Header("Turn Settings")]
    [SerializeField] private float maxVelocityTurn = 1f; // velocidad maxima en la que el jugador debe estar para comenzar su turno
    private float maxVelTurn;
    [SerializeField] private CountDownRPG countDownRPG;
    [SerializeField] private int energyGainPerTurn = 1;

    private bool isTurnPosible = true;
    public bool isTurnActive = false;
    public float turnTime = 0f;

    [SerializeField] public float timeUntilNextTurn = 0f;

    // >>> FIX TURNOS IA <<<
    // En el combate contra la IA, la IA congela el juego (Time.timeScale = 0) cada vez
    // que toma su turno y actúa en tiempo REAL (unscaledDeltaTime). El cooldown entre
    // turnos del JUGADOR HUMANO usaba Invoke(), que corre en tiempo ESCALADO y queda
    // congelado mientras timeScale = 0 -> el humano casi no podía volver a empezar su
    // turno ("la IA no te deja atacar"). Solución: SOLO el cooldown del humano corre en
    // tiempo real. La IA queda EXACTAMENTE como estaba (Invoke escalado), así su embestida
    // física conecta igual que antes. En 1VS1 (sin AIBrain) -> modoIA = false -> todo Invoke.
    private bool modoIA = false;                  // hay una IA en la escena (Practica)
    public bool controladoPorIA = false;          // este turno pertenece a la CPU (lo marca el AIBrain)
    private Coroutine nextTurnCoroutine;

    void Awake()
    {
        PlayerChoseAnAction(2f, 1000f, false);
        Time.timeScale = 0f;
        maxVelTurn = maxVelocityTurn;

        // Un prefab NO puede guardar referencias a objetos de la escena: al colocar
        // una instancia, estos dos campos vienen vacíos. Como hay uno solo de cada
        // uno por escena, los buscamos acá. Así el trompo puede ser un prefab sin
        // que haya que recablear nada a mano en cada instancia.
        // Si están asignados en el Inspector (escenas viejas), se respetan.
        if (rpgTurn == null) rpgTurn = FindFirstObjectByType<RPGTurn>();
        if (countDownRPG == null) countDownRPG = FindFirstObjectByType<CountDownRPG>();
    }

    void Start()
    {
        // Detectar si estamos en combate contra la IA (escena Practica tiene AIBrain).
        // Incluye objetos inactivos por si el personaje de la CPU se activa más tarde
        // (sistema de selección de personajes).
        modoIA = FindFirstObjectByType<AIBrain>(FindObjectsInactive.Include) != null;
    }

    void Update()
    {
        if (!isTurnPosible)
        {
            if (modoIA && !controladoPorIA)
                timeUntilNextTurn -= Time.deltaTime;
            else
                timeUntilNextTurn -= Time.deltaTime;
            return;
        }

        if (countDownRPG.countDownTime <= 0)
        {
            if (isTurnActive)
            {
                PlayerChoseAnAction(0f, 0f, false);
            }
            return;
        }

        if (isTurnActive)
        {
            Time.timeScale = 0f;
        }

        bool playerReady = playersRb.linearVelocity.magnitude < maxVelocityTurn;

        if (playerReady && !isTurnActive)
        {
            rpgTurn.NotifyReady(this);
        }
    }

    // Llamado por la IA (AIBrain) para pedir su turno activamente, sin depender de
    // la ventana de sincronización pensada para dos jugadores humanos.
    public void RequestTurnNow()
    {
        if (!isTurnPosible) return;
        if (isTurnActive) return;
        if (countDownRPG.countDownTime <= 0) return;
        rpgTurn.NotifyReadyImmediate(this); // versión sin corrutina (la IA no usa la ventana de sync)
    }

    public void Ready()
    {
        energyCounter.ChangeEnergy(energyGainPerTurn, "GainEnergy");
        //Debug.Log("Turno del jugador" + gameObject.name);
        isTurnActive = true;
        playersOptions.SetActive(true);
        countDownRPG.isCountingDown = true;
    }

    public void PlayerChoseAnAction(float nextTurnTime, float maxVelocityNextTurn, bool isAdditionalTime)
    {

        Debug.Log("timeUntilNextTurn: " + timeUntilNextTurn);
        maxVelocityNextTurn = 1000;
        turnTime = timeUntilNextTurn;
        turnTime += nextTurnTime;

        /*
        if (isAdditionalTime)
            turnTime += nextTurnTime;
        else if (nextTurnTime <= 0f)
            turnTime = nextTurnTime;
        */
        
        if (maxVelocityNextTurn <= 0f)
            maxVelocityNextTurn = maxVelTurn;

        timeUntilNextTurn = turnTime;

        maxVelocityTurn = maxVelocityNextTurn;

        CancelInvoke("NextTurnTime");
        // >>> FIX TURNOS IA <<<
        if (modoIA && !controladoPorIA)
        {
            // SOLO el humano en modo IA: cooldown en tiempo REAL, se rehabilita aunque la
            // IA tenga el juego congelado (timeScale = 0). Así el humano puede volver a
            // tomar turno y atacar.
            Invoke("NextTurnTime", turnTime);
            
            // if (nextTurnCoroutine != null) StopCoroutine(nextTurnCoroutine);
            // nextTurnCoroutine = StartCoroutine(NextTurnTimeRealtime(turnTime));
        }
        else
        {
            // IA (su embestida física necesita tiempo escalado) y 1VS1: Invoke original.
            Invoke("NextTurnTime", nextTurnTime);
        }

        isTurnPosible = false;
        isTurnActive = false;

        playersOptions.SetActive(false);

        bool everyoneFinished = rpgTurn.PlayerFinished(this);

        // Sólo el último jugador resetea el contador
        if (everyoneFinished)
        {
            countDownRPG.ResetCountDown();
        }
    }

    void NextTurnTime()
    {
        isTurnPosible = true;
        timeUntilNextTurn = 0f;
        // Debug.Log("isTurnPosible: " + isTurnPosible);
    }

    // >>> FIX TURNOS IA <<<
    // Versión del cooldown en tiempo REAL (no escalado). WaitForSecondsRealtime sigue
    // contando aunque Time.timeScale = 0 (mientras la IA tiene el juego congelado).
    private IEnumerator NextTurnTimeRealtime(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);

        isTurnPosible = true;
        timeUntilNextTurn = 0f;
    }
}