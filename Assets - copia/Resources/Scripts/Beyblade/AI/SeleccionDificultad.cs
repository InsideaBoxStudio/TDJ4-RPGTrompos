using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private string sceneName = "InitMenu";
    [SerializeField] private float time;
    [SerializeField] private GameObject DesactiveObject;
    [SerializeField] private GameObject ActiveObject;

    // Dificultad elegida, accesible desde cualquier escena (static = sobrevive al cambio de escena).
    public static AIDifficulty DificultadElegida = AIDifficulty.Normal;

    public void ElegirFacil()   { Elegir(AIDifficulty.Facil); }
    public void ElegirNormal()  { Elegir(AIDifficulty.Normal); }
    public void ElegirDificil() { Elegir(AIDifficulty.Dificil); }

    private void Elegir(AIDifficulty dif)
    {
        DificultadElegida = dif;
        ActiveObject.SetActive(true);
        DesactiveObject.SetActive(false);
    }

    private void Update()
    {
        // Confirmar/volver con joystick o teclado. Pasa por Controles (ver
        // Controles.cs), asi que acepta buttonEast/buttonSouth del joystick y
        // Enter / Espacio / Backspace / Esc del teclado.
        bool confirmar = Controles.Volver() || Controles.Confirmar();

        if (ActiveObject.activeSelf == true) // si el selector de personajes esta activo
        {
            if (confirmar)
            {
                // ActiveObject.SetActive(false);
                // DesactiveObject.SetActive(true);
            }

            if (
                CharacterData.characterIndex1 != " " &&
                CharacterData.characterIndex2 != " "
                )
            {
                sceneName = escenaPractica;
                NextScene();
            }
        }
        else // si el selector de personajes no esta activo
        {
            if (confirmar)
            {
                Invoke("NextScene", time);
            }
        }
    }

    private void NextScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
