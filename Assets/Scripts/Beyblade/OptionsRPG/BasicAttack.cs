using UnityEngine;
using UnityEngine.InputSystem;
public class BasicAttack : MonoBehaviour
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


    void Update()
    {
        if (!rpgTurn.isTurnActive) return; // actualizar estado del turno
        
        if (Gamepad.all[playerIndex].dpad.up.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost, "BasicAttack");
            rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

            rb.linearVelocity = Prompter.right * moveSpeed;

            ColliderAttack.SetActive(true);
            // Rotar el collider hacia la direccion del ataque
            ColliderAttack.transform.rotation = Quaternion.LookRotation(Vector3.forward, Prompter.right);
            Invoke("EndAttack", moveDuration);
        }
    }

    private void EndAttack()
    {
        ColliderAttack.SetActive(false);
    }
}