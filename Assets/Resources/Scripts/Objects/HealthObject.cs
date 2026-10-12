using UnityEngine;

public class HealthObject : MonoBehaviour
{
    // Cuánta vida da. Se usa el VALOR ABSOLUTO: antes este objeto curaba con un valor
    // NEGATIVO (daño negativo) y en la escena J1vsCPU quedó guardado como -25. Así
    // sigue andando con ese valor guardado y también si alguien lo pone en positivo.
    [SerializeField] private int addHealth = 25;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag("Player")) return;

        Vida vida = collider.gameObject.GetComponent<Vida>();
        if (vida == null) return;

        // Si el trompo ya tiene la vida llena, el objeto no se gasta: queda para
        // cuando haga falta.
        if (!vida.Heal(Mathf.Abs(addHealth))) return;

        Destroy(gameObject);
    }
}
