using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessSlider : MonoBehaviour
{
    [Header("UI")]
    public Slider brightnessSlider;

    [Header("Volume")]
    public Volume volume;

    private ColorAdjustments colorAdjustments;

    private const float MinExposure = -1f;
    private const float MaxExposure = 1f;

    private const string BrightnessKey = "BrightnessExposure";

    private void Start()
    {
        // Obtener Color Adjustments del Volume Profile
        if (volume != null && volume.profile.TryGet(out colorAdjustments))
        {
            // Configurar los límites del Slider
            brightnessSlider.minValue = MinExposure;
            brightnessSlider.maxValue = MaxExposure;

            // Cargar valor guardado
            float exposure = PlayerPrefs.GetFloat(
                BrightnessKey,
                colorAdjustments.postExposure.value
            );

            // Asegurarse de que esté dentro del rango
            exposure = Mathf.Clamp(
                exposure,
                MinExposure,
                MaxExposure
            );

            // Aplicar exposición
            colorAdjustments.postExposure.value = exposure;

            // Actualizar Slider sin disparar el evento
            brightnessSlider.SetValueWithoutNotify(exposure);

            // Detectar cambios
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }
        else
        {
            Debug.LogWarning(
                "No se encontró Color Adjustments en el Volume Profile."
            );
        }
    }

    public void SetBrightness(float value)
    {
        if (colorAdjustments != null)
        {
            // Limitar entre -1 y 1
            float exposure = Mathf.Clamp(
                value,
                MinExposure,
                MaxExposure
            );

            // Aplicar exposición
            colorAdjustments.postExposure.value = exposure;

            // Guardar
            PlayerPrefs.SetFloat(
                BrightnessKey,
                exposure
            );

            PlayerPrefs.Save();
        }
    }

    private void OnDestroy()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
        }
    }
}