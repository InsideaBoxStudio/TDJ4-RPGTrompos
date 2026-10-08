using UnityEngine;
using System.Linq;

static class StatisticsParts
{
    static public float[][] heaviness = new float[2][]; // [playerIndex]
    static public int[][] health = new int[2][]; // [playerIndex]
    static public float[][] unstable = new float[2][]; // [playerIndex]
    static public int[][] maxEnergy = new int[2][]; // [playerIndex]
    static public int[][] initEnergy = new int[2][]; // [playerIndex]
    static public float[][] frictionStrength = new float[2][]; // [playerIndex]

    static public void TotalStatistics(int PlayerIndex)
    {
        PlayerSettings.heaviness[PlayerIndex] = heaviness[PlayerIndex].Sum();
        PlayerSettings.maxHealth[PlayerIndex] = health[PlayerIndex].Sum();
        PlayerSettings.maxWaitTime[PlayerIndex] = unstable[PlayerIndex].Sum();
        PlayerSettings.maxEnergy[PlayerIndex] = maxEnergy[PlayerIndex].Sum();
        PlayerSettings.initEnergy[PlayerIndex] = initEnergy[PlayerIndex].Sum();
        PlayerSettings.frictionStrength[PlayerIndex] = frictionStrength[PlayerIndex].Sum();
    }
}