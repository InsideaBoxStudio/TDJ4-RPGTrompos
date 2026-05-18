using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchClone : MonoBehaviour
{
    [SerializeField] private GameObject ClonePrefab;
    [SerializeField] private Transform Prompter;
    [SerializeField] private Transform pointerPosition;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private EnergyCounter energyCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int playerIndex = 0;

    [Header("Stats")]
    [SerializeField] private int energyCost = 2;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float Recoil = 10f;

    void Update()
    {
        bool turnActive = rpgTurn.isTurnActive;

        if (turnActive && Gamepad.all[playerIndex].rightShoulder.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return;

            energyCounter.ChangeEnergy(-energyCost);
            rpgTurn.PlayerChoseAnAction(moveDuration, 3f, false);

            // Dirección hacia donde apunta el prompter
            Vector2 dir = Prompter.right.normalized;

            // Dirección perpendicular (90 grados)
            Vector2 perpendicular = new Vector2(-dir.y, dir.x);

            // Recoil del jugador
            rb.linearVelocity = -perpendicular * Recoil;

            // Crear clon
            GameObject clone = Instantiate(
                ClonePrefab,
                pointerPosition.position,
                Quaternion.identity
            );

            // Recoil del clon en dirección opuesta
            Rigidbody2D cloneRb = clone.GetComponent<Rigidbody2D>();

            if (cloneRb != null)
            {
                cloneRb.linearVelocity = perpendicular * Recoil;
            }
        }
    }
}