using UnityEngine;

public class MarkCharacterSelected : MonoBehaviour
{
    [SerializeField] bool isPlayer1 = true;
    [SerializeField] GameObject[] characters;
    [SerializeField] GameObject mark;

    // Update is called once per frame
    void Update()
    {
        string elegido = isPlayer1 ? CharacterData.characterIndex1 : CharacterData.characterIndex2;

        for (int i = 0; i < characters.Length; i++)
        {
            if (elegido == characters[i].name)
            {
                mark.SetActive(true);
                mark.transform.position = characters[i].transform.position;
                return;
            }
        }

        // Nadie elegido (o el jugador desconfirmó con el SelectorDePersonaje):
        // antes la marca quedaba prendida en el último personaje aunque ya no
        // estuviera elegido.
        mark.SetActive(false);
    }
}
