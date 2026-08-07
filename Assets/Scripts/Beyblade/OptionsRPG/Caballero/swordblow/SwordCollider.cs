using Unity.VisualScripting;
using UnityEngine;

public class SwordCollider : MonoBehaviour
{
    public GameObject attackingPlayer;
    public float impactImpulse;
    public int damage = 0;

    private GameObject playerAttacked;
    private Collider2D swordTrigger;
    private Vida vida;
    private Rigidbody2D rigidbody2D;
    private Vector2 direction;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ( !collision.gameObject.CompareTag("Player")) return;
        if (collision.gameObject == attackingPlayer) return;

        playerAttacked = collision.gameObject;
        swordTrigger = transform.GetComponent<Collider2D>();
        swordTrigger.enabled = false;

        // quitar vida
        Vector3 contactPoint = collision.ClosestPoint(transform.position);
        vida = playerAttacked.GetComponent<Vida>();
        vida.Damage(damage, contactPoint);

        // calcular direccion para impulsar
        direction = (playerAttacked.transform.position - attackingPlayer.transform.position).normalized;

        // impulsar al jugador atacante
        rigidbody2D = playerAttacked.GetComponent<Rigidbody2D>();
        rigidbody2D.AddForce(direction * impactImpulse, ForceMode2D.Impulse);
    }
}
