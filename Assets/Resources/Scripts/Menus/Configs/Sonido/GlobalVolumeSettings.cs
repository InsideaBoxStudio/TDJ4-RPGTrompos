using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeSettings : MonoBehaviour
{
    private const string VolumeWeightKey = "VolumeWeight";
    private const string FilmGrainKey = "FilmGrainEnabled";
    private const string LensDistortionKey = "LensDistortionIntensity";
    private const string ChromaticAberrationKey = "ChromaticAberrationIntensity";

    private const float MinLensDistortion = 0f;
    private const float MaxLensDistortion = 0.3f;

    private const float MinChromaticAberration = 0f;
    private const float MaxChromaticAberration = 0.1f;

    private void Start()
    {
        Volume volume = GetComponent<Volume>();

        if (volume == null)
        {
            Debug.LogWarning("No se encontró un componente Volume.");
            return;
        }

        // ==========================================
        // VOLUME WEIGHT
        // ==========================================

        float weight = PlayerPrefs.GetFloat(
            VolumeWeightKey,
            volume.weight
        );

        volume.weight = Mathf.Clamp01(weight);


        // ==========================================
        // FILM GRAIN
        // ==========================================

        if (volume.profile.TryGet(out FilmGrain filmGrain))
        {
            bool filmGrainEnabled = PlayerPrefs.GetInt(
                FilmGrainKey,
                1
            ) == 1;

            filmGrain.active = filmGrainEnabled;
        }


        // ==========================================
        // LENS DISTORTION
        // ==========================================

        if (volume.profile.TryGet(out LensDistortion lensDistortion))
        {
            float intensity = PlayerPrefs.GetFloat(
                LensDistortionKey,
                lensDistortion.intensity.value
            );

            intensity = Mathf.Clamp(
                intensity,
                MinLensDistortion,
                MaxLensDistortion
            );

            lensDistortion.intensity.value = intensity;
        }


        // ==========================================
        // CHROMATIC ABERRATION
        // ==========================================

        if (volume.profile.TryGet(out ChromaticAberration chromaticAberration))
        {
            float intensity = PlayerPrefs.GetFloat(
                ChromaticAberrationKey,
                chromaticAberration.intensity.value
            );

            intensity = Mathf.Clamp(
                intensity,
                MinChromaticAberration,
                MaxChromaticAberration
            );

            chromaticAberration.intensity.value = intensity;
        }
    }
}