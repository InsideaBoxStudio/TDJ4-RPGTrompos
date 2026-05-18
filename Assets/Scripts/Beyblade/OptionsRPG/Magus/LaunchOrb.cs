using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchOrb : MonoBehaviour
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



    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive; // actualizar estado del turno

        if (turnActive && Gamepad.all[playerIndex].rightShoulder.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost);
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
            }
        }
    }
}
