using UnityEngine;

public class WaitBar : MonoBehaviour
{
    [SerializeField] private CheckPlayerTurn checkPlayerTurn;
    private float waitTime = 0f;

    void Update()
    {
        waitTime = checkPlayerTurn.timeUntilNextTurn;

        waitTime = Mathf.Clamp(waitTime, 0f, 5f);

        // 5 segundos = escala Y 1
        // 0 segundos = escala Y 0
        float scaleY = waitTime / 5f;

        transform.localScale = new Vector3(
            transform.localScale.x,
            scaleY,
            transform.localScale.z
        );
    }
}