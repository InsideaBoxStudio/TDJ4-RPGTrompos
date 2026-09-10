using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeGravity : MonoBehaviour, IAIAction
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private GameObject gravity;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private int energyCost = 2; //costo de energia

    // Especial FUERTE (cuesta 2): solo la dificultad Difícil lo usa siempre.
    [SerializeField] private bool esEspecialFuerte = true;

    private CheckPlayerTurn rpgTurn;
    private GameObject[] players;
    private GameObject instantiateGravity;

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

        if (Controles.Derecha(playerIndex)) // joystick + teclado (ver Controles.cs)
        {
            DoGravity();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => rpgTurn != null && energyCounter != null
        && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoGravity();

    // Llamable por el jugador (joystick) Y por la IA (AIBrain).
    public void DoGravity()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "Gravity");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        instantiateGravity = Instantiate(gravity, players[playerIndex].transform);
        instantiateGravity.transform.SetParent(players[playerIndex].transform);
        instantiateGravity.GetComponent<Gravity>().playerIndex = playerIndex;
    }
}
