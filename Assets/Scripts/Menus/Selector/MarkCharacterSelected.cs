using UnityEngine;

public class MarkCharacterSelected : MonoBehaviour
{
    [SerializeField] bool isPlayer1 = true;
    [SerializeField] GameObject[] characters;
    [SerializeField] GameObject mark;

    // Update is called once per frame
    void Update()
    {
        if ( isPlayer1 )
        {
            for( int i = 0; i < characters.Length; i++)
            {
                if( CharacterData.characterIndex1 == characters[i].name)
                {
                    mark.SetActive(true);
                    mark.transform.position = characters[i].transform.position;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (CharacterData.characterIndex2 == characters[i].name)
                {
                    mark.SetActive(true);
                    mark.transform.position = characters[i].transform.position;
                    break;
                }
            }
        }
    }
}
