using UnityEngine;

public class ColliderOrbs : MonoBehaviour
{
    [SerializeField] private float impactImpulse = 5;
    [SerializeField] private int damage = 2;

    private Rigidbody2D rigidbody2D;
    private Vida vida;
    private Vector2 direction;
    private GameObject playerAttacked;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerAttacked = collision.gameObject;

            // quitar vida
            vida = playerAttacked.GetComponent<Vida>();
            vida.Damage(damage);

            // calcular direccion para impulsar
            direction = (playerAttacked.transform.position - transform.position).normalized;

            // impulsar al jugador atacante
            rigidbody2D = playerAttacked.GetComponent<Rigidbody2D>();
            rigidbody2D.AddForce(direction * impactImpulse, ForceMode2D.Impulse);
        }

        Destroy(gameObject);
    }
}