using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    [SerializeField] private EnergyCounter energyCounter;
    [Header("Stats")]
    [SerializeField] private int energyGain = 2;
    [SerializeField] private int damage = 3;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // verificar que sea un jugador 
        if (!collision.gameObject.CompareTag("Player")) return;
        
        // verificar que sea un jugador diferente al que lanzo el ataque
        if (collision.gameObject.layer != transform.parent.gameObject.layer)
        {
            energyCounter.ChangeEnergy(energyGain, "GainEnergy");

            Vector3 contactPoint = collision.GetContact(0).point;
            collision.gameObject.GetComponent<Vida>().Damage(damage, contactPoint);
        }
    }
}