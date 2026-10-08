using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class StatisticsUpdate : MonoBehaviour
{
    [SerializeField] private MultiplayerEventSystem multiplayerEventSystem;
    [SerializeField] private int PlayerIndex = 0;
    [SerializeField] private int PartType = 0;

    [Header("Estadisticas")]

    [SerializeField] private Sprite layerSprite;
    [SerializeField] private GameObject[] attackOptionsPrefabs;
    [SerializeField] private GameObject[] specialOptionsPrefabs;
    [SerializeField] private int health;
    [SerializeField] private int maxEnergy;
    [SerializeField] private int initEnergy;
    [SerializeField] private float unstable;
    [SerializeField] private float heaviness;
    [SerializeField] private float frictionStrength;

    private bool wasSelected = false;

    private void Update()
    {
        GameObject currentSelected = GetCurrentSelectedGameObject();

        bool isSelected = currentSelected == gameObject;

        // Se ejecuta solamente una vez cuando pasa a estar seleccionado
        if (isSelected && !wasSelected)
        {
            UpdateStatistics();
            //DebugPlayerData();
        }

        wasSelected = isSelected;
    }

    private GameObject GetCurrentSelectedGameObject()
    {
        // Si hay MultiplayerEventSystem asignado, usarlo
        if (multiplayerEventSystem != null)
        {
            return multiplayerEventSystem.currentSelectedGameObject;
        }

        // Si no hay MultiplayerEventSystem, usar el EventSystem normal
        if (EventSystem.current != null)
        {
            return EventSystem.current.currentSelectedGameObject;
        }

        return null;
    }

    private void UpdateStatistics()
    {
        EnsureArraySize(PlayerIndex, PartType);

        StatisticsParts.health[PlayerIndex][PartType] = health;
        StatisticsParts.maxEnergy[PlayerIndex][PartType] = maxEnergy;
        StatisticsParts.initEnergy[PlayerIndex][PartType] = initEnergy;
        StatisticsParts.unstable[PlayerIndex][PartType] = unstable;
        StatisticsParts.heaviness[PlayerIndex][PartType] = heaviness;
        StatisticsParts.frictionStrength[PlayerIndex][PartType] = frictionStrength;

        if (layerSprite != null)
        {
            PlayerSettings.layerSprite[PlayerIndex] = layerSprite;
        }

        if (attackOptionsPrefabs.Length != 0)
        {
            PlayerSettings.attackOptionsPrefabs[PlayerIndex] = attackOptionsPrefabs;
        }

        if (specialOptionsPrefabs.Length != 0)
        {
            PlayerSettings.specialOptionsPrefabs[PlayerIndex] = specialOptionsPrefabs;
        }

        StatisticsParts.TotalStatistics(PlayerIndex);
    }

    private void EnsureArraySize(int playerIndex, int partType)
    {
        int requiredSize = partType + 1;

        if (StatisticsParts.health[playerIndex] == null ||
            StatisticsParts.health[playerIndex].Length < requiredSize)
        {
            System.Array.Resize(ref StatisticsParts.health[playerIndex], requiredSize);
            System.Array.Resize(ref StatisticsParts.maxEnergy[playerIndex], requiredSize);
            System.Array.Resize(ref StatisticsParts.initEnergy[playerIndex], requiredSize);
            System.Array.Resize(ref StatisticsParts.unstable[playerIndex], requiredSize);
            System.Array.Resize(ref StatisticsParts.heaviness[playerIndex], requiredSize);
            System.Array.Resize(ref StatisticsParts.frictionStrength[playerIndex], requiredSize);
        }
    }

    private void DebugPlayerData()
    {
        Debug.Log(
            "========== PLAYER " + PlayerIndex + " ==========\n" +
            "PartType: " + PartType + "\n\n" +

            "--- Estadísticas ---\n" +
            "Health: " + StatisticsParts.health[PlayerIndex][PartType] + "\n" +
            "Max Energy: " + StatisticsParts.maxEnergy[PlayerIndex][PartType] + "\n" +
            "Init Energy: " + StatisticsParts.initEnergy[PlayerIndex][PartType] + "\n" +
            "Unstable: " + StatisticsParts.unstable[PlayerIndex][PartType] + "\n" +
            "Heaviness: " + StatisticsParts.heaviness[PlayerIndex][PartType] + "\n" +
            "Friction Strength: " + StatisticsParts.frictionStrength[PlayerIndex][PartType] + "\n\n" +

            "--- Player Settings ---\n" +
            "Layer Sprite: " +
            (PlayerSettings.layerSprite[PlayerIndex] != null
                ? PlayerSettings.layerSprite[PlayerIndex].name
                : "NULL") + "\n" +

            "Attack Options: " +
            (PlayerSettings.attackOptionsPrefabs[PlayerIndex] != null
                ? PlayerSettings.attackOptionsPrefabs[PlayerIndex].Length.ToString()
                : "NULL") + "\n" +

            "Special Options: " +
            (PlayerSettings.specialOptionsPrefabs[PlayerIndex] != null
                ? PlayerSettings.specialOptionsPrefabs[PlayerIndex].Length.ToString()
                : "NULL") +

            "\n================================"
        );
    }
}