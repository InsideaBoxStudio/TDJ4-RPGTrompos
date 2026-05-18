using UnityEngine;

public class BeybladeCollider : MonoBehaviour
{
    [SerializeField] private float reboundForce = 1f;
    [SerializeField] private float reboundForceBeyblade = 0.5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float rebound = 0f;
        if (collision.gameObject.CompareTag("Player"))
        {
            rebound = reboundForceBeyblade;
        }
        else
        {
            rebound = reboundForce;
        }

        ContactPoint2D contact = collision.contacts[0];

        Vector2 normal = contact.normal;

        Vector2 velocidadImpacto = collision.relativeVelocity;

        Vector2 velocidadReflejada = Vector2.Reflect(velocidadImpacto, normal);

        rb.linearVelocity = velocidadReflejada * rebound;
    }
}