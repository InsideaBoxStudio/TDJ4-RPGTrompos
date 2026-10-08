using UnityEngine;

public class Paralyzed : MonoBehaviour
{
    private void ApplyParalysis(GameObject player)
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("StateEffect"))
            {
                child.GetComponent<Paralysis>().ChangeParent(player, true);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ApplyParalysis(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ApplyParalysis(collision.gameObject);
    }
}
