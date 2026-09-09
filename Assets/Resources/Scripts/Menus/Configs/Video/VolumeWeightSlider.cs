using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class VolumeWeightSlider : MonoBehaviour
{
    [Header("UI")]
    public Slider weightSlider;

    [Header("Volume")]
    public Volume volume;

    private const string VolumeWeightKey = "VolumeWeight";

    private void Start()
    {
        if (volume != null)
        {
            // Configurar los límites del Slider
            weightSlider.minValue = 0f;
            weightSlider.maxValue = 1f;

            // Cargar el valor guardado
            float savedWeight = PlayerPrefs.GetFloat(VolumeWeightKey, volume.weight);

            // Asegurarse de que esté entre 0 y 1
            savedWeight = Mathf.Clamp01(savedWeight);

            // Aplicar el valor al Volume
            volume.weight = savedWeight;

            // Mostrar el valor en el Slider
            weightSlider.SetValueWithoutNotify(savedWeight);

            // Detectar cambios en el Slider
            weightSlider.onValueChanged.AddListener(SetVolumeWeight);
        }
        else
        {
            Debug.LogWarning("No se asignó ningún Volume.");
        }
    }

    public void SetVolumeWeight(float value)
    {
        if (volume != null)
        {
            // Limitar entre 0 y 1
            float weight = Mathf.Clamp01(value);

            // Aplicar al Volume
            volume.weight = weight;

            // Guardar
            PlayerPrefs.SetFloat(VolumeWeightKey, weight);
            PlayerPrefs.Save();
        }
    }

    private void OnDestroy()
    {
        if (weightSlider != null)
        {
            weightSlider.onValueChanged.RemoveListener(SetVolumeWeight);
        }
    }
}