using UnityEngine;
using UnityEngine.SceneManagement;

public class StadiumSelector : MonoBehaviour
{
    [SerializeField] private GameObject stadiumPrefab;

    public void SelectStadium()
    {
        SceneManager.LoadScene(PlayerSettings.gameMode);
    }
}
