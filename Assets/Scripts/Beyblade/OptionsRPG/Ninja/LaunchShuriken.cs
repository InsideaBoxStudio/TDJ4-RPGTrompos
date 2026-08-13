using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchShuriken : MonoBehaviour, IAIAction
{
    [SerializeField] private GameObject ShurikenPrefab;
    [SerializeField] private Transform Prompter;
    [SerializeField] private Transform pointerPosition;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;


    [Header("Stats")]
    [SerializeField] private int energyCost = 2; //costo de energia
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float Recoil = 1f;

    // El shuriken es el especial BÁSICO del Ninja: la IA lo usa en toda dificultad.
    [SerializeField] private bool esEspecialFuerte = false;



    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno

        if (turnActive && ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.right.wasPressedThisFrame)
            || TeclasJugador.Shuriken(playerIndex))) // >>> TECLADO <<<
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
        energyCounter.ChangeEnergy(-energyCost, "LaunchShuriken");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        rb.linearVelocity = Prompter.right * -Recoil;

        //lanzar shuriken
        GameObject instantiatedShuriken = Instantiate(ShurikenPrefab, pointerPosition.position, Quaternion.identity);
        instantiatedShuriken.GetComponent<Shuriken>().Launch(Prompter);

        foreach (Transform child in rb.transform)
        {
            if (child.CompareTag("Poison"))
            {
                child.GetComponent<Poison>().ChangeParent(instantiatedShuriken, false);
            }
            else if (child.CompareTag("Sustitution"))
            {
                child.transform.SetParent(instantiatedShuriken.transform);
                child.GetComponent<Sustitution>().parent = instantiatedShuriken;
                child.GetComponent<Sustitution>().playerIndex = playerIndex;
            }
            else if (child.CompareTag("Burn"))
            {
                child.GetComponent<Burn>().ChangeParent(instantiatedShuriken, false);
            }
            else if (child.CompareTag("Freeze"))
            {
                child.GetComponent<Freeze>().ChangeParent(instantiatedShuriken, false);
            }
            else if (child.CompareTag("Paralysis"))
            {
                child.GetComponent<Paralysis>().ChangeParent(instantiatedShuriken, false);
            }
        }
    }
}
