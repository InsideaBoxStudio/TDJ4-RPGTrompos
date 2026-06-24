using UnityEngine;

public class PressButtomTime : MonoBehaviour
{
    [SerializeField] private GameObject indicator;
    public bool isSuccess = false;
    private bool isActive = false;
    private float lerpTime = 0;
    private Vector3 endPosition;
    private Vector3 initPosition;
    private float currentTime = 0;
    public void InitEvent(float timeToEnd)
    {
        GameObject parent = gameObject.transform.parent.gameObject;
        isActive = true;
        indicator.GetComponent<Indicator>().playerIndex = int.Parse(parent.name);

        lerpTime = timeToEnd;
        currentTime = 0;

        initPosition = indicator.transform.localPosition;

        endPosition = new Vector3(
            indicator.transform.localPosition.x + 1,
            indicator.transform.localPosition.y,
            indicator.transform.localPosition.z
        );

        Invoke("EndEvent", timeToEnd);
    }

    void Update()
    {
        if (!isActive) return;

        currentTime += Time.deltaTime;

        if(indicator != null)
        {
            indicator.transform.localPosition = Vector3.Lerp(
                initPosition,
                endPosition,
                currentTime / lerpTime
            );
        }
        else
        {
            EndEvent();
        }
    }

    public void EndEvent()
    {
        Destroy(gameObject);
    }
}
