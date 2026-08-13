using UnityEngine;
using UnityEngine.InputSystem;

public class MagicShot: MonoBehaviour, IAIAction
{
    [SerializeField] private GameObject ShurikenPrefab;
    [SerializeField] private Transform Prompter;
    [SerializeField] private Transform pointerPosition;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;

    [Header("Stats")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private int energyCost = 2; //costo de energia
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float Recoil = 1f;

    // El disparo mágico es el especial BÁSICO del Magus: la IA lo usa en toda dificultad.
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
        energyCounter.ChangeEnergy(-energyCost, "MagicShot");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        rb.linearVelocity = Prompter.right * -Recoil;

        //lanzar shuriken
        GameObject instantiatedShuriken = Instantiate(ShurikenPrefab, pointerPosition.position, Quaternion.identity);
        instantiatedShuriken.GetComponent<Rigidbody2D>().linearVelocity = Prompter.right * moveSpeed;

        foreach (Transform child in rb.transform)
        {
            if (child.CompareTag("Burn"))
            {
                child.GetComponent<Burn>().ChangeParent(instantiatedShuriken, false);
                child.transform.position = instantiatedShuriken.transform.position;
            }
            else if (child.CompareTag("Freeze"))
            {
                child.GetComponent<Freeze>().ChangeParent(instantiatedShuriken, false);
                child.transform.position = instantiatedShuriken.transform.position;
            }
            else if (child.CompareTag("Paralysis"))
            {
                child.GetComponent<Paralysis>().ChangeParent(instantiatedShuriken, false);
                child.transform.position = instantiatedShuriken.transform.position;
            }
        }
    }
}
