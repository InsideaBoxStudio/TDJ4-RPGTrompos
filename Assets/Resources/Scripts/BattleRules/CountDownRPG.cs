using UnityEngine;
using TMPro;

public class CountDownRPG : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private float moveDuration = 5f;
    public float countDownTime = 10f; // tiempo antes de que se termine el turno
    private float initialCountDownTime = -1f; // -1 = todavía no se guardó

    public bool isCountingDown = false;

    void Awake()
    {
        GuardarTiempoInicial();
    }

    // Los trompos llaman a ResetCountDown desde SU Awake, que puede correr antes
    // que este (el orden entre Awakes no está garantizado). Si el valor inicial se
    // guardaba solo acá, ese Reset dejaba countDownTime en 0 para siempre, y con
    // 0 CheckPlayerTurn nunca da el turno: los trompos quedaban girando sin pelear.
    private void GuardarTiempoInicial()
    {
        if (initialCountDownTime < 0f) initialCountDownTime = countDownTime;
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
        GuardarTiempoInicial();
        isCountingDown = false;
        countDownText.text = "";
        countDownTime = initialCountDownTime;
    }
}
