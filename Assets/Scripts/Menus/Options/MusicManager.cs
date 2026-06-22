using UnityEngine;
using UnityEngine.Rendering;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] musicSources;
    [SerializeField] private AudioSource[] soundSources;

    [SerializeField] private float generalPower = 100;
    [SerializeField] private float musicPower = 100;
    [SerializeField] private float soundPower = 100;

    // Update is called once per frame
    private void Awake()
    {
        musicPower = PlayerPrefs.GetInt("MusicVolume", 100);
        soundPower = PlayerPrefs.GetInt("SoundVolume", 100);
        generalPower = PlayerPrefs.GetInt("GeneralVolume", 100);

        float finalMusicVolume = (musicPower / 100f) * (generalPower / 100f);
        float finalSoundVolume = (soundPower / 100f) * (generalPower / 100f);

        for (int i = 0; i < musicSources.Length; i++)
        {
            musicSources[i].volume = finalMusicVolume;
        }

        for (int i = 0; i < soundSources.Length; i++)
        {
            soundSources[i].volume = finalSoundVolume;
        }
    }
}