using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Agrega la escena Tutorial a los Build Settings (si no esta).
//
// Por que existe: SceneManager.LoadScene("Tutorial") solo funciona con las escenas que estan en
// la lista de Build Settings, tanto en el build como en el Editor. En vez de editar a mano esa
// lista, la agrega Unity solo al abrir el proyecto; es lo unico que el tutorial necesita fuera de
// su propia carpeta. Si la escena ya esta, no hace nada.
[InitializeOnLoad]
public static class TutorialEnBuild
{
    private const string RutaEscena = "Assets/Scenes/Tutorial.unity";

    static TutorialEnBuild()
    {
        // delayCall: tocar los Build Settings durante la carga del dominio puede fallar.
        EditorApplication.delayCall += Asegurar;
    }

    private static void Asegurar()
    {
        if (!File.Exists(RutaEscena)) return;

        EditorBuildSettingsScene[] escenas = EditorBuildSettings.scenes;
        if (escenas.Any(e => e.path == RutaEscena)) return;

        EditorBuildSettings.scenes = escenas
            .Concat(new[] { new EditorBuildSettingsScene(RutaEscena, true) })
            .ToArray();

        Debug.Log("Tutorial: se agregó " + RutaEscena + " a los Build Settings.");
    }
}
