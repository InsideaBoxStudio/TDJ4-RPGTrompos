using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeSword : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private GameObject sword;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private int energyCost = 1; //costo de energia

    private CheckPlayerTurn rpgTurn;
    private GameObject[] players;
    private GameObject instantiateSword;
    private void Awake()
    {
        players = GameObject
            .FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .ToArray();

        rpgTurn = players[playerIndex].GetComponent<CheckPlayerTurn>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!rpgTurn.isTurnActive) return; // actualizar estado del turno

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].leftShoulder.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost, "BasicAttack");
            rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

            instantiateSword = Instantiate(sword, players[playerIndex].transform);
            instantiateSword.transform.SetParent(players[playerIndex].transform);
            instantiateSword.transform.GetChild(0).GetComponent<SwordBlow>().playerIndex = playerIndex;
        }
    }
}