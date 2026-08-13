using UnityEngine;
using UnityEngine.InputSystem;

public class SwordThrust : MonoBehaviour, IAIAction
{
    [SerializeField] private GameObject ColliderAttack;
    [SerializeField] private Transform Prompter;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;

    [Header("Stats")]
    [SerializeField] private int energyCost = 1; //costo de energia
    [SerializeField] private float moveSpeed = 5f; //velocidad de movimiento
    [SerializeField] private float moveDuration = 1f;

    // La estocada es el especial BÁSICO del Caballero: la IA la usa en toda dificultad.
    [SerializeField] private bool esEspecialFuerte = false;

    void Update()
    {
        if (!rpgTurn.isTurnActive) return; // actualizar estado del turno

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.left.wasPressedThisFrame)
        {
            DoAttack();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => energyCounter != null && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoAttack();

    // Llamable por el jugador (teclado/joystick) Y por la IA (AIBrain).
    public void DoAttack()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "SwordThrust");
        rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

        rb.linearVelocity = Prompter.right * moveSpeed;

        ColliderAttack.SetActive(true);
        // Rotar el collider hacia la direccion del ataque
        ColliderAttack.transform.rotation = Quaternion.LookRotation(Vector3.forward, Prompter.right);
        Invoke("EndAttack", moveDuration);
    }

    private void EndAttack()
    {
        ColliderAttack.SetActive(false);
    }
}
