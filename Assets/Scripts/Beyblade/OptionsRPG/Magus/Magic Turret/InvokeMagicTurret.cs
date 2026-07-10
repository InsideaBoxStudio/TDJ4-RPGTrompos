using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeMagicTurret : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private Transform Prompter;
    [SerializeField] private GameObject MagicTurret;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 4f;
    [SerializeField] private int energyCost = 4; //costo de energia
    [SerializeField] private float moveSpeed = 3f; //velocidad de movimiento

    private CheckPlayerTurn rpgTurn;
    private GameObject[] players;
    private GameObject instantiateMagicTurret;

    private void Awake()
    {
        players = GameObject
            .FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .ToArray();

        rpgTurn = players[playerIndex].GetComponent<CheckPlayerTurn>();
    }

    void Update()
    {
        if (!rpgTurn.isTurnActive) return; // actualizar estado del turno

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.down.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost, "MagicTurret");
            rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

            instantiateMagicTurret = Instantiate(MagicTurret);
            instantiateMagicTurret.transform.position = players[playerIndex].transform.position;
            instantiateMagicTurret.GetComponent<MagicTurret>().playerIndex = playerIndex;

            players[playerIndex].GetComponent<Rigidbody2D>().linearVelocity = Prompter.right * moveSpeed;

            foreach (Transform child in players[playerIndex].transform)
            {
                if (child.CompareTag("Burn"))
                {
                    child.GetComponent<Burn>().ChangeParent(instantiateMagicTurret, false);
                    child.transform.position = instantiateMagicTurret.transform.position;
                }
                else if (child.CompareTag("Freeze"))
                {
                    child.GetComponent<Freeze>().ChangeParent(instantiateMagicTurret, false);
                    child.transform.position = instantiateMagicTurret.transform.position;
                }
                else if (child.CompareTag("Paralysis"))
                {
                    child.GetComponent<Paralysis>().ChangeParent(instantiateMagicTurret, false);
                    child.transform.position = instantiateMagicTurret.transform.position;
                }
            }
        }
    }
}
