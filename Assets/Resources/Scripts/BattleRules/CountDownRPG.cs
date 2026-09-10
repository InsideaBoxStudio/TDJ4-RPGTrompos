using UnityEngine;
using TMPro;

public class CountDownRPG : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private float moveDuration = 5f;
    public float countDownTime = 10f; // tiempo antes de que se termine el turno
    private float initialCountDownTime;

    public bool isCountingDown = false;

    void Awake()
    {
        initialCountDownTime = countDownTime;
    }

    void Update()
    {
        if (!isCountingDown) return;
        if (countDownTime > 0f)
        {
            countDownText.text = Mathf.Ceil(countDownTime).ToString();
            countDownTime -= Time.unscaledDeltaTime;
        }
        else
        {
            rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);
            ResetCountDown();
        }
    }

    public void ResetCountDown()
    {
        isCountingDown = false;
        countDownText.text = "";
        countDownTime = initialCountDownTime;
    }
}
