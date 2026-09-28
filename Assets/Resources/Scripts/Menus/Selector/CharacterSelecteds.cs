using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelecteds : MonoBehaviour
{
    [SerializeField] string sceneName;

    // Antes cargaba la pelea en el mismo instante en que elegía el segundo
    // jugador, así que la marca de su elección nunca llegaba a verse. Esta
    // pausa deja ver las dos marcas, y si alguien se arrepiente en ese rato
    // (vuelve a confirmar para desbloquear), la carga se cancela.
    [SerializeField] float esperaAntesDeCargar = 1f;

    private float tiempoConAmbosElegidos;

    void Update()
    {
        bool ambosEligieron =
            CharacterData.characterIndex1 != " " &&
            CharacterData.characterIndex2 != " ";

        if (!ambosEligieron)
        {
            tiempoConAmbosElegidos = 0f;
            return;
        }

        // Sin escalar: si el timeScale quedó en 0 la pausa no terminaría nunca.
        tiempoConAmbosElegidos += Time.unscaledDeltaTime;
        if (tiempoConAmbosElegidos >= esperaAntesDeCargar)
        {
            enabled = false; // una sola carga
            SceneManager.LoadScene(sceneName);
        }
    }
}
