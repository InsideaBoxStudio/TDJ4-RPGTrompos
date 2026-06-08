using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Parry : MonoBehaviour
{
    [SerializeField] private float parryTime = 0.1f;
    [SerializeField] private CheckPlayerTurn playerTurn;
    [SerializeField] private float failTime = 1f;
    [SerializeField] private int energycost = 1;
    [SerializeField] private SpriteRenderer beybladeSprite;
    [SerializeField] private EnergyCounter Energy;

    private CheckPlayerTurn rpgTurn;

    private bool isParryPossible = true;
    private bool isParryActive = false;

    private int playerIndex;

    private void Awake()
    {
        rpgTurn = GetComponent<CheckPlayerTurn>();
        playerIndex = int.Parse(transform.name);
    }

    private void Update()
    {
        if (rpgTurn.isTurnActive) return;
        if (!isParryPossible) return;

        // Seguridad: si no hay un gamepad conectado en este índice (ej: el trompo-IA
        // no tiene un joystick físico), no intentamos leerlo para no crashear.
        if (Gamepad.all.Count <= playerIndex) return;

        if (Gamepad.all[playerIndex].leftShoulder.wasPressedThisFrame)
        {
            StartParry();
        }
    }

    private void StartParry()
    {
        Energy.ChangeEnergy(-energycost);
        isParryPossible = false;
        isParryActive = true;

        GetComponent<BeybladeCollider>().enabled = false;
        beybladeSprite.color = Color.black;

        Invoke(nameof(EndParry), parryTime);
    }

    private void EndParry()
    {
        isParryActive = false;

        GetComponent<BeybladeCollider>().enabled = true;
        beybladeSprite.color = Color.white;

        Invoke(nameof(ResetParry), failTime);
    }

    private void ResetParry()
    {
        isParryPossible = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isDamageable = collision.gameObject.CompareTag("DamageableObject");
        bool isPlayer = collision.gameObject.CompareTag("Player");

        if (!isDamageable && !isPlayer)
            return;

        if (isParryActive)
        {
            Debug.Log("Parry exitoso");
            gameObject.GetComponent<Rigidbody2D>().angularVelocity *= 0;

            if (!isPlayer)
            {
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.angularVelocity *= -1;
                }
            }

            // Optional reward:
            playerTurn.PlayerChoseAnAction(0.5f, 0, false);
            CancelInvoke(nameof(ResetParry));
            Invoke(nameof(ResetParry), 0.1f);
        }
        else
        {
            Debug.Log("Parry fallido");
        }
    }
}