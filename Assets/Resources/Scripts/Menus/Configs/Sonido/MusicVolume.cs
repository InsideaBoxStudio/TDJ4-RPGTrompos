using UnityEngine;
using UnityEngine.UI;

public class MusicVolume : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private int volume = 100;

    private void Start()
    {
        // Cargar valor guardado
        volume = PlayerPrefs.GetInt("MusicVolume", 100);

        // Configurar Slider
        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 100;

        // Mostrar valor guardado en el Slider
        volumeSlider.SetValueWithoutNotify(volume);

        // Escuchar cambios del Slider
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float value)
    {
        volume = Mathf.RoundToInt(value);

        // Guardar valor
        PlayerPrefs.SetInt("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
        }
    }
}