using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvokeRotateOrbs : MonoBehaviour, IAIAction
{
    [SerializeField] private GameObject OrbPrefab;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private int energyCost = 4; //costo de energia
    [SerializeField] private float moveDuration = 2f;

    // Especial FUERTE (cuesta 4): solo la dificultad Difícil lo usa siempre.
    [SerializeField] private bool esEspecialFuerte = true;

    private GameObject[] players;

    // Se resuelve en Start (no en Awake): el AIBrain reescribe playerIndex durante
    // su propio Awake y el orden entre Awakes no está garantizado. Todos los Awake
    // corren antes que cualquier Start, así que acá el índice ya es el definitivo.
    private void Start()
    {
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
        if (rpgTurn == null) return;
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno

        if (turnActive && Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.left.wasPressedThisFrame)
        {
            DoProtectiveOrbs();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => rpgTurn != null && energyCounter != null
        && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoProtectiveOrbs();

    // Llamable por el jugador (joystick) Y por la IA (AIBrain).
    public void DoProtectiveOrbs()
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
