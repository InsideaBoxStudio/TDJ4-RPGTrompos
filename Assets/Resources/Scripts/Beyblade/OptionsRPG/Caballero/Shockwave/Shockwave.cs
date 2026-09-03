using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class Shockwave : MonoBehaviour
{
    [SerializeField] private GameObject qteObject;
    [SerializeField] private GameObject player;
    public int playerIndex = 0;

    [Header("Estadisticas")]
    [SerializeField] private float radius = 1f;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float impulse = 10;

    private GameObject qteInstantiate;
    [SerializeField] private GameObject[] players;
    private GameObject camera;
    private PressButtomTime2 scriptQTE;
    private CircleRenderer circleRenderer;

    private bool trigger = true;
    private bool trigger2 = false;
    private bool end = false;

    private TopDownJumpScale jumpScript;

    private void Awake()
    {
        camera = GameObject.FindGameObjectWithTag("MainCamera");

        players = GameObject
            .FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .ToArray();

        player = players[playerIndex];


        foreach (Transform child in player.transform.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("PlayerSprite"))
            {
                jumpScript = child.GetComponent<TopDownJumpScale>();
                break;
            }
        }

        circleRenderer = transform.GetComponent<CircleRenderer>();
        transform.position = player.transform.position;
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;
        if (end) return;

        if (trigger)
        {
            for (int i = 0; i < players.Length; i++)
            {
                players[i].GetComponent<PausePlayer>().Pause(10f);
            }

            trigger = false; // gatillo para que no se repita mas de 1 vez

            qteInstantiate = Instantiate(qteObject);
            qteInstantiate.transform.SetParent(players[playerIndex].transform);
            qteInstantiate.transform.position = player.transform.position;
            scriptQTE = qteInstantiate.GetComponent<PressButtomTime2>();
            scriptQTE.playerIndex = playerIndex;

            trigger2 = true;
        }

        if (scriptQTE.ended && trigger2)
        {
            trigger2 = false; // gatillo para que no se repita mas de 1 vez

            for (int i = 0; i < players.Length; i++)
            {
                players[i].GetComponent<PausePlayer>().UnPause();
            }

            if (scriptQTE.success)
            {
                player.GetComponent<GravityToPoint>().enabled = false;
                jumpScript.StartJump(); // comenzar salto
            }
            else
            {
                End();
            }
        }

        if (scriptQTE == null && jumpScript.isTouchingFloor && !jumpScript.isJumping)
        {
            end = true;
            camera.GetComponent<Shake>().StartShake(0.3f, 0.1f);
            circleRenderer.radius = radius;
            circleRenderer.DrawCircle();
            Invoke("End", attackDuration);

            for (int i = 0; i < players.Length; i++)
            {
                if (i != playerIndex)
                {
                    float distanciaSqr = (players[i].transform.position - player.transform.position).sqrMagnitude;

                    Debug.Log("distanciaSqr " + distanciaSqr + " / distancia necesaria" + (radius + 0.16f));

                    if(distanciaSqr < radius + 0.16f)
                    {
                        players[i].GetComponent<Vida>().Damage(damage, players[i].transform.position);

                        // calcular direccion para impulsar
                        Vector2 direction = (players[i].transform.position - player.transform.position).normalized;

                        // impulsar al jugador atacante
                        Rigidbody2D rigidbody2D = players[i].GetComponent<Rigidbody2D>();
                        rigidbody2D.AddForce(direction * impulse, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    private void End()
    {
        player.GetComponent<GravityToPoint>().enabled = true;
        Destroy(gameObject);
    }
}
