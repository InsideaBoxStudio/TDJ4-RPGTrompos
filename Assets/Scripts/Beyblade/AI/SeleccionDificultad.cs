using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
//  SELECCIÓN DE DIFICULTAD
//  ----------------------------------------------------------------------------
//  Guarda la dificultad elegida en la pantalla de selección y la lleva a la
//  escena de Práctica. El AIBrain la lee al iniciar.
//
//  Se usa así: los 3 botones (Fácil/Normal/Difícil) llaman a ElegirFacil(),
//  ElegirNormal() o ElegirDificil(); cada uno guarda la dificultad y carga la
//  escena de práctica.
// ============================================================================
public class SeleccionDificultad : MonoBehaviour
{
    [SerializeField] private string escenaPractica = "Practica";

    // Dificultad elegida, accesible desde cualquier escena (static = sobrevive al cambio de escena).
    public static AIDifficulty DificultadElegida = AIDifficulty.Normal;

    public void ElegirFacil()   { Elegir(AIDifficulty.Facil); }
    public void ElegirNormal()  { Elegir(AIDifficulty.Normal); }
    public void ElegirDificil() { Elegir(AIDifficulty.Dificil); }

    private void Elegir(AIDifficulty dif)
    {
        DificultadElegida = dif;
        SceneManager.LoadScene(escenaPractica);
    }
}
