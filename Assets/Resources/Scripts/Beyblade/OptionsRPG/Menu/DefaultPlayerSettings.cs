using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultPlayerSettings : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private Gamepad gamepad;

    [SerializeField] private Sprite layerSprite;
    [SerializeField] private GameObject[] attackOptionsPrefabs;
    [SerializeField] private GameObject[] specialOptionsPrefabs;
    [SerializeField] private float heaviness;
    [SerializeField] private int health;
    [SerializeField] private float maxWaitTime;
    [SerializeField] private int maxEnergy;
    [SerializeField] private int initEnergy;
    [SerializeField] private float frictionStrength;

    void Awake()
    {
        Debug.Log("Se esta ejecutando incluso estando desactivado");
        // --------------------------------------------------
        // GAMEPAD
        // --------------------------------------------------

        if (Gamepad.all.Count > playerIndex)
        {
            gamepad = Gamepad.all[playerIndex];
        }

        // --------------------------------------------------
        // GAMEPAD ARRAY
        // --------------------------------------------------

        if (PlayerSettings.gamepad == null)
        {
            PlayerSettings.gamepad = new Gamepad[playerIndex + 1];
        }
        else if (PlayerSettings.gamepad.Length <= playerIndex)
        {
            Array.Resize(
                ref PlayerSettings.gamepad,
                playerIndex + 1
            );
        }

        PlayerSettings.gamepad[playerIndex] = gamepad;


        // --------------------------------------------------
        // SPRITE ARRAY
        // --------------------------------------------------

        if (PlayerSettings.layerSprite == null)
        {
            PlayerSettings.layerSprite = new Sprite[playerIndex + 1];
        }
        else if (PlayerSettings.layerSprite.Length <= playerIndex)
        {
            Array.Resize(
                ref PlayerSettings.layerSprite,
                playerIndex + 1
            );
        }

        PlayerSettings.layerSprite[playerIndex] = layerSprite;


        // --------------------------------------------------
        // ATTACK OPTIONS
        // --------------------------------------------------

        if (PlayerSettings.attackOptionsPrefabs == null)
        {
            PlayerSettings.attackOptionsPrefabs =
                new GameObject[playerIndex + 1][];
        }
        else if (PlayerSettings.attackOptionsPrefabs.Length <= playerIndex)
        {
            Array.Resize(
                ref PlayerSettings.attackOptionsPrefabs,
                playerIndex + 1
            );
        }

        PlayerSettings.attackOptionsPrefabs[playerIndex] =
            attackOptionsPrefabs;


        // --------------------------------------------------
        // SPECIAL OPTIONS
        // --------------------------------------------------

        if (PlayerSettings.specialOptionsPrefabs == null)
        {
            PlayerSettings.specialOptionsPrefabs =
                new GameObject[playerIndex + 1][];
        }
        else if (PlayerSettings.specialOptionsPrefabs.Length <= playerIndex)
        {
            Array.Resize(
                ref PlayerSettings.specialOptionsPrefabs,
                playerIndex + 1
            );
        }

        PlayerSettings.specialOptionsPrefabs[playerIndex] =
            specialOptionsPrefabs;


        // --------------------------------------------------
        // STATS
        // --------------------------------------------------

        if (PlayerSettings.heaviness[playerIndex] == 0)
        {
            PlayerSettings.heaviness[playerIndex] = heaviness;
        }

        if (PlayerSettings.maxHealth[playerIndex] == 0)
        {
            PlayerSettings.maxHealth[playerIndex] = health;
        }

        if (PlayerSettings.maxWaitTime[playerIndex] == 0)
        {
            PlayerSettings.maxWaitTime[playerIndex] = maxWaitTime;
        }

        if (PlayerSettings.maxEnergy[playerIndex] == 0)
        {
            PlayerSettings.maxEnergy[playerIndex] = maxEnergy;
        }

        if (PlayerSettings.initEnergy[playerIndex] == 0)
        {
            PlayerSettings.initEnergy[playerIndex] = initEnergy;
        }

        if (PlayerSettings.frictionStrength[playerIndex] == 0)
        {
            PlayerSettings.frictionStrength[playerIndex] = frictionStrength;
        }
    }
}