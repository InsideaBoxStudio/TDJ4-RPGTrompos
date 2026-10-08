using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActivePoison : MonoBehaviour
{
    [SerializeField] private GameObject PrefabVeneno;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private GameObject Player;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private int energyCost = 4; //costo de energia

    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavia no lo tiene, queda el valor serializado de siempre.
    private void Start()
    {
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
        FindReferences();
    }

    void Update()
    {
        if (rpgTurn.isTurnActive && ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].leftShoulder.wasPressedThisFrame)
            || TeclasJugador.Veneno(playerIndex))) // >>> TECLADO <<<
        {
            energyCounter.ChangeEnergy(-energyCost, "Poison");
            Instantiate(PrefabVeneno, Player.transform);
        }
    }
    
    private void FindReferences()
    {
        if (Player == null)
        {
            Player = PlayerSettings.player[playerIndex];
        }

        DefPlayerID defPlayerID = Player.GetComponent<DefPlayerID>();
        
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
