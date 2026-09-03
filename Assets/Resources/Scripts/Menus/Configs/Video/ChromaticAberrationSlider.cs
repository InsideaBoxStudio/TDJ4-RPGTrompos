using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationSlider : MonoBehaviour
{
    [Header("UI")]
    public Slider intensitySlider;

    [Header("Volume")]
    public Volume volume;

    private ChromaticAberration chromaticAberration;

    private const float MinIntensity = 0f;
    private const float MaxIntensity = 0.1f;

    private const string ChromaticAberrationKey = "ChromaticAberrationIntensity";

    private void Start()
    {
        // Obtener Chromatic Aberration del Volume Profile
        if (volume != null && volume.profile.TryGet(out chromaticAberration))
        {
            // Configurar los límites del Slider
            intensitySlider.minValue = MinIntensity;
            intensitySlider.maxValue = MaxIntensity;

            // Cargar valor guardado
            float intensity = PlayerPrefs.GetFloat(
                ChromaticAberrationKey,
                chromaticAberration.intensity.value
            );

            // Asegurarse de que esté dentro del rango
            intensity = Mathf.Clamp(
                intensity,
                MinIntensity,
                MaxIntensity
            );

            // Aplicar intensidad
            chromaticAberration.intensity.value = intensity;

            // Actualizar Slider sin disparar el evento
            intensitySlider.SetValueWithoutNotify(intensity);

            // Detectar cambios
            intensitySlider.onValueChanged.AddListener(SetIntensity);
        }
        else
        {
            Debug.LogWarning(
                "No se encontró Chromatic Aberration en el Volume Profile."
            );
        }
    }

    public void SetIntensity(float value)
    {
        if (chromaticAberration != null)
        {
            // Limitar entre 0 y 1
            float intensity = Mathf.Clamp(
                value,
                MinIntensity,
                MaxIntensity
            );

            // Aplicar
            chromaticAberration.intensity.value = intensity;

            // Guardar
            PlayerPrefs.SetFloat(
                ChromaticAberrationKey,
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