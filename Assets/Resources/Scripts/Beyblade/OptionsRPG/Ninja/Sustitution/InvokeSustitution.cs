using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeSustitution : MonoBehaviour, IAIAction
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private GameObject sustitution;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private int energyCost = 2; //costo de energia

    // Especial FUERTE (defensivo, cuesta 2): solo la dificultad Difícil lo usa siempre.
    [SerializeField] private bool esEspecialFuerte = true;

    private CheckPlayerTurn rpgTurn;
    private GameObject[] players;
    private GameObject instantiateSustitution;

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

        if (Controles.L2(playerIndex)) // joystick + teclado (ver Controles.cs)
        {
            DoSustitution();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => rpgTurn != null && energyCounter != null
        && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoSustitution();

    // Llamable por el jugador (joystick) Y por la IA (AIBrain).
    public void DoSustitution()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "Sustitution");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        instantiateSustitution = Instantiate(sustitution, players[playerIndex].transform);
        instantiateSustitution.transform.SetParent(players[playerIndex].transform);
        instantiateSustitution.transform.position = players[playerIndex].transform.position;
        instantiateSustitution.GetComponent<Sustitution>().playerIndex = playerIndex;
    }
}
