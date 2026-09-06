using UnityEngine;
using UnityEngine.InputSystem;

public class Cierra : MonoBehaviour, IAIAction
{
    [SerializeField] private GameObject PrefabCierra;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private Transform Prompter;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private EnergyCounter energyCounter;
    
    [SerializeField] private Rigidbody2D rb;

    [Header("Stats")]
    [SerializeField] private int energyCost = 4; //costo de energia
    [SerializeField] private float moveSpeed = 5f; //velocidad de movimiento
    [SerializeField] private float moveDuration = 5f;
    [SerializeField] private float desactiveAttack = 2f;

    // Especial FUERTE (cuesta 4): solo la dificultad Difícil lo usa siempre.
    [SerializeField] private bool esEspecialFuerte = true;

    private GameObject instantiatedCierra;

    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavía no lo tiene, queda el valor serializado de siempre.
    private void Start()
    {
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (rpgTurn.isTurnActive && ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.down.wasPressedThisFrame)
            || TeclasJugador.Abajo(playerIndex))) // >>> TECLADO <<< (si es su turno y presiona abajo)
        {
            DoCierra();
        }
    }

    // ---- IAIAction: contrato con la IA (ver IAIAction.cs) ----
    public int EnergyCost => energyCost;
    public bool IsStrong => esEspecialFuerte;
    public bool CanExecute() => energyCounter != null && energyCounter.currentEnergy >= energyCost;
    public void Execute() => DoCierra();

    // Llamable por el jugador (teclado/joystick) Y por la IA (AIBrain).
    public void DoCierra()
    {
        if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
        energyCounter.ChangeEnergy(-energyCost, "Sierra"); // descontar energia
        rpgTurn.PlayerChoseAnAction(moveDuration, 2f, false);

        instantiatedCierra = Instantiate(PrefabCierra, rb.position, Quaternion.identity, rb.transform);
        instantiatedCierra.GetComponent<CierraCollider>().playerID = playerIndex;

        foreach (Transform child in rb.transform)
        {
            if (child.CompareTag("Poison"))
            {
                child.GetComponent<Poison>().ChangeParent(instantiatedCierra, false); //envenenar al jugadorx
            }
            else if (child.CompareTag("Burn"))
            {
                child.GetComponent<Burn>().ChangeParent(instantiatedCierra, false);
            }
            else if (child.CompareTag("Freeze"))
            {
                child.GetComponent<Freeze>().ChangeParent(instantiatedCierra, false);
            }
            else if (child.CompareTag("Paralysis"))
            {
                child.GetComponent<Paralysis>().ChangeParent(instantiatedCierra, false);
            }
        }

        rb.linearVelocity = Prompter.right * moveSpeed;

        Invoke("EndAttack", desactiveAttack);
    }

    private void EndAttack()
    {
        if (instantiatedCierra != null && !instantiatedCierra.GetComponent<CierraCollider>().hasCollided)
        {
            Destroy(instantiatedCierra);
        }
    }
}
