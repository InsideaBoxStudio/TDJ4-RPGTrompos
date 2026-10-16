using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
//  ENGANCHE DEL TUTORIAL (no toca ninguna escena ni script existente)
//  ----------------------------------------------------------------------------
//  Se registra solo al iniciar el juego y mira cada escena que se carga:
//    - SeleccionDificultad (la entrada a Practica): agrega el boton "Tutorial".
//    - Practica con el tutorial activo: crea el director que guia la pelea.
//    - Cualquier otra escena: da el tutorial por terminado y restaura lo elegido.
//
//  POR QUE ASI Y NO EDITANDO LAS ESCENAS: el equipo pidio no tocar lo que ya funciona.
//  Agregar el boton y el director desde codigo deja las escenas intactas, y para
//  sacar el tutorial alcanza con borrar la carpeta Tutorial.
// ============================================================================
public static class TutorialArranque
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Registrar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena; // por si el dominio se recarga sin reiniciar
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (modo != LoadSceneMode.Single) return; // las escenas aditivas (config) no cuentan

        if (escena.name == TutorialModo.EscenaCombate && TutorialModo.Activo)
        {
            new GameObject("TutorialDirector").AddComponent<TutorialDirector>();
            return;
        }

        // Cualquier otra escena: se salio del tutorial (volvio al menu, se reinicio, etc.).
        TutorialModo.Terminar();

        if (escena.name == TutorialModo.EscenaEntrada)
        {
            new GameObject("TutorialBoton").AddComponent<TutorialBoton>();
        }
    }
}
