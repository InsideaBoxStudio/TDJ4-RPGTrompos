using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    private void Awake()
    {
        CharacterData.characterIndex1 = " ";
        CharacterData.characterIndex2 = " ";
    }

    public void Player1Select(string charIndex)
    {
        CharacterData.characterIndex1 = charIndex;
        Debug.Log(CharacterData.characterIndex1);
    }
    public void Player2Select(string charIndex)
    {
        CharacterData.characterIndex2 = charIndex;
        Debug.Log(CharacterData.characterIndex2);
    }
}