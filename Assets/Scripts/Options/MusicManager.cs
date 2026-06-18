using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] musicSources;
    [SerializeField] private AudioSource[] soundSources;
    [SerializeField] private float MusicPower = 100;
    [SerializeField] private float soundPower = 100;

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < musicSources.Length; i++)
        {
            musicSources[i].volume = MusicPower / 100;
        }
        for (int i = 0; i < musicSources.Length; i++)
        {
            musicSources[i].volume = soundPower / 100;
        }
    }
}