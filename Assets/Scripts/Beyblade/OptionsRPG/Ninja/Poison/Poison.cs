using Unity.VisualScripting;
using UnityEngine;

public class Poison : MonoBehaviour
{
    [SerializeField] private int damagePerSecond = 2;
    [SerializeField] private int lifeDuration = 5;
    [SerializeField] private GameObject QuickTimeEvent;

    public bool isActive = false;
    private bool endEvent = false;
    private bool isSuccess = false;
    private bool initQuickTime = false;
    private float endQuickTime = 1f;
    private GameObject quickTimeEvent;
    private GameObject parent;
    void Awake()
    {
        //pausar todos los tag player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player == null) return;
            player.GetComponent<PausePlayer>().Pause(0);
        }

        parent = gameObject.transform.parent.gameObject;
        quickTimeEvent = Instantiate(QuickTimeEvent, parent.transform);
        quickTimeEvent.GetComponent<PressButtomTime>().InitEvent(endQuickTime);
        initQuickTime = true;
        Invoke("EndEvent", endQuickTime);
    }

    private void Life()
    {
        lifeDuration--;

        if (!isActive) return;

        //si el objeto padre tiene un tag "Player"
        if (transform.parent != null && transform.parent.CompareTag("Player"))
        {
            GameObject player = transform.parent.gameObject;
            Vida vida = player.GetComponent<Vida>();

            if (vida == null) return;

            vida.Damage(damagePerSecond, player.transform.position);
        }

        if (lifeDuration <= 0)
        {
            CancelInvoke("Life");
            Destroy(gameObject);
        }
    }

    public void ChangeParent(GameObject newParent, bool activePoison)
    {
        //cambiar padre del objeto
        if (isActive) return;
        transform.SetParent(newParent.transform);
        isActive = activePoison;
    }

    private void Update()
    {
        if (initQuickTime)
        {
            if (!endEvent && quickTimeEvent != null)
            {
                isSuccess = quickTimeEvent.GetComponent<PressButtomTime>().isSuccess;
                if (isSuccess) endEvent = true;
            }

            else
            {
                //despausar todos los tag player
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                foreach (GameObject player in players)
                {
                    if (player == null) return;
                    player.GetComponent<PausePlayer>().UnPause();
                    CheckPlayerTurn checkPlayerTurn = player.GetComponent<CheckPlayerTurn>();
                    if (checkPlayerTurn != null) {
                        player.GetComponent<CheckPlayerTurn>().PlayerChoseAnAction(1f, 3, false);
                    }
                }

                if (!isSuccess)
                {
                    isActive = true; //activa el veneno dañando al jugador que lo uso
                    parent.GetComponent<CheckPlayerTurn>().PlayerChoseAnAction(2f, 2, false);
                }
                else
                {
                    parent.GetComponent<CheckPlayerTurn>().PlayerChoseAnAction(0.1f, 1000f, false);
                }

                //comenzar cronometro
                InvokeRepeating("Life", 0f, 1f);

                initQuickTime = false;
            }
        }
    }

    private void EndEvent()
    {
        endEvent = true;
    }
}
