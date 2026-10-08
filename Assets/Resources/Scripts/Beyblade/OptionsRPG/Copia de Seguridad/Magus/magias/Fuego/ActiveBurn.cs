using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActiveBurn : MonoBehaviour
{
    [SerializeField] private GameObject PrefabFuego;
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
    }

    void Update()
    {
        if (rpgTurn.isTurnActive && ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].leftShoulder.wasPressedThisFrame)
            || TeclasJugador.L1(playerIndex))) // >>> TECLADO <<<
        {
            energyCounter.ChangeEnergy(-energyCost, "Burn");
            Instantiate(PrefabFuego, Player.transform);
        }
    }
}
