using UnityEngine;

public class Pincho : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float rebound = 1f;
    [SerializeField] private float additionalTime = 1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Vida vida = collision.gameObject.GetComponent<Vida>();
        if (vida == null) return;

        vida.Damage(damage);
        Vector2 direction = (collision.transform.position - transform.position).normalized;
        collision.gameObject.GetComponent<Rigidbody2D>().AddForce(direction * rebound, ForceMode2D.Impulse);

        CheckPlayerTurn checkPlayerTurn = collision.gameObject.GetComponent<CheckPlayerTurn>();
        if (checkPlayerTurn != null)
        {
            checkPlayerTurn.PlayerChoseAnAction(additionalTime, 1f, true);
        }
        Destroy(gameObject);
    }
}
