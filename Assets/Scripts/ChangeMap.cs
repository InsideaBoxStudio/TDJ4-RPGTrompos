using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeMap : MonoBehaviour
{
    [SerializeField] private GameObject[] activeGameObject;
    [SerializeField] private string map = "elegir mapa";

    private bool isSceneChanged = false;
    public void ChangeScene()
    {
        for(int i = 0; i < activeGameObject.Length; i++)
        {
            activeGameObject[i].SetActive(true);
        }
        isSceneChanged = true;
    }

    void Update()
    {
        if (
            CharacterData.characterIndex1 != " " &&
            CharacterData.characterIndex2 != " " &&
            isSceneChanged
            )
        {
            SceneManager.LoadScene(map);
        }
    }
}
