using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchShuriken : MonoBehaviour
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



    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno
        
        if (turnActive && Gamepad.all[playerIndex].dpad.right.wasPressedThisFrame)
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
}