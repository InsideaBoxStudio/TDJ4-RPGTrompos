using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class useShockwave : MonoBehaviour, IAIAction
{
    [SerializeField] private GameObject shockwave;
    [SerializeField] private GameObject[] players;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 5f;
    [SerializeField] private int energyCost = 1; //costo de energia

    // Especial BÁSICO (cuesta 1): la IA lo usa en toda dificultad.
    [SerializeField] private bool esEspecialFuerte = false;

    private CheckPlayerTurn rpgTurn;
    private GameObject shockwaveInstantiate;

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

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].rightShoulder.wasPressedThisFrame)
        {
            DoShockwave();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => rpgTurn != null && energyCounter != null
        && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoShockwave();

    // Llamable por el jugador (joystick) Y por la IA (AIBrain).
    public void DoShockwave()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "Shockwave");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        shockwaveInstantiate = Instantiate(shockwave);
        shockwaveInstantiate.transform.SetParent(players[playerIndex].transform);
        shockwaveInstantiate.GetComponent<Shockwave>().playerIndex = playerIndex;
    }
}
