using UnityEngine;

public class DefPlayerID : MonoBehaviour
{
    [SerializeField] private int playerID = 0;
    public GameObject spriteObject;
    public Transform PointerArrow;
    public EnergyCounter energyCounter;
    public Transform pointerPosition;

    void Awake()
    {
        PlayerSettings.player[playerID] = gameObject;
    }
}
