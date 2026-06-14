using UnityEngine;

public class ActiveCharacter : MonoBehaviour
{
    [SerializeField] private bool isPlayer1 = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (isPlayer1)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name != CharacterData.characterIndex1) continue;
                transform.GetChild(i).gameObject.SetActive(true);
                Debug.Log(CharacterData.characterIndex1);
            }
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name != CharacterData.characterIndex2) continue;
                transform.GetChild(i).gameObject.SetActive(true);
                Debug.Log(CharacterData.characterIndex2);
            }
        }
    }
}
