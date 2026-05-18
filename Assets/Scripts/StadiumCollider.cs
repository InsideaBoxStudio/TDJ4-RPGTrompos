using UnityEngine;

public class StadiumCollider : MonoBehaviour
{
    [SerializeField] private bool stadiumCauseDamaged = true;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!collision.gameObject.CompareTag("Player") && !stadiumCauseDamaged) return;

        Vida vida = collision.gameObject.GetComponent<Vida>();

        if (vida != null)
        {
            vida.Damage(2);
        }
    }
}
