using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    [SerializeField] private string map = "InitMenu";

    private void Awake()
    {
        Invoke("ChangeMap", 5f);
    }

    private void ChangeMap()
    {
        SceneManager.LoadScene(map);
    }
}