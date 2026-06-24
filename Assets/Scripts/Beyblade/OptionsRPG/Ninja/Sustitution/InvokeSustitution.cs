using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeSustitution : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private GameObject sustitution;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private int energyCost = 2; //costo de energia

    private CheckPlayerTurn rpgTurn;
    private GameObject[] players;
    private GameObject instantiateSustitution;

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

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].leftTrigger.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost, "BasicAttack");
            rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

            instantiateSustitution = Instantiate(sustitution, players[playerIndex].transform);
            instantiateSustitution.transform.SetParent(players[playerIndex].transform);
            instantiateSustitution.transform.position = players[playerIndex].transform.position;
            instantiateSustitution.GetComponent<Sustitution>().playerIndex = playerIndex;
        }
    }
}
