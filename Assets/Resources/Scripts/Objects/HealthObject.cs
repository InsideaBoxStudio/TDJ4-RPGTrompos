using UnityEngine;

public class HealthObject : MonoBehaviour
{
    [SerializeField] private int addHealth = 25;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player")) return;

        Vida vida = collider.gameObject.GetComponent<Vida>();
        if (vida == null) return;

        Vector3 contactPoint = collider.ClosestPoint(transform.position);
        vida.Damage(addHealth, contactPoint);

        Destroy(gameObject);
    }
}
