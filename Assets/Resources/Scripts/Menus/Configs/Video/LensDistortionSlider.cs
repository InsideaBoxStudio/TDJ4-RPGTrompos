using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LensDistortionSlider : MonoBehaviour
{
    [Header("UI")]
    public Slider intensitySlider;

    [Header("Volume")]
    public Volume volume;

    private LensDistortion lensDistortion;

    private const float MinIntensity = 0f;
    private const float MaxIntensity = 0.3f;

    private const string LensDistortionKey = "LensDistortionIntensity";

    private void Start()
    {
        // Obtener Lens Distortion del Volume Profile
        if (volume != null && volume.profile.TryGet(out lensDistortion))
        {
            // Configurar los límites del Slider
            intensitySlider.minValue = MinIntensity;
            intensitySlider.maxValue = MaxIntensity;

            // Cargar valor guardado
            float intensity = PlayerPrefs.GetFloat(
                LensDistortionKey,
                lensDistortion.intensity.value
            );

            // Asegurarse de que esté dentro del rango
            intensity = Mathf.Clamp(
                intensity,
                MinIntensity,
                MaxIntensity
            );

            // Aplicar al Lens Distortion
            lensDistortion.intensity.value = intensity;

            // Actualizar Slider sin disparar el evento
            intensitySlider.SetValueWithoutNotify(intensity);

            // Detectar cambios
            intensitySlider.onValueChanged.AddListener(SetIntensity);
        }
        else
        {
            Debug.LogWarning(
                "No se encontró Lens Distortion en el Volume Profile."
            );
        }
    }

    public void SetIntensity(float value)
    {
        if (lensDistortion != null)
        {
            // Limitar entre 0 y 0.3
            float intensity = Mathf.Clamp(
                value,
                MinIntensity,
                MaxIntensity
            );

            // Aplicar
            lensDistortion.intensity.value = intensity;

            // Guardar
            PlayerPrefs.SetFloat(
                LensDistortionKey,
                intensity
            );

            PlayerPrefs.Save();
        }
    }

    private void OnDestroy()
    {
        if (intensitySlider != null)
        {
            intensitySlider.onValueChanged.RemoveListener(SetIntensity);
        }
    }
}