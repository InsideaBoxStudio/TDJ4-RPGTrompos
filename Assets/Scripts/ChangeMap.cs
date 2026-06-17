using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeMap : MonoBehaviour
{
    [SerializeField] private GameObject[] activeGameObject;
    [SerializeField] private string map = "elegir mapa";
    [SerializeField] private bool noSelected = false;

    private bool isSceneChanged = false;
    public void ChangeScene()
    {
        isSceneChanged = true;
        if (noSelected) return;

        for(int i = 0; i < activeGameObject.Length; i++)
        {
            activeGameObject[i].SetActive(true);
        }
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
}
