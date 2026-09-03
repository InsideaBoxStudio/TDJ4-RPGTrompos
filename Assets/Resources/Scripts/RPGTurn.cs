using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class RPGTurn : MonoBehaviour
{
    [SerializeField] private float syncWindow = 0.2f;
    private List<CheckPlayerTurn> activePlayers = new List<CheckPlayerTurn>();

    private List<CheckPlayerTurn> readyPlayers = new List<CheckPlayerTurn>();
    private float firstReadyTime;

    public void NotifyReady(CheckPlayerTurn player)
    {
        // Si este componente está en un objeto inactivo (ej: escena Practica, donde
        // el RPGTurn vive en un EventSystem desactivado), NO se pueden correr corrutinas.
        // En ese caso activamos el turno de inmediato. En 1VS1 (RPGTurn activo) se usa
        // la corrutina original con su ventana de sincronización -> comportamiento intacto.
        if (!isActiveAndEnabled)
        {
            NotifyReadyImmediate(player);
            return;
        }

        if (readyPlayers.Contains(player)) return;

        if (readyPlayers.Count == 0)
        {
            firstReadyTime = Time.time;
            readyPlayers.Add(player);
            StartCoroutine(CheckSync());
        }
        else
        {
            readyPlayers.Add(player);
        }
    }

    // Para la IA: activa el turno YA, sin corrutina ni ventana de sincronización.
    // Necesario porque el RPGTurn puede vivir en un objeto inactivo (no puede correr
    // corrutinas) y porque la IA no necesita sincronizarse con otro jugador humano.
    public void NotifyReadyImmediate(CheckPlayerTurn player)
    {
        if (activePlayers.Contains(player)) return;
        activePlayers.Add(player);
        player.Ready();
        Time.timeScale = 0f;
    }

    public bool PlayerFinished(CheckPlayerTurn player)
    {
        if (activePlayers.Contains(player))
        {
            activePlayers.Remove(player);
        }

        bool everyoneFinished = activePlayers.Count == 0;

        if (everyoneFinished)
        {
            Time.timeScale = 1f;
        }

        return everyoneFinished;
    }

    public bool AreAllPlayersFinished()
    {
        return activePlayers.Count == 0;
    }

    IEnumerator CheckSync()
    {
        yield return new WaitForSeconds(syncWindow);

        activePlayers.Clear();

        foreach (var p in readyPlayers)
        {
            activePlayers.Add(p);
            p.Ready();
        }

        readyPlayers.Clear();

        Time.timeScale = 0f;
    }
}