using UnityEngine;
using UnityEngine.InputSystem;
static class PlayerSettings
{
    static public GameObject[] player = new GameObject[2]; // [playerIndex]
    static public Gamepad[] gamepad = new Gamepad[2]; // [playerIndex]

    // estadisticas y atributos del trompo
    static public Sprite[] layerSprite = new Sprite[2]; // [playerIndex]
    static public GameObject[][] attackOptionsPrefabs = new GameObject[2][]; // [playerIndex][optionPrefab]
    static public GameObject[][] specialOptionsPrefabs = new GameObject[2][]; // [playerIndex][optionPrefab]
    static public float[] heaviness = new float[2]; // [playerIndex]
    static public int[] maxHealth = new int[2]; // [playerIndex]
    static public float[] maxWaitTime = new float[2]; // [playerIndex]
    static public int[] maxEnergy = new int[2]; // [playerIndex]
    static public int[] initEnergy = new int[2]; // [playerIndex]
    static public float[] frictionStrength = new float[2]; // [playerIndex]
}