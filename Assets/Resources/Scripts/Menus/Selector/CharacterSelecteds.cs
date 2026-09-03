using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelecteds : MonoBehaviour
{
    [SerializeField] string sceneName;
    void Update()
    {
        if (
            CharacterData.characterIndex1 != " " &&
            CharacterData.characterIndex2 != " "
            )
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
