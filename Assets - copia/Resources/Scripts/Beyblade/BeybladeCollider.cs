using Unity.VisualScripting;
using UnityEngine;

public class BeybladeCollider : MonoBehaviour
{
    [SerializeField] private float reboundForce = 1f;
    [SerializeField] private float reboundForceBeyblade = 0.5f;
    [SerializeField] private float reboundForceBeybladeStay = 10f;
    [SerializeField] private float maxSpeedForRebound = 1f;
    [SerializeField] private float randomAngle = 10f; // Desvío máximo en grados
    [SerializeField] private GameObject particles;

    private float contactTime;
    private float necessaryTime = 0.5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 pointColision = collision.GetContact(0).point;
        GameObject sparks = Instantiate(particles, pointColision, Quaternion.identity);

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

        // Aplicar una pequeña desviación aleatoria
        float angle = Random.Range(-randomAngle, randomAngle);
        velocidadReflejada = Quaternion.Euler(0, 0, angle) * velocidadReflejada;

        rb.linearVelocity = velocidadReflejada * rebound;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        contactTime += Time.deltaTime;
        Vector2 pointColision = collision.GetContact(0).point;
        GameObject sparks = Instantiate(particles, pointColision, Quaternion.identity);

        if (contactTime >= necessaryTime)
        {
            EjecutarFuncion(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        contactTime = 0f;
    }

    private void EjecutarFuncion(Collision2D collision)
    {
        Vector2 direccion = transform.position - collision.transform.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        Vector2 direccionContraria = -direccion.normalized;
        rb.linearVelocity = direccionContraria * reboundForceBeybladeStay;
    }
}