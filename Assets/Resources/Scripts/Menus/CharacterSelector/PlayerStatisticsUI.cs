using UnityEngine;
using UnityEngine.UI;

public class PlayerStatisticsUI : MonoBehaviour
{
    public int PlayerIndex = 0;
    public Image layerSprite;
    public Slider health;
    public int maxEnergy = 0;
    public int initEnergy = 0;
    public Slider unstable;
    public Slider heaviness;
    public Slider frictionStrength;
    
    [SerializeField] private GameObject energyObject;

    private void Update()
    {
        layerSprite.sprite = PlayerSettings.layerSprite[PlayerIndex];
        health.value = PlayerSettings.maxHealth[PlayerIndex];
        maxEnergy = PlayerSettings.maxEnergy[PlayerIndex];
        initEnergy = PlayerSettings.initEnergy[PlayerIndex];
        unstable.value = PlayerSettings.maxWaitTime[PlayerIndex];
        heaviness.value = PlayerSettings.heaviness[PlayerIndex];
        frictionStrength.value = PlayerSettings.frictionStrength[PlayerIndex];
    }
}