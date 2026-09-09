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
    public bool destroy = false;

    public bool pressNow = false;
    private float tiempo = 0;

    private void Awake()
    {
        Invoke("PressTime", pressTime);
        Invoke("EndTime", endTime);
    }

    void Update()
    {
        // mover indicador
        tiempo += Time.unscaledDeltaTime;
        float t = tiempo / endTime;

        indicator.transform.position = Vector3.Lerp(
            startPos.position,
            endPos.position,
            t
        );

        // Si el jugador presiona su boton (joystick o teclado: Q el J1, U el J2).
        // Antes hacia Gamepad.all[playerIndex] sin chequear que existiera:
        // con cero joysticks tiraba excepcion en cada frame.
        if (Controles.Esperar(playerIndex))
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

        if (destroy == true)
        {
            Destroy(gameObject);
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