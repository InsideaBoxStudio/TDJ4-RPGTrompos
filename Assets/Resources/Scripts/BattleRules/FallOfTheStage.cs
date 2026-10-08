using Unity.VisualScripting;
using UnityEngine;

public class FallOfTheStage : MonoBehaviour
{
    [SerializeField] private float radioStage;
    [SerializeField] private float radioGravity;
    [SerializeField] private float fallTime = 1f;
    [SerializeField] private GameObject[] player = new GameObject[2];

    private int fallPlayer = 0;
    private bool isFalled = false;
    private float distance;

    private void Start()
    {
        player = PlayerSettings.player;
    }

    private void Update()
    {
        for(int i = 0; i < player.Length; i++)
        {
            if (player[i] == null) continue;

            distance = Vector2.Distance(player[i].transform.position, gameObject.transform.position);
            
            FallPlayer(i);
        }
    }

    private void FallPlayer(int i)
    {
        if (distance > radioStage &&
            !isFalled
            )
        {
            isFalled = true;
            Debug.Log("el jugador " + i + "ha caido del escenario");
            fallPlayer = i;

            // quitar vida
            Vida vida = player[fallPlayer].GetComponent<Vida>();
            if (vida == null) return;

            vida.Damage(vida.vidaActual, player[fallPlayer].transform.position);
            player[fallPlayer].transform.localScale = new Vector3(0, 0, 0);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioStage);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioGravity);
    }
}