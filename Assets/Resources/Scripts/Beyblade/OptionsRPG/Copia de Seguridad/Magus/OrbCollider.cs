using UnityEngine;

public class OrbCollider : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private float reboundForce = 5f;
    [SerializeField] private float lifeTime = 3f;
    private GameObject pointerPlayer;

    public void Init(int layer)
    {
        //cambiar layer de este objeto
        gameObject.layer = layer;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player != gameObject &&
                player.activeInHierarchy &&
                player.layer != gameObject.layer)
            {
                pointerPlayer = player;
                break;
            }
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != pointerPlayer) return;

        collision.gameObject.GetComponent<Rigidbody2D>().AddForce((pointerPlayer.transform.position - transform.position).normalized * reboundForce, ForceMode2D.Impulse);

        Vector3 contactPoint = collision.ClosestPoint(transform.position);
        collision.gameObject.GetComponent<Vida>().Damage(damage, contactPoint);
        Destroy(gameObject);
    }

    void Update()
    {
        if (pointerPlayer == null) return;

        transform.position = Vector2.MoveTowards( //seguir al jugador
            transform.position,
            pointerPlayer.transform.position,
            speed * Time.deltaTime
        );
    }
}
