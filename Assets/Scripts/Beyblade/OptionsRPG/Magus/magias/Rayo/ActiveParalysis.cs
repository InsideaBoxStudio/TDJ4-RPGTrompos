using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActiveParalysis : MonoBehaviour
{
    [SerializeField] private GameObject PrefabHielo;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private GameObject Player;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private int energyCost = 4; //costo de energia

    void Update()
    {
        if (rpgTurn.isTurnActive && Gamepad.all[playerIndex].leftShoulder.wasPressedThisFrame)
        {
            energyCounter.ChangeEnergy(-energyCost, "Paralysis");
            Instantiate(PrefabHielo, Player.transform);
        }
    }
}
