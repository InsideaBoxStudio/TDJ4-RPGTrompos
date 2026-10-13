using System;
using UnityEngine;

// ============================================================================
//  PRESET DE UN TROMPO DE LA CPU
//  ----------------------------------------------------------------------------
//  El trompo del jugador ("Base") no trae ni sus estadísticas ni sus ataques: los lee
//  de PlayerSettings, que la pantalla de selección de piezas llena con lo que el
//  jugador eligió. La CPU no pasa por esa pantalla, así que esto hace de "selección
//  automática": escribe en PlayerSettings, en la posición del jugador de la CPU, el
//  aspecto, las estadísticas y las listas de ataques de ESTE personaje (Ninja, Magus
//  o Caballero). Todo lo demás (Vida, EnergyCounter, GravityToPoint, ChangeSprite,
//  el menú de ataques...) lee de ahí y no se entera de que es la CPU.
//
//  Se pega en el contenedor de cada personaje de la CPU, junto al AIBrain. Los valores
//  son datos del Inspector: para balancear un personaje se cambian acá, sin tocar
//  código.
//
//  POR QUÉ CORRE ANTES QUE TODO: EnergyCounter, por ejemplo, lee la energía máxima en su
//  Awake. El orden entre Awakes de scripts distintos no está garantizado, así que este
//  se adelanta con DefaultExecutionOrder. Solo escribe datos estáticos: no toca el
//  tiempo ni el sistema de turnos.
// ============================================================================
[DefaultExecutionOrder(-200)]
public class PresetCPU : MonoBehaviour
{
    [Tooltip("Posición del jugador de la CPU en PlayerSettings (0 = Jugador 1, 1 = Jugador 2).")]
    [SerializeField] private int playerIndex = 1;

    [Header("Aspecto")]
    [SerializeField] private Sprite layerSprite;

    [Header("Ataques (los prefabs de Assets/Prefabs/UI/AttackOptions/P2Attacks)")]
    [SerializeField] private GameObject[] attackOptionsPrefabs;
    [SerializeField] private GameObject[] specialOptionsPrefabs;

    [Header("Estadísticas")]
    [SerializeField] private float heaviness = 8f;
    [SerializeField] private int health = 100;
    [SerializeField] private float maxWaitTime = 2.5f;
    [SerializeField] private int maxEnergy = 8;
    [SerializeField] private int initEnergy = 4;
    [SerializeField] private float frictionStrength = 3f;

    private void Awake()
    {
        Aplicar();

        // El teclado del jugador humano no le llega a la CPU (ver TeclasJugador.MarcarIA).
        TeclasJugador.MarcarIA(playerIndex, true);
    }

    // Se suelta al destruirse: en el 1 contra 1 que sigue, ese jugador vuelve a ser humano.
    private void OnDestroy()
    {
        TeclasJugador.MarcarIA(playerIndex, false);
    }

    // Pisa SIEMPRE lo que hubiera: PlayerSettings es estático y sobrevive de una pelea a
    // la otra, así que después de un 1 contra 1 todavía tendría las piezas del jugador 2.
    private void Aplicar()
    {
        Asegurar(ref PlayerSettings.layerSprite, playerIndex);
        Asegurar(ref PlayerSettings.attackOptionsPrefabs, playerIndex);
        Asegurar(ref PlayerSettings.specialOptionsPrefabs, playerIndex);
        Asegurar(ref PlayerSettings.heaviness, playerIndex);
        Asegurar(ref PlayerSettings.maxHealth, playerIndex);
        Asegurar(ref PlayerSettings.maxWaitTime, playerIndex);
        Asegurar(ref PlayerSettings.maxEnergy, playerIndex);
        Asegurar(ref PlayerSettings.initEnergy, playerIndex);
        Asegurar(ref PlayerSettings.frictionStrength, playerIndex);
        Asegurar(ref PlayerSettings.gamepad, playerIndex);

        PlayerSettings.layerSprite[playerIndex] = layerSprite;
        PlayerSettings.attackOptionsPrefabs[playerIndex] = attackOptionsPrefabs;
        PlayerSettings.specialOptionsPrefabs[playerIndex] = specialOptionsPrefabs;
        PlayerSettings.heaviness[playerIndex] = heaviness;
        PlayerSettings.maxHealth[playerIndex] = health;
        PlayerSettings.maxWaitTime[playerIndex] = maxWaitTime;
        PlayerSettings.maxEnergy[playerIndex] = maxEnergy;
        PlayerSettings.initEnergy[playerIndex] = initEnergy;
        PlayerSettings.frictionStrength[playerIndex] = frictionStrength;

        // La CPU no tiene joystick: que el menú de ataques no se ate al de otro jugador.
        PlayerSettings.gamepad[playerIndex] = null;
    }

    // Los arreglos de PlayerSettings arrancan con 2 posiciones, pero otros scripts los
    // pueden reemplazar por uno más corto (DefaultPlayerSettings) o nulo.
    private static void Asegurar<T>(ref T[] arreglo, int indice)
    {
        if (arreglo == null) arreglo = new T[indice + 1];
        else if (arreglo.Length <= indice) Array.Resize(ref arreglo, indice + 1);
    }
}
