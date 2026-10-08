using UnityEngine;
using UnityEngine.InputSystem;

public class SawOption : MonoBehaviour
{
    [SerializeField] private int childIndex = 0;
    [SerializeField] private GameObject PrefabCierra;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private Transform Prompter;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private EnergyCounter energyCounter;
    
    [SerializeField] private Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField] private int energyCost = 4; //costo de energia
    [SerializeField] private float moveSpeed = 5f; //velocidad de movimiento
    [SerializeField] private float moveDuration = 5f;
    [SerializeField] private float desactiveAttack = 2f;

    private GameObject playerObject;
    private AttacksMenuPreset attacksMenuPreset;

    private GameObject instantiatedCierra;

    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavía no lo tiene, queda el valor serializado de siempre.

    private void Start()
    {
        childIndex = transform.GetSiblingIndex();
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
        FindReferences();
    }

    // Update is called once per frame
    void Update()
    {
        if (attacksMenuPreset.buttonState == childIndex) // >>> TECLADO <<<
        {
            DoAttack();
        }
    }

    public void DoAttack()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "Sierra"); // descontar energia
        rpgTurn.PlayerChoseAnAction(moveDuration, 2f, false);

        instantiatedCierra = Instantiate(PrefabCierra, rb.position, Quaternion.identity, rb.transform);
        instantiatedCierra.GetComponent<CierraCollider>().playerID = playerIndex;

        foreach (Transform child in rb.transform)
        {
            if (child.CompareTag("Poison"))
            {
                child.GetComponent<Poison>().ChangeParent(instantiatedCierra, false); //envenenar al jugadorx
            }
            else if (child.CompareTag("Burn"))
            {
                child.GetComponent<Burn>().ChangeParent(instantiatedCierra, false);
            }
            else if (child.CompareTag("Freeze"))
            {
                child.GetComponent<Freeze>().ChangeParent(instantiatedCierra, false);
            }
            else if (child.CompareTag("Paralysis"))
            {
                child.GetComponent<Paralysis>().ChangeParent(instantiatedCierra, false);
            }
        }

        rb.linearVelocity = Prompter.right * moveSpeed;

        Invoke("EndAttack", desactiveAttack);
    }

    private void EndAttack()
    {
        if (instantiatedCierra != null && !instantiatedCierra.GetComponent<CierraCollider>().hasCollided)
        {
            Destroy(instantiatedCierra);
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
