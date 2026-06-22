using UnityEngine;
using UnityEngine.InputSystem;

public class MovementOption : MonoBehaviour
{
    [SerializeField] private Transform Prompter;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;

    
    [Header("Stats")]
    [SerializeField] private int energyCost = 1; //costo de energia
    [SerializeField] private float moveSpeed = 5f; //velocidad de movimiento
    [SerializeField] private float moveDuration = 1f;


    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno
        
        if (turnActive && ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].buttonNorth.wasPressedThisFrame)
            || TeclasJugador.Avanzar(playerIndex))) // >>> TECLADO <<<
        {
            DoMove();
        }
    }

    // Llamable por el jugador (teclado/joystick) Y por la IA (AIBrain).
    public void DoMove()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "Move");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        rb.linearVelocity = Prompter.right * moveSpeed;
    }
}
