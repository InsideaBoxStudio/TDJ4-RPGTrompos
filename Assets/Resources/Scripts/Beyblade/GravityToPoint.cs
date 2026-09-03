using UnityEngine;

public class GravityToPoint : MonoBehaviour
{
    [SerializeField] public Transform gravityPoint; // El punto al que atrae
    public float gravityStrength = 10f;
    [SerializeField] private float spinDirection = 1f;
    public float frictionStrength = 0.5f;

    Rigidbody2D rb;

    void Start()
    {
        if (gravityPoint == null)
        {
            gravityPoint = GameObject.FindGameObjectWithTag("GravityPoint").transform;
        }

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Desactivamos gravedad normal
        rb.linearVelocity = new Vector2(0, spinDirection);
    }

    void FixedUpdate()
    {
        Vector2 direction = (gravityPoint.position - transform.position).normalized;
        rb.AddForce(direction * gravityStrength);

        rb.AddForce(-rb.linearVelocity * frictionStrength);
    }
}