using UnityEngine;
using UnityEngine.InputSystem;

public class Cierra : MonoBehaviour
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

    private GameObject instantiatedCierra;

    // Update is called once per frame
    void Update()
    {
        if (rpgTurn.isTurnActive && ((Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].dpad.down.wasPressedThisFrame)
            || TeclasJugador.Abajo(playerIndex))) // >>> TECLADO <<< (si es su turno y presiona abajo)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost, "Cierra"); // descontar energia
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
    }

    private void EndAttack()
    {
        if (instantiatedCierra != null && !instantiatedCierra.GetComponent<CierraCollider>().hasCollided)
        {
            Destroy(instantiatedCierra);
        }
    }
}
