using UnityEngine;
using UnityEngine.SceneManagement;

public class StadiumSelector : MonoBehaviour
{
    [SerializeField] private GameObject stadiumPrefab;

    public void SelectStadium()
    {
        // Sin esto la pelea nunca se enteraba del estadio elegido. Un botón sin
        // prefab deja null y la pelea usa el muro que trae la escena.
        PlayerSettings.stadium = stadiumPrefab;
        SceneManager.LoadScene(PlayerSettings.gameMode);
    }
}
