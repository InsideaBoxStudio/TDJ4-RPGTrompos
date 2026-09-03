using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SoundVolume : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI volumeNum;
    private Button thisButton;

    private int volume = 100;

    private void Start()
    {
        thisButton = transform.GetComponent<Button>();

        volume = PlayerPrefs.GetInt("SoundVolume", 100);

        volumeNum.text = volume + "%";
    }
    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != thisButton.gameObject) return;
        // Antes hacia "if (Gamepad.current == null) return;": sin joystick no se
        // podia cambiar el volumen. Ahora tambien con las flechas del teclado.
        if (Controles.MenuIzquierda())
        {
            LowerVolume();
        }
        else if (Controles.MenuDerecha())
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
        PlayerPrefs.SetInt("SoundVolume", volume);
        PlayerPrefs.Save();
    }
}
