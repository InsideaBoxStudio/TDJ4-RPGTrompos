using UnityEngine;

public class Frozen : MonoBehaviour
{
    private void ApplyFreeze(GameObject player)
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("StateEffect"))
            {
                child.GetComponent<Freeze>().ChangeParent(player, true);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ApplyFreeze(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ApplyFreeze(collision.gameObject);
    }
}
