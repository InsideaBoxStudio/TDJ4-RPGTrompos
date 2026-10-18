using UnityEngine;
using UnityEngine.InputSystem;

public class JoystickSpriteManager : MonoBehaviour
{
    public static JoystickSpriteManager Instance;

    [Header("PlayStation")]
    public Sprite psButtonSouth;
    public Sprite psButtonEast;
    public Sprite psButtonWest;
    public Sprite psButtonNorth;
    public Sprite psButtonL1;
    public Sprite psButtonR1;

    [Header("Xbox")]
    public Sprite xboxButtonSouth;
    public Sprite xboxButtonEast;
    public Sprite xboxButtonWest;
    public Sprite xboxButtonNorth;
    public Sprite xboxButtonL1;
    public Sprite xboxButtonR1;

    [Header("Nintendo")]
    public Sprite nintendoButtonSouth;
    public Sprite nintendoButtonEast;
    public Sprite nintendoButtonWest;
    public Sprite nintendoButtonNorth;
    public Sprite nintendoButtonL1;
    public Sprite nintendoButtonR1;

    [Header("Teclado")]
    public Sprite keyboardButtonSouth;
    public Sprite keyboardButtonEast;
    public Sprite keyboardButtonWest;
    public Sprite keyboardButtonNorth;
    public Sprite keyboardButtonL1;
    public Sprite keyboardButtonR1;

    [Header("Genérico")]
    public Sprite genericButtonSouth;
    public Sprite genericButtonEast;
    public Sprite genericButtonWest;
    public Sprite genericButtonNorth;
    public Sprite genericButtonL1;
    public Sprite genericButtonR1;

    public enum JoystickType
    {
        PlayStation,
        Xbox,
        Nintendo,
        Keyboard,
        Generic
    }

    private JoystickType[] playerJoystickTypes;
    private Gamepad[] playerGamepads;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DetectGamepads();
    }

    private void Update()
    {
        if (Gamepad.all.Count != playerGamepads.Length)
        {
            DetectGamepads();
        }
    }

    // =========================================================
    // DETECTAR CONTROLES
    // =========================================================

    public void DetectGamepads()
    {
        int playerCount = 2;

        playerJoystickTypes = new JoystickType[playerCount];
        playerGamepads = new Gamepad[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            if (i < Gamepad.all.Count)
            {
                Gamepad gamepad = Gamepad.all[i];

                playerGamepads[i] = gamepad;
                playerJoystickTypes[i] = DetectJoystickType(gamepad);
            }
            else
            {
                playerGamepads[i] = null;
                playerJoystickTypes[i] = JoystickType.Keyboard;
            }
        }
    }

    // =========================================================
    // DETECTAR TIPO DE GAMEPAD
    // =========================================================

    private JoystickType DetectJoystickType(Gamepad gamepad)
    {
        string name = gamepad.name.ToLower();

        if (name.Contains("dualshock") ||
            name.Contains("dualsense") ||
            name.Contains("playstation"))
        {
            return JoystickType.PlayStation;
        }

        if (name.Contains("xbox"))
        {
            return JoystickType.Xbox;
        }

        if (name.Contains("switch") ||
            name.Contains("nintendo"))
        {
            return JoystickType.Nintendo;
        }

        return JoystickType.Generic;
    }

    // =========================================================
    // BOTÓN SOUTH
    // =========================================================

    public Sprite GetButtonSouth(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return genericButtonSouth;

        switch (playerJoystickTypes[playerIndex])
        {
            case JoystickType.PlayStation:
                return psButtonSouth;
            case JoystickType.Xbox:
                return xboxButtonSouth;
            case JoystickType.Nintendo:
                return nintendoButtonSouth;
            case JoystickType.Keyboard:
                return keyboardButtonSouth;
            default:
                return genericButtonSouth;
        }
    }

    // =========================================================
    // BOTÓN EAST
    // =========================================================

    public Sprite GetButtonEast(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return genericButtonEast;

        switch (playerJoystickTypes[playerIndex])
        {
            case JoystickType.PlayStation:
                return psButtonEast;
            case JoystickType.Xbox:
                return xboxButtonEast;
            case JoystickType.Nintendo:
                return nintendoButtonEast;
            case JoystickType.Keyboard:
                return keyboardButtonEast;
            default:
                return genericButtonEast;
        }
    }

    // =========================================================
    // BOTÓN WEST
    // =========================================================

    public Sprite GetButtonWest(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return genericButtonWest;

        switch (playerJoystickTypes[playerIndex])
        {
            case JoystickType.PlayStation:
                return psButtonWest;
            case JoystickType.Xbox:
                return xboxButtonWest;
            case JoystickType.Nintendo:
                return nintendoButtonWest;
            case JoystickType.Keyboard:
                return keyboardButtonWest;
            default:
                return genericButtonWest;
        }
    }

    // =========================================================
    // BOTÓN NORTH
    // =========================================================

    public Sprite GetButtonNorth(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return genericButtonNorth;

        switch (playerJoystickTypes[playerIndex])
        {
            case JoystickType.PlayStation:
                return psButtonNorth;
            case JoystickType.Xbox:
                return xboxButtonNorth;
            case JoystickType.Nintendo:
                return nintendoButtonNorth;
            case JoystickType.Keyboard:
                return keyboardButtonNorth;
            default:
                return genericButtonNorth;
        }
    }

    // =========================================================
    // BOTÓN L1
    // =========================================================

    public Sprite GetButtonL1(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return genericButtonL1;

        switch (playerJoystickTypes[playerIndex])
        {
            case JoystickType.PlayStation:
                return psButtonL1;
            case JoystickType.Xbox:
                return xboxButtonL1;
            case JoystickType.Nintendo:
                return nintendoButtonL1;
            case JoystickType.Keyboard:
                return keyboardButtonL1;
            default:
                return genericButtonL1;
        }
    }

    // =========================================================
    // BOTÓN R1
    // =========================================================

    public Sprite GetButtonR1(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return genericButtonR1;

        switch (playerJoystickTypes[playerIndex])
        {
            case JoystickType.PlayStation:
                return psButtonR1;
            case JoystickType.Xbox:
                return xboxButtonR1;
            case JoystickType.Nintendo:
                return nintendoButtonR1;
            case JoystickType.Keyboard:
                return keyboardButtonR1;
            default:
                return genericButtonR1;
        }
    }

    // =========================================================
    // INFORMACIÓN
    // =========================================================

    public JoystickType GetJoystickType(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return JoystickType.Generic;

        return playerJoystickTypes[playerIndex];
    }

    public Gamepad GetGamepad(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return null;

        return playerGamepads[playerIndex];
    }

    public bool IsUsingKeyboard(int playerIndex)
    {
        if (!IsValidPlayer(playerIndex))
            return false;

        return playerJoystickTypes[playerIndex] == JoystickType.Keyboard;
    }

    private bool IsValidPlayer(int playerIndex)
    {
        return playerJoystickTypes != null &&
               playerIndex >= 0 &&
               playerIndex < playerJoystickTypes.Length;
    }
}