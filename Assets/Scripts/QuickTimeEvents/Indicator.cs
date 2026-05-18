using UnityEngine;
using UnityEngine.InputSystem;

public class Indicator : MonoBehaviour
{
    [SerializeField] private PressButtomTime pressButtomTime;
    [SerializeField] private GameObject zone;
    public int playerIndex = 0;
    private bool isTrigger = false;

    void Awake()
    {
        isTrigger = false;
    }

    void Update()
    {
        if (Gamepad.all[playerIndex].buttonSouth.wasPressedThisFrame)
        {
            if (isTrigger)
            {
                pressButtomTime.isSuccess = true;
            }
            Destroy(gameObject);
        }
        else
        {
            if ((Vector2.Distance(transform.position, zone.transform.position) <= (zone.transform.localScale.x / 2)) && !isTrigger)
            {
                isTrigger = true;
                Debug.Log(isTrigger);
            }
            else if (isTrigger && (Vector2.Distance(transform.position, zone.transform.position) >= (zone.transform.localScale.x / 2)))
            {
                isTrigger = false;
                Debug.Log(isTrigger);
            }
        }
    }
}
