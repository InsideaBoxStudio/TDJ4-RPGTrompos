using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TopLayerStats
{
    public Sprite layerSprite;

    public List<GameObject> attackOptionsPrefabs;
    public List<GameObject> specialOptionsPrefabs;

    public float heaviness;
    public int health;
    public float maxWaitTime;
    public int initEnergy;
}

[System.Serializable]
public class DiscStats
{
    public float heaviness;
    public int health;
    public int maxEnergy;
    public int initEnergy;
}

[System.Serializable]
public class DriverStats
{
    public float frictionStrength;
    public float heaviness;
    public int health;
    public int maxEnergy;
    public float maxWaitTime;
}