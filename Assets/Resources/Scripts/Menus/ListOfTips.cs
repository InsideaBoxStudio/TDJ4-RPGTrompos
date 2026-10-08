using UnityEngine;
using UnityEngine.InputSystem;

public class ListOfTips : MonoBehaviour
{
    [SerializeField] private GameObject[] tips;
    [SerializeField] private float nextTipTime = 5f;
    [SerializeField] private int currentTip = 0;
    [SerializeField] private int lastTip = -1;

    void Start()
    {
        InvokeRepeating("NextTip", nextTipTime, nextTipTime);
    }

    void Update()
    {
        if (Gamepad.all[0].leftShoulder.wasPressedThisFrame) // reemplazar por InputSystem
        {
            PreviousTip();
        }
        else if (Gamepad.all[0].rightShoulder.wasPressedThisFrame) // reemplazar por InputSystem
        {
            NextTip();
        }

        if (lastTip == currentTip) return;

        lastTip = currentTip;

        if (currentTip > tips.Length - 1)
        {
            currentTip = 0;
        }
        else if (currentTip < 0)
        {
            currentTip = tips.Length - 1;
        }

        ChangeTip();
    }

    private void PreviousTip()
    {
        currentTip --;
    }

    private void NextTip()
    {
        currentTip ++;
    }

    private void ChangeTip()
    {
        for (int i = 0; i < tips.Length; i++)
        {
            if (i == currentTip)
            {
                tips[i].SetActive(true);
            }
            else
            {
                tips[i].SetActive(false);
            }
        }
    }
}
