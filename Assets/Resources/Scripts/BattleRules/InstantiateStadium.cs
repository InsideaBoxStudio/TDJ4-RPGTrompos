using UnityEngine;

public class InstantiateStadium : MonoBehaviour
{
    void Awake()
    {
        if (PlayerSettings.stadium != null)
        {
            Instantiate(PlayerSettings.stadium, gameObject.transform);
        }
    }
}