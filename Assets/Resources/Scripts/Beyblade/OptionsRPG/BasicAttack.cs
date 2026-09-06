using UnityEngine;
using UnityEngine.InputSystem;
public class BasicAttack : MonoBehaviour
{
    [SerializeField] private GameObject ColliderAttack;
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
        if (!rpgTurn.isTurnActive) return; // actualizar estado del turno
        
        if ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.up.wasPressedThisFrame)
            || TeclasJugador.Atacar(playerIndex)) // >>> TECLADO <<<
        {
            DoAttack();
        }
    }

    // Llamable por el jugador (teclado/joystick) Y por la IA (AIBrain).
    public void DoAttack()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "BasicAttack");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, true);

        rb.linearVelocity = Prompter.right * moveSpeed;

        ColliderAttack.SetActive(true);
        // Rotar el collider hacia la direccion del ataque
        ColliderAttack.transform.rotation = Quaternion.LookRotation(Vector3.forward, Prompter.right);
        Invoke("EndAttack", moveDuration);
    }

    private void EndAttack()
    {
        ColliderAttack.SetActive(false);
    }
}