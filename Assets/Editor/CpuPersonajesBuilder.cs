#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
//  CPU PERSONAJES BUILDER (Editor)
//  ----------------------------------------------------------------------------
//  PROBLEMA QUE RESUELVE
//  En la escena Practica solo el Ninja de la CPU estaba armado de verdad:
//   - CPU > Magus era una copia del Ninja (traía Pinchos, Shuriken y Clon) y
//     además tenía las referencias cruzadas con el Magus del Jugador 1 (su
//     "Esperar" movía el trompo del Jugador 1).
//   - CPU no tenía Caballero: elegirlo dejaba la arena sin trompo.
//  El código de la IA no era el problema (descubre solo los poderes de cualquier
//  personaje, ver IAIAction.cs); faltaba la escena.
//
//  QUÉ HACE
//  Menú:  Trompos IA > CPU > Construir Magus y Caballero
//   Duplica Jugador1 > Magus y Jugador1 > Knight con la duplicación de Unity (las
//   referencias internas quedan bien) y los convierte en CPU:
//    - nombre del trompo "0" -> "1" (varios poderes ordenan los Player por nombre),
//    - capa de colisión del Jugador 1 -> capa de la CPU,
//    - posición inicial y estado de la UI iguales a los del Ninja de la CPU,
//    - un AIBrain con los valores por defecto (igual que el del Ninja),
//    - lo anota en las listas de la escena (EndGame.Players y MusicManager).
//   El Ninja de la CPU no se toca.
//
//  Menú:  Trompos IA > CPU > Validar personajes
//   Para cada personaje de la CPU: lista los poderes que la IA va a encontrar y
//   avisa si algún componente apunta a OTRO personaje (el cruce de cables).
//   Se puede correr antes y después de construir, sin darle Play.
//
//  Todo se puede deshacer con Ctrl+Z. Después de construir, revisá con "Validar"
//  y guardá la escena (Ctrl+S).
// ============================================================================
public static class CpuPersonajesBuilder
{
    private const string ESCENA = "Practica";
    private const string RAIZ_CPU = "CPU";
    private const string RAIZ_JUGADOR1 = "Jugador1";
    private const string MOLDE_CPU = "Ninja";   // personaje de la CPU que ya funciona
    private static readonly string[] PERSONAJES = { "Magus", "Knight" };

    // ------------------------------------------------------------------------
    //  CONSTRUIR
    // ------------------------------------------------------------------------
    [MenuItem("Trompos IA/CPU/Construir Magus y Caballero")]
    public static void Construir()
    {
        if (!EscenaCorrecta(out Scene escena)) return;
        if (!BuscarBase(escena, out Transform cpu, out Transform jugador1, out Transform molde)) return;

        bool seguir = EditorUtility.DisplayDialog(
            "Construir Magus y Caballero de la CPU",
            "Va a REEMPLAZAR CPU > Magus (hoy es una copia del Ninja) y a CREAR CPU > Knight, " +
            "copiándolos del Jugador 1.\n\nEl Ninja de la CPU no se toca. " +
            "Se puede deshacer con Ctrl+Z.\n\n¿Seguir?",
            "Sí, construir", "Cancelar");
        if (!seguir) return;

        Undo.IncrementCurrentGroup();
        int grupo = Undo.GetCurrentGroup();

        var informe = new StringBuilder();
        foreach (string nombre in PERSONAJES)
        {
            string resultado = ConstruirUno(nombre, cpu, jugador1, molde);
            informe.AppendLine(resultado);
        }

        LimpiarListasNulas(escena);
        Undo.CollapseUndoOperations(grupo);
        EditorSceneManager.MarkSceneDirty(escena);

        Debug.Log("CpuPersonajesBuilder:\n" + informe);
        EditorUtility.DisplayDialog("Listo",
            informe + "\nAhora corré  Trompos IA > CPU > Validar personajes  y guardá la escena (Ctrl+S).",
            "OK");
    }

    private static string ConstruirUno(string nombre, Transform cpu, Transform jugador1, Transform molde)
    {
        Transform origen = jugador1.Find(nombre);
        if (origen == null) return $"{nombre}: NO existe en {RAIZ_JUGADOR1}, se saltea.";
        Transform trompoOrigen = origen.Find("0");
        if (trompoOrigen == null) return $"{nombre}: no encontré el trompo \"0\" dentro de {RAIZ_JUGADOR1}/{nombre}.";

        Transform trompoMolde = molde.Find("1");
        if (trompoMolde == null) return $"{nombre}: no encontré el trompo \"1\" dentro de {RAIZ_CPU}/{MOLDE_CPU}.";

        int capaJugador = trompoOrigen.gameObject.layer;
        int capaCpu = trompoMolde.gameObject.layer;

        // Si ya había uno (el Magus que era un Ninja), se reemplaza.
        Transform existente = cpu.Find(nombre);
        int indice = existente != null ? existente.GetSiblingIndex() : cpu.childCount;
        if (existente != null) Undo.DestroyObjectImmediate(existente.gameObject);

        // La duplicación de Unity remapea las referencias internas; las que salen
        // del personaje (RPGTurn, CountDownRPG, Estadio) siguen apuntando a los
        // objetos compartidos de la escena, que es justo lo que queremos.
        GameObject copia = Object.Instantiate(origen.gameObject, cpu);
        Undo.RegisterCreatedObjectUndo(copia, "Construir " + nombre + " de la CPU");
        copia.name = nombre;
        copia.transform.SetSiblingIndex(indice);

        // Mismo lugar y escala que el Ninja de la CPU.
        CopiarLocal(molde, copia.transform);

        Transform trompo = copia.transform.Find("0");
        trompo.name = "1";
        CopiarLocal(trompoMolde, trompo);

        int capasCambiadas = CambiarCapa(copia.transform, capaJugador, capaCpu);

        EspejarEstadoDeUI(molde, copia.transform);

        // AIBrain con los valores por defecto (los mismos que usa el del Ninja).
        // Las referencias las completa solo en Awake.
        Undo.AddComponent<AIBrain>(copia);

        CopiarReferenciasDeBarra(molde, copia.transform);
        AnotarEnLaEscena(trompo);

        return $"{nombre}: construido ({capasCambiadas} objetos pasaron a la capa de la CPU).";
    }

    private static void CopiarLocal(Transform desde, Transform hacia)
    {
        hacia.localPosition = desde.localPosition;
        hacia.localRotation = desde.localRotation;
        hacia.localScale = desde.localScale;
    }

    private static int CambiarCapa(Transform raiz, int capaVieja, int capaNueva)
    {
        int n = 0;
        foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (t.gameObject.layer != capaVieja) continue;
            t.gameObject.layer = capaNueva;
            n++;
        }
        return n;
    }

    // La UI de combate de la CPU no se muestra: igualamos qué objetos están
    // prendidos/apagados a los del Ninja de la CPU (que ya anda), y la configuración
    // del Canvas, para que las copias se comporten igual.
    private static void EspejarEstadoDeUI(Transform molde, Transform copia)
    {
        Transform canvasMolde = molde.Find("PlayerCanvas");
        Transform canvasCopia = copia.Find("PlayerCanvas");
        if (canvasMolde == null || canvasCopia == null) return;

        var rutas = new Dictionary<string, Transform>();
        foreach (Transform t in canvasCopia.GetComponentsInChildren<Transform>(true))
            rutas[RutaRelativa(canvasCopia, t)] = t;

        foreach (Transform t in canvasMolde.GetComponentsInChildren<Transform>(true))
        {
            if (rutas.TryGetValue(RutaRelativa(canvasMolde, t), out Transform par))
                par.gameObject.SetActive(t.gameObject.activeSelf);
        }

        Canvas cMolde = canvasMolde.GetComponent<Canvas>();
        Canvas cCopia = canvasCopia.GetComponent<Canvas>();
        if (cMolde != null && cCopia != null) EditorUtility.CopySerialized(cMolde, cCopia);
    }

    private static string RutaRelativa(Transform raiz, Transform t)
    {
        if (t == raiz) return "";
        var partes = new List<string>();
        for (Transform x = t; x != null && x != raiz; x = x.parent) partes.Add(x.name);
        partes.Reverse();
        return string.Join("/", partes);
    }

    // El Ninja de la CPU tiene asignadas la cuenta regresiva y el EndGame en su
    // barra de lanzamiento; las copias las traen vacías.
    private static void CopiarReferenciasDeBarra(Transform molde, Transform copia)
    {
        TimingBar barraMolde = molde.GetComponentInChildren<TimingBar>(true);
        TimingBar barraCopia = copia.GetComponentInChildren<TimingBar>(true);
        if (barraMolde == null || barraCopia == null) return;

        var origen = new SerializedObject(barraMolde);
        var destino = new SerializedObject(barraCopia);
        foreach (string campo in new[] { "countDown", "endGame" })
        {
            SerializedProperty o = origen.FindProperty(campo);
            SerializedProperty d = destino.FindProperty(campo);
            if (o != null && d != null) d.objectReferenceValue = o.objectReferenceValue;
        }
        destino.ApplyModifiedProperties();
    }

    // Lo que la escena sabe de cada trompo: el EndGame (para saber quién perdió)
    // y el MusicManager (volumen de los sonidos).
    private static void AnotarEnLaEscena(Transform trompo)
    {
        EndGame endGame = Object.FindFirstObjectByType<EndGame>(FindObjectsInactive.Include);
        if (endGame != null) AgregarAArray(endGame, "Players", trompo.gameObject);

        MusicManager musica = Object.FindFirstObjectByType<MusicManager>(FindObjectsInactive.Include);
        AudioSource audio = trompo.GetComponent<AudioSource>();
        if (musica != null && audio != null) AgregarAArray(musica, "soundSources", audio);
    }

    private static void AgregarAArray(Object componente, string campo, Object valor)
    {
        var so = new SerializedObject(componente);
        SerializedProperty arr = so.FindProperty(campo);
        if (arr == null || !arr.isArray) return;
        arr.arraySize++;
        arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = valor;
        so.ApplyModifiedProperties();
    }

    // Al borrar el Magus que era un Ninja, sus entradas en las listas quedan vacías.
    private static void LimpiarListasNulas(Scene escena)
    {
        EndGame endGame = Object.FindFirstObjectByType<EndGame>(FindObjectsInactive.Include);
        if (endGame != null) QuitarNulos(endGame, "Players");

        MusicManager musica = Object.FindFirstObjectByType<MusicManager>(FindObjectsInactive.Include);
        if (musica != null) QuitarNulos(musica, "soundSources");
    }

    private static void QuitarNulos(Object componente, string campo)
    {
        var so = new SerializedObject(componente);
        SerializedProperty arr = so.FindProperty(campo);
        if (arr == null || !arr.isArray) return;
        for (int i = arr.arraySize - 1; i >= 0; i--)
        {
            if (arr.GetArrayElementAtIndex(i).objectReferenceValue != null) continue;
            arr.DeleteArrayElementAtIndex(i);
        }
        so.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------------
    //  VALIDAR
    // ------------------------------------------------------------------------
    [MenuItem("Trompos IA/CPU/Validar personajes")]
    public static void Validar()
    {
        if (!EscenaCorrecta(out Scene escena)) return;
        if (!BuscarBase(escena, out Transform cpu, out Transform jugador1, out Transform molde)) return;

        var informe = new StringBuilder();
        bool todoBien = true;

        foreach (Transform personaje in cpu)
        {
            informe.AppendLine($"=== {RAIZ_CPU} > {personaje.name}");

            if (personaje.GetComponent<AIBrain>() == null)
            {
                informe.AppendLine("   FALTA el AIBrain");
                todoBien = false;
            }

            IAIAction[] poderes = personaje.GetComponentsInChildren<IAIAction>(true);
            informe.AppendLine($"   poderes que va a usar la IA ({poderes.Length}):");
            foreach (IAIAction p in poderes)
                informe.AppendLine($"      {p.GetType().Name}  costo {p.EnergyCost}  {(p.IsStrong ? "fuerte" : "basico")}");

            List<string> cruces = BuscarCruces(personaje, cpu, jugador1);
            if (cruces.Count > 0)
            {
                todoBien = false;
                informe.AppendLine("   REFERENCIAS CRUZADAS (apuntan a otro personaje):");
                foreach (string c in cruces) informe.AppendLine("      " + c);
            }
            else
            {
                informe.AppendLine("   referencias: OK (no apunta a otros personajes)");
            }
        }

        informe.AppendLine(todoBien ? "\nTODO OK." : "\nHAY PROBLEMAS: mirá arriba.");
        Debug.Log("CpuPersonajesBuilder.Validar:\n" + informe);
        EditorUtility.DisplayDialog("Validar CPU", informe.ToString(), "OK");
    }

    // Busca referencias de los componentes del personaje hacia objetos que
    // pertenecen a OTRO personaje (del Jugador 1 o de otro hijo de la CPU). Las
    // referencias a objetos compartidos de la escena (RPGTurn, Estadio, etc.) son
    // normales y no se reportan.
    private static List<string> BuscarCruces(Transform personaje, Transform cpu, Transform jugador1)
    {
        var cruces = new List<string>();
        foreach (MonoBehaviour mb in personaje.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            var so = new SerializedObject(mb);
            SerializedProperty it = so.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                Object destino = it.objectReferenceValue;
                if (destino == null) continue;

                Transform t = (destino as Component)?.transform ?? (destino as GameObject)?.transform;
                if (t == null || t.IsChildOf(personaje)) continue;

                bool deOtroPersonaje = t.IsChildOf(jugador1) || (t.IsChildOf(cpu) && t != cpu);
                if (deOtroPersonaje)
                    cruces.Add($"{mb.GetType().Name}.{it.name} -> {RutaCompleta(t)}");
            }
        }
        return cruces;
    }

    private static string RutaCompleta(Transform t)
    {
        var partes = new List<string>();
        for (Transform x = t; x != null; x = x.parent) partes.Add(x.name);
        partes.Reverse();
        return string.Join(" > ", partes);
    }

    // ------------------------------------------------------------------------
    //  Utilidades
    // ------------------------------------------------------------------------
    private static bool EscenaCorrecta(out Scene escena)
    {
        escena = SceneManager.GetActiveScene();
        if (escena.name == ESCENA) return true;
        EditorUtility.DisplayDialog("Escena equivocada",
            $"Abrí la escena \"{ESCENA}\" (ahora está abierta \"{escena.name}\") y probá de nuevo.", "OK");
        return false;
    }

    private static bool BuscarBase(Scene escena, out Transform cpu, out Transform jugador1, out Transform molde)
    {
        cpu = jugador1 = molde = null;
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            if (raiz.name == RAIZ_CPU) cpu = raiz.transform;
            else if (raiz.name == RAIZ_JUGADOR1) jugador1 = raiz.transform;
        }
        if (cpu != null) molde = cpu.Find(MOLDE_CPU);

        if (cpu != null && jugador1 != null && molde != null) return true;
        EditorUtility.DisplayDialog("Falta algo en la escena",
            $"No encontré en la raíz de la escena \"{RAIZ_CPU}\" (con \"{MOLDE_CPU}\" adentro) y/o \"{RAIZ_JUGADOR1}\".", "OK");
        return false;
    }
}
#endif
