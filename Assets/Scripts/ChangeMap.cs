using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeMap : MonoBehaviour
{
    [SerializeField] private string map = "elegir mapa";
    public void ChangeScene()
    {
        SceneManager.LoadScene(map);
    }
}
