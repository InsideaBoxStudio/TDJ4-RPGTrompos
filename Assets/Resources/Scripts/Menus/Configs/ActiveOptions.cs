using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActiveOptions : MonoBehaviour
{
    public Button[] buttons;
    public GameObject[] configList;

    public GameObject buttonSelected;

    void Update()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (configList[i] != buttonSelected)
            {
                configList[i].SetActive(false);
            }

            if (EventSystem.current.currentSelectedGameObject == buttons[i].gameObject)
            {
                buttonSelected = configList[i];
                configList[i].SetActive(true);
            }
        }
    }

    public void SelectedOption()
    {
        if (buttonSelected == null)
            return;

        Transform target = buttonSelected.transform.GetChild(0).GetChild(0);

        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }
}