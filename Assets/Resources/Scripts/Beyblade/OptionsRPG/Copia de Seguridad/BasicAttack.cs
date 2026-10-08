using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class BasicAttack : MonoBehaviour
{
    [SerializeField] private GameObject BasicAttackPrefab;
    [SerializeField] private GameObject ColliderAttack;
    [SerializeField] private Transform Prompter;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;
    [SerializeField] private bool destroyAttack = false;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;
    
    [Header("Stats")]
    [SerializeField] private int energyCost = 1; //costo de energia
    [SerializeField] private float moveSpeed = 5f; //velocidad de movimiento
    [SerializeField] private float moveDuration = 1f;

    private GameObject playerObject;


    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavia no lo tiene, queda el valor serializado de siempre.
    private void Start()
    {
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
        FindReferences();
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

        if (ColliderAttack == null)
        {
            ColliderAttack = Instantiate(BasicAttackPrefab, playerObject.transform);
        }
        else
        {
            ColliderAttack.SetActive(true);
            // Rotar el collider hacia la direccion del ataque
        }
        ColliderAttack.transform.rotation = Quaternion.LookRotation(Vector3.forward, Prompter.right);
        Invoke("EndAttack", moveDuration);
    }

    private void EndAttack()
    {
        ColliderAttack.SetActive(false);
        if (destroyAttack)
        {
            Destroy(ColliderAttack);
        }
    }

    private void FindReferences()
    {
        playerObject = PlayerSettings.player[playerIndex];
        DefPlayerID defPlayerID = playerObject.GetComponent<DefPlayerID>();
        
        if (energyCounter == null)
        {
            energyCounter = defPlayerID.energyCounter;
        }
        if (Prompter == null)
        {
            Prompter = defPlayerID.PointerArrow;
        }
        if (rpgTurn == null)
        {
            rpgTurn = playerObject.GetComponent<CheckPlayerTurn>();
        }
        if (rb == null)
        {
            rb = playerObject.GetComponent<Rigidbody2D>();
        }
    }
}