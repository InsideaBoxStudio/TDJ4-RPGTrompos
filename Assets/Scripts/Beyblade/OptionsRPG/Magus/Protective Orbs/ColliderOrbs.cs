using UnityEngine;

public class ColliderOrbs : MonoBehaviour
{
    [SerializeField] private float impactImpulse = 5;
    [SerializeField] private int damage = 2;
    [SerializeField] private GameObject stateBarr;

    public int playerIndex = 0;

    [SerializeField] private string element = "nothing";

    private GameObject brother;
    private Rigidbody2D rigidbody2D;
    private Vida vida;
    private Vector2 direction;
    private GameObject playerAttacked;
    private GameObject barrInstantiate;

    private bool trigger = false;

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

            if (element != "nothing" && element != "Paralysis")
            {
                bool barrCreated = false;

                foreach (Transform hijo in playerAttacked.transform)
                {
                    if (hijo.name == stateBarr.name) barrCreated = true;
                }

                if (!barrCreated)
                {
                    barrInstantiate = Instantiate(stateBarr, playerAttacked.transform);
                    barrInstantiate.transform.position = playerAttacked.transform.position;
                }

                if (element == "Burn")
                {
                    if (barrInstantiate.GetComponent<CircularBarState>().stateValue > 0)
                    {
                        vida.Damage(damage);
                    }

                    barrInstantiate.GetComponent<CircularBarState>().AddState(-damage);
                }
                else if (element == "Freeze")
                {
                    if (barrInstantiate.GetComponent<CircularBarState>().stateValue < 0)
                    {
                        vida.Damage(damage);
                    }

                    barrInstantiate.GetComponent<CircularBarState>().AddState(+damage);
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.name != playerIndex.ToString())
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

            if (element != "nothing" && element != "Paralysis")
            {
                bool barrCreated = false;

                foreach (Transform hijo in playerAttacked.transform)
                {
                    if (hijo.name == stateBarr.name) barrCreated = true;
                }

                if (!barrCreated)
                {
                    barrInstantiate = Instantiate(stateBarr, playerAttacked.transform);
                    barrInstantiate.transform.position = playerAttacked.transform.position;
                }

                if (element == "Burn")
                {
                    if (barrInstantiate.GetComponent<CircularBarState>().stateValue > 0)
                    {
                        vida.Damage(damage);
                    }

                    barrInstantiate.GetComponent<CircularBarState>().AddState(-damage);
                }
                else if (element == "Freeze")
                {
                    if (barrInstantiate.GetComponent<CircularBarState>().stateValue < 0)
                    {
                        vida.Damage(damage);
                    }

                    barrInstantiate.GetComponent<CircularBarState>().AddState(+damage);
                }
            }

            Destroy(gameObject);
        }
        else if (!collision.gameObject.CompareTag("Player") && collision.gameObject.name != gameObject.name)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (trigger == true) return;

        trigger = true;
        GameObject parent = transform.parent.gameObject;

        foreach (Transform hijo in parent.transform)
        {
            if (hijo.CompareTag("Burn"))
            {
                transform.GetComponent<SpriteRenderer>().color = Color.orange;
                var main = transform.GetComponent<ParticleSystem>().main;
                main.startColor = Color.orange;

                brother = hijo.gameObject;

                element = "Burn";
            }
            else if (hijo.CompareTag("Freeze"))
            {
                transform.GetComponent<SpriteRenderer>().color = Color.blue;
                var main = transform.GetComponent<ParticleSystem>().main;
                main.startColor = Color.blue;

                brother = hijo.gameObject;

                element = "Freeze";
            }
            else if (hijo.CompareTag("Paralysis"))
            {
                transform.GetComponent<SpriteRenderer>().color = Color.yellow;
                var main = transform.GetComponent<ParticleSystem>().main;
                main.startColor = Color.yellow;

                brother = hijo.gameObject;

                element = "Paralysis";
            }
        }
    }
}