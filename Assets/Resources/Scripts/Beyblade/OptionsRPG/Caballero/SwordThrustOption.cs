using UnityEngine;
using UnityEngine.InputSystem;

public class SwordThrustOption : MonoBehaviour, IAIAction
{
    [SerializeField] private int childIndex = 0;
    [SerializeField] private GameObject SwordThrustPrefab;
    [SerializeField] private GameObject ColliderAttack;
    [SerializeField] private Transform Prompter;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private bool destroyAttack = false;

    [Header("Stats")]
    [SerializeField] private int energyCost = 1; //costo de energia
    [SerializeField] private float moveSpeed = 5f; //velocidad de movimiento
    [SerializeField] private float moveDuration = 1f;

    // La estocada es el especial BÁSICO del Caballero: la IA la usa en toda dificultad.
    [SerializeField] private bool esEspecialFuerte = false;

    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavia no lo tiene, queda el valor serializado de siempre.
    private AttacksMenuPreset attacksMenuPreset;
    private GameObject playerObject;

    private void Start()
    {
        childIndex = transform.GetSiblingIndex();
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
        FindReferences();
    }

    void Update()
    {
        if (!rpgTurn.isTurnActive) return; // actualizar estado del turno
        
        if (attacksMenuPreset.buttonState == childIndex) // >>> TECLADO <<<
        {
            DoAttack();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => energyCounter != null && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoAttack();

    // Llamable por el jugador (teclado/joystick) Y por la IA (AIBrain).
    public void DoAttack()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "SwordThrust");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        rb.linearVelocity = Prompter.right * moveSpeed;

        if (ColliderAttack == null)
        {
            ColliderAttack = Instantiate(SwordThrustPrefab, playerObject.transform);
        }
        else
        {
            ColliderAttack.SetActive(true);
            // Rotar el collider hacia la direccion del ataque
        }
        // Rotar el collider hacia la direccion del ataque
        ColliderAttack.transform.rotation = Quaternion.LookRotation(Vector3.forward, Prompter.right);
        Invoke("EndAttack", moveDuration);
    }

    private void EndAttack()
    {
        if (destroyAttack)
        {
            Destroy(ColliderAttack);
        }
        ColliderAttack.SetActive(false);
    }

    private void FindReferences()
    {
        attacksMenuPreset = transform.parent.GetComponent<AttacksMenuPreset>();
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