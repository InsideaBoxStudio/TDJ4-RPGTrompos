using UnityEngine;

// Corre antes que cualquier otro script de la escena: así apaga los trompos no
// elegidos ANTES de que hagan su Awake y se anoten en el sistema de turnos.
// Si se apagaran después, quedarían registrados como un tercer jugador fantasma.
[DefaultExecutionOrder(-1000)]
public class ActiveCharacter : MonoBehaviour
{
    [SerializeField] private bool isPlayer1 = false;

    void Awake()
    {
        string elegido = isPlayer1 ? CharacterData.characterIndex1 : CharacterData.characterIndex2;

        // Sin elección (se dio Play directo en esta escena desde el editor): se
        // deja la escena como está, con los trompos que ya estaban prendidos.
        if (!HayHijoLlamado(elegido)) return;

        // Antes solo prendía al elegido: si en la escena había otro trompo ya
        // prendido (pasaba con los Knight del 1VS1), aparecían 3 en la arena.
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject hijo = transform.GetChild(i).gameObject;
            hijo.SetActive(hijo.name == elegido);
        }
        Debug.Log(elegido);
    }

    private bool HayHijoLlamado(string nombre)
    {
        for (int i = 0; i < transform.childCount; i++)
            if (transform.GetChild(i).name == nombre) return true;
        return false;
    }
}
