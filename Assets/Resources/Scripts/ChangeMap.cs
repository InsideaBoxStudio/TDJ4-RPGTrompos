using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeMap : MonoBehaviour
{
    [SerializeField] private GameObject[] activeGameObject;
    [SerializeField] private string map = "elegir mapa";
    [SerializeField] private bool noSelected = false;
    [SerializeField] private float wait = 0.2f;

    private bool isSceneChanged = false;
    public void ChangeScene()
    {
        Invoke("WaitScene", wait);
        if (noSelected) return;

        for(int i = 0; i < activeGameObject.Length; i++)
        {
            activeGameObject[i].SetActive(true);
        }
    }

    private void Start()
    {
        Time.timeScale = 1;
    }

    void Update()
    {
        if (
            CharacterData.characterIndex1 != " " &&
            CharacterData.characterIndex2 != " " &&
            isSceneChanged
            || noSelected && isSceneChanged)
        {
            SceneManager.LoadScene(map);
        }
    }

    private void WaitScene()
    {
        isSceneChanged = true;
    }
}
