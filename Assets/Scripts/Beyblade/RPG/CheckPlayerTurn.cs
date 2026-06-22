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
    [SerializeField] public float minTurnTime = 1f; // tiempo minimo antes del siguiente turno
    [SerializeField] private float maxVelocityTurn = 1f; // velocidad maxima en la que el jugador debe estar para comenzar su turno
    private float maxVelTurn;
    [SerializeField] private CountDownRPG countDownRPG;
    [SerializeField] private int energyGainPerTurn = 1;

    private bool isTurnPosible = true;
    public bool isTurnActive = false;
    public float turnTime = 0f;

    // >>> FIX TURNOS IA <<<
    // En el combate contra la IA, la IA congela el juego (Time.timeScale = 0) cada vez
    // que toma su turno y actúa en tiempo REAL (unscaledDeltaTime). El cooldown entre
    // turnos del jugador usaba Invoke(), que corre en tiempo ESCALADO y queda congelado
    // mientras timeScale = 0 -> el jugador casi no podía volver a empezar su turno
    // ("la IA no te deja atacar"). Solución: en modo IA el cooldown corre en tiempo real.
    // En 1VS1 (humano vs humano) NO hay AIBrain -> modoIA = false -> Invoke original intacto.
    private bool modoIA = false;
    private Coroutine nextTurnCoroutine;

    void Awake()
    {
        maxVelTurn = maxVelocityTurn;
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
        if (!isTurnPosible) return;
        if (countDownRPG.countDownTime <= 0){
            if (isTurnActive)
            {
                PlayerChoseAnAction(0f, 0f, false);
            }
            return;
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
        turnTime = nextTurnTime;

        if (isAdditionalTime)
            nextTurnTime += nextTurnTime;
        else if (nextTurnTime <= 0f)
            nextTurnTime = minTurnTime;

        if (maxVelocityNextTurn <= 0f)
            maxVelocityNextTurn = maxVelTurn;

        maxVelocityTurn = maxVelocityNextTurn;

        CancelInvoke("NextTurnTime");
        // >>> FIX TURNOS IA <<<
        if (modoIA)
        {
            // Cooldown en tiempo REAL: se rehabilita aunque la IA tenga el juego
            // congelado (timeScale = 0). Así el jugador puede volver a tomar turno.
            if (nextTurnCoroutine != null) StopCoroutine(nextTurnCoroutine);
            nextTurnCoroutine = StartCoroutine(NextTurnTimeRealtime(nextTurnTime));
        }
        else
        {
            Invoke("NextTurnTime", nextTurnTime); // 1VS1: comportamiento original intacto
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
    }

    // >>> FIX TURNOS IA <<<
    // Versión del cooldown en tiempo REAL (no escalado). WaitForSecondsRealtime sigue
    // contando aunque Time.timeScale = 0 (mientras la IA tiene el juego congelado).
    private IEnumerator NextTurnTimeRealtime(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        isTurnPosible = true;
    }
}