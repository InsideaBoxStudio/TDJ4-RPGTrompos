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
        // Tiempo REAL, no escalado. >>> FIX TURNOS <<<
        // Durante un turno el juego corre con Time.timeScale = 0, y WaitForSeconds
        // se congela con él: si alguien ya estaba en turno, esta corrutina quedaba
        // colgada y la ventana de sincronización no cerraba nunca.
        yield return new WaitForSecondsRealtime(syncWindow);

        // OJO: acá NO va un activePlayers.Clear(). >>> FIX TURNOS <<<
        // La IA toma su turno por NotifyReadyImmediate, que la agrega a activePlayers.
        // El Clear() la borraba de la lista, y después PlayerFinished no la encontraba
        // -> everyoneFinished salía mal -> el timeScale y el reseteo del contador
        // quedaban desfasados. Ahora solo agregamos a quien falte.
        foreach (var p in readyPlayers)
        {
            if (p == null) continue;
            if (activePlayers.Contains(p)) continue; // ya estaba en turno

            activePlayers.Add(p);
            p.Ready();
        }

        readyPlayers.Clear();

        Time.timeScale = 0f;
    }
}