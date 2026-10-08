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


    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavia no lo tiene, queda el valor serializado de siempre.
    private void Start()
    {
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
    }

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
    // Mueve hacia donde apunta el prompter.
    public void DoMove()
    {
        DoMove(Prompter.right);
    }

    // Mismo movimiento pero en una dirección cualquiera. Lo usa la IA para poder
    // ALEJARSE del rival: el prompter siempre apunta al enemigo, así que sin esto
    // la IA solo sabía avanzar hacia él y "retroceder" terminaba siendo esperar.
    public void DoMove(Vector2 direccion)
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente

        // Sin dirección válida, moverse hacia donde apunta (comportamiento de siempre).
        if (direccion.sqrMagnitude < 0.0001f) direccion = Prompter.right;

        energyCounter.ChangeEnergy(-energyCost, "Move");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        rb.linearVelocity = direccion.normalized * moveSpeed;
    }
}
