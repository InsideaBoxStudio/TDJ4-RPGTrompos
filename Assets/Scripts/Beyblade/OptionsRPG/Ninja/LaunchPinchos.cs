using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchPinchos : MonoBehaviour
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



    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno

        // Joystick (dpad izquierda) O teclado del Jugador 1 (tecla R)
        bool joy = Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.left.wasPressedThisFrame;
        bool tecla = playerIndex == 0 && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
        if (turnActive && (joy || tecla))
        {
            DoLaunch();
        }
    }

    // Cuánta energía cuesta (la IA lo consulta para decidir).
    public int EnergyCost => energyCost;

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
        }
    }
}
