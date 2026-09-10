using UnityEngine;

public class TimeBar : MonoBehaviour
{
    [SerializeField] private CheckPlayerTurn checkPlayerTurn;
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private float masHeight = 300f;
    [SerializeField] private float maxTime = 10f;

    private float currentTime = 0f;
    private float currentHeight = 0f;

    void Update()
    {
        currentTime = checkPlayerTurn.turnTime;

        if(currentTime > maxTime)
        {
            currentTime = maxTime;
        }

        currentHeight = masHeight / maxTime * currentTime;

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, currentHeight);
    }
}
