using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TimingBar : MonoBehaviour
{
    [SerializeField] private CountDown countDown;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Vida vida;
    [SerializeField] private float barSpeed = 1f;
    [SerializeField] private int jugador = 0;

    public float distanceFromCenter = 2;

    private bool barStopped = false;
    private int countDownTime = 5;
    
    void Update()
    {
        countDownTime = countDown.countDownTime;

        if (barStopped) return;

        if (Gamepad.all.Count > jugador && Gamepad.all[jugador].buttonSouth.wasPressedThisFrame)
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
        else
        {
            barStopped = true;
            vida.WaitForInfo(2);
        }
    }
}