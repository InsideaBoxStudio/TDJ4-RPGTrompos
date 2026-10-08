using UnityEngine;
using UnityEngine.InputSystem;

public class ShurikenOption : MonoBehaviour
{
    [SerializeField] private int childIndex = 0;
    [SerializeField] private GameObject ShurikenPrefab;
    [SerializeField] private Transform Prompter;
    [SerializeField] private Transform pointerPosition;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;


    [Header("Stats")]
    [SerializeField] private int energyCost = 2; //costo de energia
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float Recoil = 1f;

    private GameObject playerObject;
    private AttacksMenuPreset attacksMenuPreset;

    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavia no lo tiene, queda el valor serializado de siempre.
    private void Start()
    {
        childIndex = transform.GetSiblingIndex();
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
        FindReferences();
    }

    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno
        
        if (attacksMenuPreset.buttonState == childIndex) // >>> TECLADO <<<
        {
            DoAttack();
        }
    }

    // Llamable por el jugador (teclado/joystick) Y por la IA (AIBrain).
    public void DoAttack()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "LaunchShuriken");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        rb.linearVelocity = Prompter.right * -Recoil;

        //lanzar shuriken
        GameObject instantiatedShuriken = Instantiate(ShurikenPrefab, pointerPosition.position, Quaternion.identity);
        instantiatedShuriken.GetComponent<Shuriken>().Launch(Prompter);

        foreach (Transform child in rb.transform)
        {
            if (child.CompareTag("Poison"))
            {
                child.GetComponent<Poison>().ChangeParent(instantiatedShuriken, false);
            }
            else if (child.CompareTag("Sustitution"))
            {
                child.transform.SetParent(instantiatedShuriken.transform);
                child.GetComponent<Sustitution>().parent = instantiatedShuriken;
                child.GetComponent<Sustitution>().playerIndex = playerIndex;
            }
            else if (child.CompareTag("Burn"))
            {
                child.GetComponent<Burn>().ChangeParent(instantiatedShuriken, false);
            }
            else if (child.CompareTag("Freeze"))
            {
                child.GetComponent<Freeze>().ChangeParent(instantiatedShuriken, false);
            }
            else if (child.CompareTag("Paralysis"))
            {
                child.GetComponent<Paralysis>().ChangeParent(instantiatedShuriken, false);
            }
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
