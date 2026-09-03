using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeRotateOrbs : MonoBehaviour
{
    [SerializeField] private GameObject OrbPrefab;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private int energyCost = 4; //costo de energia
    [SerializeField] private float moveDuration = 2f;

    private GameObject[] players;

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
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno

        if (turnActive && Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.left.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost, "ProtectOrbs");
            rpgTurn.PlayerChoseAnAction(moveDuration, 3f, false);

            GameObject instantiatedOrbs = Instantiate(OrbPrefab);
            instantiatedOrbs.transform.SetParent(players[playerIndex].transform);
            instantiatedOrbs.transform.position = players[playerIndex].transform.position;

            foreach (Transform child in players[playerIndex].transform)
            {
                if (child.CompareTag("Burn"))
                {
                    child.GetComponent<Burn>().ChangeParent(instantiatedOrbs, false);
                    child.transform.position = instantiatedOrbs.transform.position;
                }
                else if (child.CompareTag("Freeze"))
                {
                    child.GetComponent<Freeze>().ChangeParent(instantiatedOrbs, false);
                    child.transform.position = instantiatedOrbs.transform.position;
                }
                else if (child.CompareTag("Paralysis"))
                {
                    child.GetComponent<Paralysis>().ChangeParent(instantiatedOrbs, false);
                    child.transform.position = instantiatedOrbs.transform.position;
                }
            }
        }
    }
}
