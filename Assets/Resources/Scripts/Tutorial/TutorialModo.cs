using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
//  ESTADO DEL TUTORIAL
//  ----------------------------------------------------------------------------
//  El tutorial vive en su PROPIA escena ("Tutorial", una copia de Practica con la pelea
//  contra la computadora) y un "director" la guia como una coreografia: pausa el juego
//  cada vez que el jugador tiene que hacer algo (ver TutorialDirector). La escena de
//  Practica no se toca, asi que la pelea normal no puede romperse por culpa del tutorial.
//  Este archivo solo guarda si el tutorial esta activo y deja todo como estaba al salir.
//
//  POR QUE SE RESTAURAN LOS DATOS: la dificultad y los personajes elegidos son
//  estaticos y sobreviven al cambio de escena. Si el tutorial dejara "Ninja" cargado,
//  la pantalla de seleccion de personajes arrancaria la proxima pelea sola (cree que ya
//  se eligieron los dos personajes) y se pasaria por alto la eleccion del jugador.
//
//  Nada de lo que ya existia se modifica: el tutorial se engancha desde afuera
//  (ver TutorialArranque).
// ============================================================================
public static class TutorialModo
{
    public const string EscenaEntrada = "SeleccionDificultad"; // pantalla previa a Practica: aca aparece el boton
    public const string EscenaCombate = "Tutorial";            // copia de Practica donde corre la clase guiada
    public const string PersonajeTutorial = "Ninja";           // el que tiene mas ataques distintos para probar

    public static bool Activo { get; private set; }

    // Lo que habia elegido el jugador antes de entrar al tutorial.
    private static AIDifficulty dificultadPrevia;
    private static string personaje1Previo = " ";
    private static string personaje2Previo = " ";

    public static void Iniciar()
    {
        if (Activo) return;

        dificultadPrevia = SeleccionDificultad.DificultadElegida;
        personaje1Previo = CharacterData.characterIndex1;
        personaje2Previo = CharacterData.characterIndex2;

        Activo = true;

        // Los dos personajes se fijan antes de cargar la escena: ActiveCharacter los lee en su Awake.
        CharacterData.characterIndex1 = PersonajeTutorial;
        CharacterData.characterIndex2 = PersonajeTutorial;
        SeleccionDificultad.DificultadElegida = AIDifficulty.Facil;

        SceneManager.LoadScene(EscenaCombate);
    }

    public static void Terminar()
    {
        if (!Activo) return;

        Activo = false;
        SeleccionDificultad.DificultadElegida = dificultadPrevia;
        CharacterData.characterIndex1 = personaje1Previo;
        CharacterData.characterIndex2 = personaje2Previo;
    }
}
