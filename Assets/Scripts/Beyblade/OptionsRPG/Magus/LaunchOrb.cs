using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchOrb : MonoBehaviour, IAIAction
{
    [SerializeField] private GameObject OrbPrefab;
    [SerializeField] private Transform Prompter;
    [SerializeField] private Transform pointerPosition;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;


    [Header("Stats")]
    [SerializeField] private int energyCost = 4; //costo de energia
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float Recoil = 10f;

    // Especial FUERTE (cuesta 4): solo la dificultad Difícil lo usa siempre.
    [SerializeField] private bool esEspecialFuerte = true;

    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno

        if (turnActive && ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.down.wasPressedThisFrame)
            || TeclasJugador.Abajo(playerIndex))) // >>> TECLADO <<<
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
        energyCounter.ChangeEnergy(-energyCost, "LaunchOrb");
        rpgTurn.PlayerChoseAnAction(moveDuration, 3f, false);

        rb.linearVelocity = Prompter.right * -Recoil;

        GameObject instantiatedOrb = Instantiate(OrbPrefab, pointerPosition.position, Quaternion.identity);
        instantiatedOrb.GetComponent<OrbCollider>().Init(Prompter.gameObject.layer);

        foreach (Transform child in rb.transform)
        {
            if (child.CompareTag("Poison"))
            {
                child.GetComponent<Poison>().ChangeParent(instantiatedOrb, false); //envenenar al jugador
            }
            else if (child.CompareTag("Burn"))
            {
                child.GetComponent<Burn>().ChangeParent(instantiatedOrb, false);
            }
            else if (child.CompareTag("Freeze"))
            {
                child.GetComponent<Freeze>().ChangeParent(instantiatedOrb, false);
            }
            else if (child.CompareTag("Paralysis"))
            {
                child.GetComponent<Paralysis>().ChangeParent(instantiatedOrb, false);
            }
        }
    }
}
