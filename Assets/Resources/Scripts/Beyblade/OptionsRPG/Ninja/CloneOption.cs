using UnityEngine;
using UnityEngine.InputSystem;

public class CloneOption : MonoBehaviour
{
    [SerializeField] private int childIndex = 0;
    [SerializeField] private GameObject ClonePrefab;
    [SerializeField] private Transform Prompter;
    [SerializeField] private Transform pointerPosition;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;

    [Header("Stats")]
    [SerializeField] private int energyCost = 2;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float Recoil = 10f;

    private AttacksMenuPreset attacksMenuPreset;

    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavia no lo tiene, queda el valor serializado de siempre.

    private GameObject playerObject;
    
    private void Start()
    {
        childIndex = transform.GetSiblingIndex();
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
        FindReferences();
    }

    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive;
        
        if (attacksMenuPreset.buttonState == childIndex) // >>> TECLADO <<<
        {
            DoAttack();
        }
    }

    // Llamable por el jugador (teclado/joystick) Y por la IA (AIBrain).
    public void DoAttack()
    {
        if (energyCounter.currentEnergy < energyCost) return;

        energyCounter.ChangeEnergy(-energyCost, "LaunchClone");
        rpgTurn.PlayerChoseAnAction(moveDuration, 3f, false);

        // Dirección hacia donde apunta el prompter
        Vector2 dir = Prompter.right.normalized;

        // Dirección perpendicular (90 grados)
        Vector2 perpendicular = new Vector2(-dir.y, dir.x);

        // Recoil del jugador
        rb.linearVelocity = -perpendicular * Recoil;

        // Crear clon
        GameObject clone = Instantiate(
            ClonePrefab,
            pointerPosition.position,
            Quaternion.identity
        );

        // Recoil del clon en dirección opuesta
        Rigidbody2D cloneRb = clone.GetComponent<Rigidbody2D>();

        if (cloneRb != null)
        {
            cloneRb.linearVelocity = perpendicular * Recoil;
        }
    }
    private void FindReferences()
    {
        playerObject = PlayerSettings.player[playerIndex];
        DefPlayerID defPlayerID = playerObject.GetComponent<DefPlayerID>();
        attacksMenuPreset = transform.parent.GetComponent<AttacksMenuPreset>();
        
        if (energyCounter == null)
        {
            energyCounter = defPlayerID.energyCounter;
        }
        if (pointerPosition == null)
        {
            pointerPosition = defPlayerID.pointerPosition;
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