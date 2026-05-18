using UnityEngine;

public class Shuriken : MonoBehaviour
{
    [SerializeField] private int possibleRebounds = 3;
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private int Damage = 5;
    private Rigidbody2D rb;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Vida>().Damage(Damage);
            gameObject.GetComponent<Collider2D>().enabled = false;
            Invoke("Dead", 0.1f);
            return;
        }
        if (possibleRebounds < 0)
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
            Invoke("Dead", 0.1f);
            return;
        }

        ContactPoint2D contact = collision.contacts[0];

        Vector2 normal = contact.normal;

        Vector2 velocidadImpacto = collision.relativeVelocity;

        Vector2 velocidadReflejada = Vector2.Reflect(velocidadImpacto, normal);

        rb.linearVelocity = -velocidadReflejada;

        possibleRebounds--;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Transform Prompter)
    {
        rb.linearVelocity = Prompter.right * moveSpeed;
    }

    private void Dead()
    {
        Destroy(gameObject);
    }
}
