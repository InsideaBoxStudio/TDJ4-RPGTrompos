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

    void Update()
    {
        if (rpgTurn.isTurnActive && Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].leftShoulder.wasPressedThisFrame)
        {
            energyCounter.ChangeEnergy(-energyCost, "ActiveBurn");
            Instantiate(PrefabFuego, Player.transform);
        }
    }
}
