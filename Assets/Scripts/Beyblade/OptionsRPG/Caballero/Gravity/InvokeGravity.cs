using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeGravity : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private GameObject gravity;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private int energyCost = 2; //costo de energia

    private CheckPlayerTurn rpgTurn;
    private GameObject[] players;
    private GameObject instantiateGravity;

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

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.right.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost, "Gravity");
            rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

            instantiateGravity = Instantiate(gravity, players[playerIndex].transform);
            instantiateGravity.transform.SetParent(players[playerIndex].transform);
            instantiateGravity.GetComponent<Gravity>().playerIndex = playerIndex;
        }
    }
}
