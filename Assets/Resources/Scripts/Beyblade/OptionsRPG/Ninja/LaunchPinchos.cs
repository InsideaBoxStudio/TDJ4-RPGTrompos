using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchPinchos : MonoBehaviour, IAIAction
{
    [SerializeField] private GameObject PinchosPrefab;
    [SerializeField] private Transform Prompter;
    [SerializeField] private Transform pointerPosition;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;

    
    [Header("Stats")]
    [SerializeField] private int energyCost = 2; //costo de energia
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float Recoil = 10f;

    // Especial FUERTE: solo la dificultad Difícil lo usa siempre.
    [SerializeField] private bool esEspecialFuerte = true;



    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavia no lo tiene, queda el valor serializado de siempre.
    private void Start()
    {
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
    }

    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno

        if (turnActive && ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.left.wasPressedThisFrame)
            || TeclasJugador.Pinchos(playerIndex))) // >>> TECLADO <<<
        {
            DoLaunch();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => energyCounter != null && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoLaunch();

    // Llamable por el jugador (teclado/joystick) Y por la IA (AIBrain).
    public void DoLaunch()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "LaunchPinchos");
        rpgTurn.PlayerChoseAnAction(moveDuration, 3f, false);

        rb.linearVelocity = Prompter.right * -Recoil;

        //lanzar pinchos
        GameObject instantiatedPincho = Instantiate(PinchosPrefab, pointerPosition.position, Quaternion.identity);

        foreach (Transform child in rb.transform)
        {
            if (child.CompareTag("Poison"))
            {
                child.GetComponent<Poison>().ChangeParent(instantiatedPincho, false); //envenenar al jugador
            }
            else if (child.CompareTag("Sustitution")){
                child.transform.SetParent(instantiatedPincho.transform);
                child.GetComponent<Sustitution>().parent = instantiatedPincho;
                child.GetComponent<Sustitution>().playerIndex = playerIndex;
            }
        }
    }
}
