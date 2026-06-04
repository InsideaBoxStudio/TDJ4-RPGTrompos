using UnityEngine;
using System.Collections;

public class CheckPlayerTurn : MonoBehaviour
{
    // titulo para SerializeField
    [Header("Player")]
    [SerializeField] private Rigidbody2D playersRb;
    [SerializeField] private GameObject playersOptions;
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

    void Awake()
    {
        maxVelTurn = maxVelocityTurn;
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
        energyCounter.ChangeEnergy(energyGainPerTurn);
        //Debug.Log("Turno del jugador" + gameObject.name);
        isTurnActive = true;
        playersOptions.SetActive(true);
        countDownRPG.isCountingDown = true;
    }

    public void PlayerChoseAnAction(float nextTurnTime, float maxVelocityNextTurn, bool isAdditionalTime) // Cuando ya eligio una accion
    {
        if (isAdditionalTime) nextTurnTime += nextTurnTime;
        else if (nextTurnTime <= 0f) nextTurnTime = minTurnTime;
        if (maxVelocityNextTurn <= 0f) maxVelocityNextTurn = maxVelTurn;
        maxVelocityTurn = maxVelocityNextTurn;
        CancelInvoke("NextTurnTime");
        Invoke("NextTurnTime", nextTurnTime); // Esperar antes de poder hacer otra accion
        isTurnPosible = false;
        isTurnActive = false;
        countDownRPG.ResetCountDown();
        playersOptions.SetActive(false);
        rpgTurn.PlayerFinished(this);

        //Debug.Log(nextTurnTime);
    }

    void NextTurnTime()
    {
        isTurnPosible = true;
    }
}