using UnityEngine;

public class Redirection : MonoBehaviour
{
    [SerializeField] private float attackTime = 1f;

    private Transform parent;
    private GameObject objective;
    public float Speed = 5f;

    public void ChangeParent(GameObject newParent)
    {
        transform.SetParent(newParent.transform);
        Init();
    }

    private void Init()
    {
        // Guardar padre
        parent = transform.parent;

        if (parent.CompareTag("Player"))
        {
            parent.GetComponent<CheckPlayerTurn>()
                  .PlayerChoseAnAction(attackTime, 2, true);
        }

        Invoke(nameof(Attack), attackTime);
    }

    private void Attack()
    {
        if (parent == null) return;
        gameObject.SetActive(false);

        // Buscar jugadores
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            // Elegir uno que NO esté en la misma layer
            if (player.layer != parent.gameObject.layer)
            {
                objective = player;
                break; // Toma el primero encontrado
            }
        }

        // Si encontró objetivo
        if (objective != null)
        {
            // Dirección hacia el objetivo
            Vector3 dir = (objective.transform.position - parent.position).normalized;

            // Ejemplo: mover al padre hacia el objetivo
            Rigidbody rb = parent.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = dir * Speed;
                Destroy(gameObject);
            }
        }
    }
}