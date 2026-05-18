using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;

public class EndGame : MonoBehaviour
{
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private GameObject[] Players;
    [SerializeField] private bool isPracticeMode = false;
    private bool[] playersDefeated;
    [SerializeField] private string sceneToLoad = "InitMenu";
    private bool gameFinished = false;
    private int playerID = 0;

    void Start()
    {
        // Eliminar jugadores inactivos de la lista
        Players = Players
            .Where(player => player != null && player.activeInHierarchy)
            .ToArray();
        playersDefeated = new bool[Players.Length];
    }

    void Update()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            playersDefeated[i] = Players[i].GetComponent<Vida>().vidaActual <= 0;
        }

        //si 1 o menos jugadores estan vivos terminar el juego
        if (!gameFinished && playersDefeated.Count(x => x) >= Players.Length - 1)
        {
            playerID = System.Array.IndexOf(playersDefeated, false); // obtener el índice del jugador ganador
            winText.text = $"Player {playerID + 1} Wins!";
            if (playerID == -1) {
                playerID = 0;
                winText.text = "It's a tie!";
            }
            if (isPracticeMode) playerID = 0;

            Time.timeScale = 0f;
            gameFinished = true;
            endGamePanel.SetActive(true);
        }

        if (!gameFinished) return;

        if (Gamepad.all[playerID].buttonSouth.wasPressedThisFrame)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
