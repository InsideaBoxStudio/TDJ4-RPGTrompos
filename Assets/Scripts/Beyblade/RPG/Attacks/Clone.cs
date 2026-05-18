using Unity.VisualScripting;
using UnityEngine;

public class Clone : MonoBehaviour
{
    [SerializeField] private float timeAttack = 1;
    [SerializeField] private float timeDead = 2;
    [SerializeField] private float velocity;
    [SerializeField] private Transform prompter;

    private bool isAttacking = false;

    private void Awake()
    {
        Invoke("AttackNow", timeAttack);
        Invoke("Dead", timeDead);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && isAttacking)
        {
            collision.gameObject.GetComponent<Vida>().Damage(10);
            Vector2 direction = (transform.position - collision.gameObject.transform.position).normalized;
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(-direction * velocity, ForceMode2D.Impulse);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && isAttacking)
        {
            collision.gameObject.GetComponent<Vida>().Damage(10);
            Vector2 direction = (transform.position - collision.gameObject.transform.position).normalized;
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(-direction * velocity, ForceMode2D.Impulse);
            Destroy(gameObject);
        }
    }

    private void AttackNow()
    {
        isAttacking = true;
        Vector2 direction = prompter.right;
        GetComponent<Rigidbody2D>().AddForce(direction * velocity, ForceMode2D.Impulse);
    }

    private void Dead()
    {
        Destroy(gameObject);
    }
}
