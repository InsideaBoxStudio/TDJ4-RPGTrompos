using UnityEngine;

public class EnergyObject : MonoBehaviour
{
    [SerializeField] private int addEnergy = 5;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player")) return;

        EnergyCounter energyCounter = collider.gameObject.GetComponent<DefPlayerID>().energyCounter;
        if (energyCounter == null) return;

        energyCounter.ChangeEnergy(addEnergy, "Nothing");
        
        Destroy(gameObject);
    }
}
