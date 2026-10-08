using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PoisonOption : MonoBehaviour
{
    [SerializeField] private int childIndex = 0;
    [SerializeField] private GameObject PrefabVeneno;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private GameObject Player;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private int energyCost = 4; //costo de energia
    [SerializeField] private float moveDuration = 0.1f;

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
        if (!rpgTurn.isTurnActive) return;
        if (attacksMenuPreset.buttonState == childIndex) // >>> TECLADO <<<
        {
            DoAttack();
        }
    }

    public void DoAttack()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, true);
        energyCounter.ChangeEnergy(-energyCost, "Poison");
        Instantiate(PrefabVeneno, Player.transform);
    }

    private void FindReferences()
    {
        if (Player == null)
        {
            Player = PlayerSettings.player[playerIndex];
        }

        DefPlayerID defPlayerID = Player.GetComponent<DefPlayerID>();
        attacksMenuPreset = transform.parent.GetComponent<AttacksMenuPreset>();
        
        if (energyCounter == null)
        {
            energyCounter = defPlayerID.energyCounter;
        }
        if (rpgTurn == null)
        {
            rpgTurn = Player.GetComponent<CheckPlayerTurn>();
        }
    }
}
