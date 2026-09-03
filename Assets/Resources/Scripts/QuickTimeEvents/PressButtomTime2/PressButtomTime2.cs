using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PressButtomTime2 : MonoBehaviour
{
    [SerializeField] private GameObject indicator;
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;

    [Header("Momento para presionar")]
    [SerializeField] private float endTime = 1.0f;
    [SerializeField] private float pressTime = 0.5f;

    [Header("Resultado")]
    public int playerIndex = 0;
    public bool success = false;
    public bool ended = false;

    private bool pressNow = false;
    private float tiempo = 0;

    private void Awake()
    {
        Debug.Log(Time.timeScale);
        Invoke("PressTime", pressTime);
        Invoke("EndTime", endTime);
    }

    void Update()
    {
        // mover indicador
        tiempo += Time.deltaTime;
        float t = tiempo / endTime;

        indicator.transform.position = Vector3.Lerp(
            startPos.position,
            endPos.position,
            t
        );

        //si el jugador presiona su boton
        if (Gamepad.all[playerIndex].buttonSouth.wasPressedThisFrame)
        {
            if (pressNow == true) // si esta en tiempo de precionar
            {
                success = true;
            }
            else // si esta en tiempo de precionar
            {
                success = false;
            }

            EndTime();
        }
    }

    private void PressTime()
    {
        pressNow = true;
    }

    private void EndTime()
    {
        CancelInvoke("EndTime");
        CancelInvoke("PressTime");
        pressNow = false;
        ended = true;

        if (!success)
        {
            transform.GetComponent<Shake>().StartShake(0.1f, 0.2f);
        }

        Invoke("End", 0.2f);
    }

    private void End()
    {
        Destroy(gameObject);
    }
}
