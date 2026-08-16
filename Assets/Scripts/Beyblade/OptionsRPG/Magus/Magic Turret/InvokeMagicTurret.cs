using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeMagicTurret : MonoBehaviour, IAIAction
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private Transform Prompter;
    [SerializeField] private GameObject MagicTurret;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 4f;
    [SerializeField] private int energyCost = 4; //costo de energia
    [SerializeField] private float moveSpeed = 3f; //velocidad de movimiento

    // Especial FUERTE (cuesta 4): solo la dificultad Difícil lo usa siempre.
    [SerializeField] private bool esEspecialFuerte = true;

    private CheckPlayerTurn rpgTurn;
    private GameObject[] players;
    private GameObject instantiateMagicTurret;

    // Se resuelve en Start (no en Awake): el AIBrain reescribe playerIndex durante
    // su propio Awake y el orden entre Awakes no está garantizado. Todos los Awake
    // corren antes que cualquier Start, así que acá el índice ya es el definitivo.
    private void Start()
    {
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);

        players = GameObject
            .FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .ToArray();

        if (playerIndex < players.Length)
            rpgTurn = players[playerIndex].GetComponent<CheckPlayerTurn>();
    }

    void Update()
    {
        if (rpgTurn == null || !rpgTurn.isTurnActive) return; // actualizar estado del turno

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.down.wasPressedThisFrame)
        {
            DoMagicTurret();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => rpgTurn != null && energyCounter != null
        && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoMagicTurret();

    // Llamable por el jugador (joystick) Y por la IA (AIBrain).
    public void DoMagicTurret()
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
