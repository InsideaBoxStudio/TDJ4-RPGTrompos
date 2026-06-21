using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GeneralVolume : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI volumeNum;
    private Button thisButton;

    private int volume = 100;

    private void Start()
    {
        thisButton = transform.GetComponent<Button>();

        volume = PlayerPrefs.GetInt("GeneralVolume", 100);

        volumeNum.text = volume + "%";
    }
    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != thisButton.gameObject) return;
        if (Gamepad.current == null) return;

        if (Gamepad.all[0].dpad.left.wasPressedThisFrame)
        {
            LowerVolume();
        }
        else if (Gamepad.all[0].dpad.right.wasPressedThisFrame)
        {
            IncreaseVolume();
        }
    }

    public void IncreaseVolume()
    {
        volume += 5;
        SaveVolume();
    }

    private void LowerVolume()
    {
        volume -= 5;
        SaveVolume();
    }

    private void SaveVolume()
    {
        if (volume < 0) volume = 0;
        else if (volume > 100) volume = 100;

        volumeNum.text = volume.ToString() + "%";
        PlayerPrefs.SetInt("GeneralVolume", volume);
        PlayerPrefs.Save();
    }
}
