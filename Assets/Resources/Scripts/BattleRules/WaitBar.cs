using UnityEngine;
using UnityEngine.UI;

public class WaitBar : MonoBehaviour
{
    [SerializeField] private CheckPlayerTurn checkPlayerTurn;
    [SerializeField] private Image waitBar;
    private float waitTime = 0f;
    private float maxWaitTime = 0f;

    void Start()
    {
        maxWaitTime = checkPlayerTurn.maxWaitTime;
        if (waitBar == null)
        {
            waitBar = gameObject.GetComponent<Image>();
        }
    }

    void Update()
    {
        waitTime = checkPlayerTurn.timeUntilNextTurn;

        waitTime = Mathf.Clamp(waitTime, 0f, maxWaitTime);

        // maxWaitTime = escala Y 1
        // 0 segundos = escala Y 0
        float scaleY = waitTime / maxWaitTime;

        if (waitBar == null)
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                scaleY,
                transform.localScale.z
            );
        }
        else
        {
            waitBar.fillAmount = waitTime / maxWaitTime;
        }
    }
}