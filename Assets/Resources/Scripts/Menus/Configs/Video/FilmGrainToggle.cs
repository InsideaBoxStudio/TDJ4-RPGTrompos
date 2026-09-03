using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FilmGrainToggle : MonoBehaviour
{
    [Header("UI")]
    public Toggle filmGrainToggle;

    [Header("Volume")]
    public Volume volume;

    private FilmGrain filmGrain;

    private const string FilmGrainKey = "FilmGrainEnabled";

    private void Start()
    {
        if (volume != null && volume.profile.TryGet(out filmGrain))
        {
            // Cargar valor guardado
            bool enabled = PlayerPrefs.GetInt(FilmGrainKey, 1) == 1;

            // Aplicar al Film Grain
            filmGrain.active = enabled;

            // Actualizar Toggle sin disparar el evento
            filmGrainToggle.SetIsOnWithoutNotify(enabled);

            // Detectar cambios del Toggle
            filmGrainToggle.onValueChanged.AddListener(SetFilmGrain);
        }
        else
        {
            Debug.LogWarning("No se encontró Film Grain en el Volume Profile.");
        }
    }

    public void SetFilmGrain(bool enabled)
    {
        if (filmGrain != null)
        {
            // Aplicar
            filmGrain.active = enabled;

            // Guardar
            PlayerPrefs.SetInt(FilmGrainKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void OnDestroy()
    {
        if (filmGrainToggle != null)
        {
            filmGrainToggle.onValueChanged.RemoveListener(SetFilmGrain);
        }
    }
}