using UnityEngine;

public class Poisoned : MonoBehaviour
{
    private void ApplyPoison(GameObject player)
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Poison"))
            {
                child.GetComponent<Poison>().ChangeParent(player, true);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ApplyPoison(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ApplyPoison(collision.gameObject);
    }
}
