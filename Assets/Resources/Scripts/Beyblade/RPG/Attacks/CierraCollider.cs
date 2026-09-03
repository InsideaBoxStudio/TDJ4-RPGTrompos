using UnityEngine;
using UnityEngine.InputSystem;
public class CierraCollider : MonoBehaviour
{
    [SerializeField] private float colliderDuration = 1f;

    [Header("Stats")]
    [SerializeField] private float reboundPlayer = 3f;
    [SerializeField] private float reboundOtherPlayer = 6f;
    [SerializeField] private int damage = 3;
    [SerializeField] private float moveDuration = 2f;

    private GameObject player;
    private GameObject otherPlayer;
    public int playerID = 0;
    public bool hasCollided = false;
    private float timer;

    private Vector3 contactPoint;

    void Awake()
    {
        if (transform.parent.gameObject.layer == 3)
        {
            playerID = 0;
        }
        else
        {
            playerID = 1;
        }
        
        player = transform.parent.gameObject;
    }

    void Start()
    {
        timer = colliderDuration;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // verificar que sea un jugador 
        if (!collision.gameObject.CompareTag("Player")) return;
        
        // verificar que sea un jugador diferente al que lanzo el ataque
        if (hasCollided == false && collision.gameObject.layer != transform.parent.gameObject.layer)
        {
            contactPoint = collision.ClosestPoint(transform.position);
            otherPlayer = collision.gameObject;
            player.GetComponent<PausePlayer>().Pause(0f);
            otherPlayer.GetComponent<PausePlayer>().Pause(0f);
            hasCollided = true;
        }
    }

    void Update()
    {
        transform.Rotate(0, 0, 360 * Time.unscaledDeltaTime);

        if (hasCollided)
        {
            //hacer daño cada vez que se precione el boton
            if ((Gamepad.all.Count > playerID && Gamepad.all[playerID].buttonSouth.wasPressedThisFrame)
                || TeclasJugador.Esperar(playerID)) // >>> TECLADO <<<
            {
                otherPlayer.GetComponent<Vida>().Damage(damage, contactPoint);
            }

            timer -= Time.unscaledDeltaTime;

            if (timer <= 0f)
            {
                player.GetComponent<PausePlayer>().UnPause();
                otherPlayer.GetComponent<PausePlayer>().UnPause();
                player.GetComponent<CheckPlayerTurn>().PlayerChoseAnAction(moveDuration, 0f, false);

                //calcular rebote
                Vector2 direction = (otherPlayer.transform.position - player.transform.position).normalized;
                player.GetComponent<Rigidbody2D>().AddForce(direction * reboundPlayer, ForceMode2D.Impulse);
                otherPlayer.GetComponent<Rigidbody2D>().AddForce(-direction * reboundOtherPlayer, ForceMode2D.Impulse);

                Destroy(gameObject);
            }
        }
    }
}
