using System.Linq;
using UnityEngine;

public class SwordBlow : MonoBehaviour
{
    [SerializeField] private GameObject QuickTimeEvent;
    [SerializeField] private SwordCollider swordCollider;
    [SerializeField] private GameObject sword;
    [SerializeField] private Transform StartPosition;
    [SerializeField] private Transform EndPosition;
    [SerializeField] public int playerIndex = 0;

    [Header("Estadisticas")]
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private int damageBase = 5;
    [SerializeField] private Vector3 incrementScale = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] private int incrementDamage = 1;
    [SerializeField] private int repeating = 3;
    [SerializeField] private float impulse = 5;

    private bool trigger = false;
    private bool end = false;
    private GameObject QTE;
    private PressButtomTime2 scriptQTE;
    private GameObject[] players;

    private float tiempo = 0;
    private bool pauseTrigger = false;

    private int TURN_ID = 0;

    private void Awake()
    {
        TURN_ID = TimeScaleController.Instance.GetMyNumberTurn();

        players = GameObject
            .FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .ToArray();

        swordCollider.attackingPlayer = players[playerIndex];
        swordCollider.impactImpulse = impulse;
    }

    private void Update()
    {
        if (end)
        {
            // mover espada;
            Debug.Log("Mover la espada");
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / attackDuration);

            sword.transform.rotation = Quaternion.Lerp(
                StartPosition.rotation,
                EndPosition.rotation,
                t
            );
        }

        if (!TimeScaleController.Instance.IsMyTurn(TURN_ID)) return;

        if (!pauseTrigger)
        {
            /* cambiamos el sistema de pausar
            for (int i = 0; i < players.Length; i++) // busca a todos los jugadores
            {
                players[i].GetComponent<PausePlayer>().Pause(10f); // pausa a todos los jugadores
            }
            */
            pauseTrigger = true;
        }

        if (!end) // si se esta ejecutando los Quick Time Event
        {
            if (QTE == null) // creamos un Quick Time Event
            {
                QTE = Instantiate(QuickTimeEvent, players[playerIndex].transform);
                QTE.transform.SetParent(players[playerIndex].transform);
                scriptQTE = QTE.GetComponent<PressButtomTime2>();
                scriptQTE.playerIndex = playerIndex;
                trigger = true;
            }
            else
            {
                if (scriptQTE.ended && trigger) // esperamos a que termine y nos de un resultado
                {
                    trigger = false; // gatillo para que no se repita mas de 1 vez
                    repeating -= 1;

                    if (scriptQTE.success && repeating != 0) // si el jugador acerto el Quick Time Event
                    {
                        gameObject.transform.localScale += incrementScale; // incrementar estadisticas del ataque
                        damageBase += incrementDamage;
                        swordCollider.damage = damageBase;
                    }
                    else if (repeating == 0)
                    {
                        gameObject.transform.localScale += incrementScale; // incrementar estadisticas del ataque
                        damageBase += incrementDamage;
                        swordCollider.damage = damageBase;

                        end = true; // terminar Quick Time Events
                        /* 
                        for (int i = 0; i < players.Length; i++)
                        {
                            players[i].GetComponent<PausePlayer>().UnPause();
                        }
                        */
                        TimeScaleController.Instance.EndTurn(TURN_ID);
                        Invoke("End", attackDuration * 1.2f);
                    }
                    else
                    {
                        end = true; // terminar Quick Time Events
                        /* 
                        for (int i = 0; i < players.Length; i++)
                        {
                            players[i].GetComponent<PausePlayer>().UnPause();
                        }
                        */
                        
                        TimeScaleController.Instance.EndTurn(TURN_ID);
                        Invoke("End", attackDuration * 1.2f);
                    }
                }
            }
        }

    }

    private void End()
    {
        Destroy(gameObject);
    }
}
