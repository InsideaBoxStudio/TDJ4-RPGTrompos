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

    public void PlayerFinished(CheckPlayerTurn player)
    {
        if (activePlayers.Contains(player))
        {
            activePlayers.Remove(player);
        }

        // reanuda cuando TODOS terminaron
        if (activePlayers.Count == 0)
        {
            Time.timeScale = 1f;
        }
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