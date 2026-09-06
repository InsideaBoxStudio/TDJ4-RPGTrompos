using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeSword : MonoBehaviour, IAIAction
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private GameObject sword;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private int energyCost = 1; //costo de energia

    // Especial BÁSICO (cuesta 1): la IA lo usa en toda dificultad.
    [SerializeField] private bool esEspecialFuerte = false;

    private CheckPlayerTurn rpgTurn;
    private GameObject[] players;
    private GameObject instantiateSword;

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

    // Update is called once per frame
    void Update()
    {
        if (rpgTurn == null || !rpgTurn.isTurnActive) return; // actualizar estado del turno

        if (Controles.L1(playerIndex)) // joystick + teclado (ver Controles.cs)
        {
            DoSwordSlash();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => rpgTurn != null && energyCounter != null
        && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoSwordSlash();

    // Llamable por el jugador (joystick) Y por la IA (AIBrain).
    public void DoSwordSlash()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "SwordSlash");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        instantiateSword = Instantiate(sword, players[playerIndex].transform);
        instantiateSword.transform.SetParent(players[playerIndex].transform);
        instantiateSword.transform.GetChild(0).GetComponent<SwordBlow>().playerIndex = playerIndex;
    }
}