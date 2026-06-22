using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TimingBar : MonoBehaviour
{
    [SerializeField] private EndGame endGame;
    [SerializeField] private CountDown countDown;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Vida vida;
    [SerializeField] private float barSpeed = 1f;
    [SerializeField] private int jugador = 0;
    [SerializeField] private bool isAI = false; // IA: frena la barra sola (no necesita joystick)

    public float distanceFromCenter = 2;

    private bool barStopped = false;
    private int countDownTime = 5;

    // El AIBrain usa esto para encontrar la barra de SU trompo (emparejando por Vida)
    // y para frenarla solo, sin que haya que tildar nada en el Inspector.
    public Vida GetVida() => vida;
    public void StopAsAI() { isAI = true; }

    void Update()
    {
        if (barStopped) return;

        // >>> IA: frena la barra automáticamente para tener vida sin joystick (borrá este bloque para quitar) >>>
        if (isAI)
        {
            barStopped = true;
            distanceFromCenter = 0.1f; // casi centrado => casi vida máxima
            if (vida != null) vida.WaitForInfo(distanceFromCenter);
            return;
        }
        // <<< FIN IA <<<

        if (countDown != null) countDownTime = countDown.countDownTime;

        if ((Gamepad.all.Count > jugador && Gamepad.all[jugador].buttonSouth.wasPressedThisFrame)
            || TeclasJugador.Esperar(jugador)) // >>> TECLADO <<< (Q frena la barra del J1)
        {
            barStopped = true;

            distanceFromCenter = Mathf.Abs(scrollbar.value - 0.5f) * 2f;

            vida.WaitForInfo(distanceFromCenter);
        }
        else if (countDownTime > 0)
        {
            float value = Mathf.PingPong(Time.unscaledTime * barSpeed, 1f);
            scrollbar.value = value;
        }
        else // si se acabo el tiempo y el jugador no lanzo su trompo.
        {
            barStopped = true;

            distanceFromCenter = 0.5f;

            vida.WaitForInfo(distanceFromCenter);
        }
    }
}