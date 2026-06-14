using UnityEngine;

public class Burned : MonoBehaviour
{
    private void ApplyBurn(GameObject player)
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("StateEffect"))
            {
                child.GetComponent<Burn>().ChangeParent(player, true);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ApplyBurn(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ApplyBurn(collision.gameObject);
    }
}
