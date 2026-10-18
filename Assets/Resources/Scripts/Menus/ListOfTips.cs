using UnityEngine;
using UnityEngine.InputSystem;

public class ListOfTips : MonoBehaviour
{
    [SerializeField] private GameObject[] tips;
    [SerializeField] private float nextTipTime = 5f;
    [SerializeField] private int currentTip = 0;
    [SerializeField] private int lastTip = -1;
    public InputActionReference L1Action;
    public InputActionReference R1Action;

    void Start()
    {
        Invoke("NextTip", nextTipTime);
    }

    void OnEnable()
    {
        if (L1Action == null || R1Action == null) return;
        L1Action.action.Enable();
        R1Action.action.Enable();
    }

    void Update()
    {
        if ( L1Action.action.WasPressedThisFrame() ||
             Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame ||
             Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) // reemplazar por InputSystem
        {
            PreviousTip();
        }
        else if ( R1Action.action.WasPressedThisFrame() ||
                  Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame ||
                  Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) // reemplazar por InputSystem
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
        CancelInvoke("NextTip");
        Invoke("NextTip", nextTipTime);
    }

    private void NextTip()
    {
        currentTip ++;
        CancelInvoke("NextTip");
        Invoke("NextTip", nextTipTime);
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
