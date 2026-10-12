#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
//  CPU BASE BUILDER (Editor)
//  ----------------------------------------------------------------------------
//  PROBLEMA QUE RESUELVE
//  En la escena J1vsCPU el jugador usa el trompo "Base" (estructura nueva de Nemi:
//  estadísticas y ataques salen de PlayerSettings), pero los tres personajes de la CPU
//  (Ninja, Magus, Caballero) siguen armados con los scripts viejos de ataque. Por eso
//  la CPU no usaba los ataques nuevos. Además el Magus de la CPU tomaba la energía y el
//  punto de disparo del Ninja de la CPU (referencias cruzadas).
//
//  QUÉ HACE
//  Menú:  Trompos IA > CPU > Reconstruir CPU con trompo Base
//   Para cada personaje de la CPU (Ninja, Magus, Knight) reemplaza el cuerpo viejo por
//   una copia del trompo Base del Jugador 1, hecha con la duplicación de Unity (las
//   referencias internas quedan bien), y la convierte en CPU:
//    - nombre del trompo "0" -> "1", y capa de colisión del Jugador 1 -> la de la CPU,
//    - todos los índices de jugador (playerIndex, playersIndex, playerID) -> 1,
//    - la vida máxima de Vida en 0, para que la lea del preset y no de un valor fijo,
//    - la interfaz espejada a la derecha, como la tenía el cuerpo viejo,
//    - un PresetCPU con el aspecto del cuerpo viejo y los ataques / estadísticas de
//      cada personaje (se pueden ajustar en el Inspector), y un AIBrain,
//    - se anota en EndGame, MusicManager y CountDown, y se sacan las entradas viejas.
//
//  Menú:  Trompos IA > CPU > Validar CPU (trompo Base)
//   Revisa lo anterior sin darle Play, y avisa de referencias cruzadas entre personajes.
//
//  Se puede deshacer con Ctrl+Z. Después de construir, validá y guardá (Ctrl+S).
// ============================================================================
public static class CpuBaseBuilder
{
    private const string ESCENA = "J1vsCPU";
    private const string RAIZ_CPU = "CPU";
    private const string RAIZ_JUGADOR1 = "Jugador1";
    private const string CUERPO_BASE = "Base";
    private const int INDICE_CPU = 1;

    private const string CARPETA_ATAQUES = "Assets/Prefabs/UI/AttackOptions/P2Attacks/";
    private static readonly string[] SUBCARPETAS = { "", "New Folder/" };

    // Nombres de campos que guardan "de qué jugador es este trompo".
    private static readonly HashSet<string> NOMBRES_DE_INDICE = new HashSet<string>
    {
        "playerindex", "playersindex", "playerid"
    };

    // Un preset por personaje. Los números son un punto de partida: se ajustan después en
    // el Inspector del PresetCPU de cada personaje, sin tocar código.
    //   Ninja y Magus: las listas y estadísticas que ya traen las escenas de Nemi.
    //   Caballero: no tenía ninguna; se arma con sus ataques y estadísticas de peso alto.
    private class Preset
    {
        public string nombre;
        public string[] ataques;
        public string[] especiales;
        public float heaviness;
        public int health;
        public float maxWaitTime;
        public int maxEnergy;
        public int initEnergy;
        public float frictionStrength;
    }

    private static readonly Preset[] PRESETS =
    {
        new Preset { nombre = "Ninja",
            ataques = new[] { "BasicAttack", "Shuriken", "Spike", "Saw" },
            especiales = new[] { "Clone", "Poison", "Sustitution" },
            heaviness = 9f, health = 100, maxWaitTime = 2.5f, maxEnergy = 8, initEnergy = 4, frictionStrength = 3f },
        new Preset { nombre = "Magus",
            ataques = new[] { "BasicAttack", "MagicShot", "MagicTurret", "RotateOrbs" },
            especiales = new[] { "FireElement", "IceLement" },
            heaviness = 8f, health = 90, maxWaitTime = 3f, maxEnergy = 12, initEnergy = 10, frictionStrength = 2f },
        new Preset { nombre = "Knight",
            ataques = new[] { "BasicAttack", "SwordThrust", "Onda Expansiva", "Gravity" },
            especiales = new[] { "Espadazo", "Shield" },
            heaviness = 10f, health = 120, maxWaitTime = 2.5f, maxEnergy = 8, initEnergy = 4, frictionStrength = 3f },
    };

    // ------------------------------------------------------------------------
    //  CONSTRUIR
    // ------------------------------------------------------------------------
    [MenuItem("Trompos IA/CPU/Reconstruir CPU con trompo Base")]
    public static void Construir()
    {
        if (!EscenaCorrecta(out Scene escena)) return;
        if (!BuscarBase(escena, out Transform cpu, out Transform baseJugador)) return;

        bool seguir = EditorUtility.DisplayDialog(
            "Reconstruir la CPU con el trompo Base",
            "Va a REEMPLAZAR los personajes de la CPU (Ninja, Magus y Caballero) por copias del trompo " +
            "Base del Jugador 1, con los ataques nuevos.\n\nLos cuerpos viejos se borran (podés " +
            "recuperarlos con Ctrl+Z o con git). El Jugador 1 no se toca.\n\n¿Seguir?",
            "Sí, reconstruir", "Cancelar");
        if (!seguir) return;

        Undo.IncrementCurrentGroup();
        int grupo = Undo.GetCurrentGroup();

        var informe = new StringBuilder();
        foreach (Preset p in PRESETS)
            informe.AppendLine(ConstruirUno(p, cpu, baseJugador));

        LimpiarListasNulas();
        Undo.CollapseUndoOperations(grupo);
        EditorSceneManager.MarkSceneDirty(escena);

        Debug.Log("CpuBaseBuilder:\n" + informe);
        EditorUtility.DisplayDialog("Listo",
            informe + "\nAhora corré  Trompos IA > CPU > Validar CPU (trompo Base)  y guardá la escena (Ctrl+S).",
            "OK");
    }

    private static string ConstruirUno(Preset p, Transform cpu, Transform baseJugador)
    {
        Transform trompoBase = baseJugador.Find("0");
        if (trompoBase == null) return $"{p.nombre}: no encontré el trompo \"0\" dentro de {RAIZ_JUGADOR1}/{CUERPO_BASE}.";

        // Lo que hay que rescatar del cuerpo viejo ANTES de borrarlo.
        Transform viejo = cpu.Find(p.nombre);
        Transform molde = viejo != null ? viejo : PrimerHijo(cpu);
        Transform trompoMolde = molde != null ? molde.Find("1") : null;
        Sprite sprite = SpriteDe(trompoMolde);
        Transform imagenViejaUI = BuscarImagenDeLaInterfaz(viejo != null ? viejo : molde);

        int capaJugador = trompoBase.gameObject.layer;
        int capaCpu = trompoMolde != null ? trompoMolde.gameObject.layer : capaJugador;

        int indice = viejo != null ? viejo.GetSiblingIndex() : cpu.childCount;

        // El cuerpo viejo se borra AL FINAL, cuando ya se leyó todo lo que hacía falta de él
        // (posición, interfaz): borrarlo antes dejaría esas lecturas apuntando a objetos
        // destruidos. Mientras tanto conviven dos hijos con el mismo nombre; el resto del
        // armado trabaja con la referencia a la copia, no busca por nombre.

        // La duplicación de Unity remapea las referencias internas; las que salen del
        // personaje (RPGTurn, CountDownRPG, Estadio) siguen apuntando a los objetos
        // compartidos de la escena, que es justo lo que queremos.
        GameObject copia = Object.Instantiate(baseJugador.gameObject, cpu);
        Undo.RegisterCreatedObjectUndo(copia, "Construir " + p.nombre + " de la CPU");
        copia.name = p.nombre;
        copia.transform.SetSiblingIndex(indice);
        if (molde != null) CopiarLocal(molde, copia.transform);

        Transform trompo = copia.transform.Find("0");
        trompo.name = "1"; // varios scripts ordenan o buscan a los Player por nombre
        if (trompoMolde != null) CopiarLocal(trompoMolde, trompo);

        int capas = CambiarCapa(copia.transform, capaJugador, capaCpu);
        int indices = PonerIndiceDeJugador(copia);
        PonerVidaMaximaEnCero(copia);
        bool espejada = EspejarInterfaz(imagenViejaUI, copia.transform);

        ConfigurarPreset(copia, p, sprite);
        Undo.AddComponent<AIBrain>(copia);

        // Arranca apagado, como los cuerpos viejos: ActiveCharacter prende solo al elegido.
        copia.SetActive(false);

        // Ya no hace falta nada del cuerpo viejo.
        if (viejo != null) Undo.DestroyObjectImmediate(viejo.gameObject);

        RegistrarEnLaEscena(copia.transform, trompo, baseJugador);

        return $"{p.nombre}: construido ({capas} objetos a la capa de la CPU, {indices} índices de jugador a 1, " +
               $"interfaz espejada: {(espejada ? "sí" : "NO ENCONTRÉ la del cuerpo viejo")}, " +
               $"sprite: {(sprite != null ? sprite.name : "SIN SPRITE")}).";
    }

    private static Transform PrimerHijo(Transform t) => t.childCount > 0 ? t.GetChild(0) : null;

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

    // El sprite que tenía el cuerpo viejo (cada personaje se ve distinto).
    private static Sprite SpriteDe(Transform trompoViejo)
    {
        if (trompoViejo == null) return null;
        foreach (SpriteRenderer sr in trompoViejo.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.gameObject.name == "Beyblade" && sr.sprite != null) return sr.sprite;
        }
        return null;
    }

    // ------------------------------------------------------------------------
    //  Índices de jugador y vida
    // ------------------------------------------------------------------------
    // Pone en 1 TODOS los campos enteros que dicen "de qué jugador soy". Se recorren por
    // SerializedObject (no por reflexión): lo que se edita es lo que Unity guarda.
    private static int PonerIndiceDeJugador(GameObject copia)
    {
        int n = 0;
        foreach (MonoBehaviour mb in copia.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            var so = new SerializedObject(mb);
            SerializedProperty it = so.GetIterator();
            bool cambio = false;
            while (it.NextVisible(true))
            {
                if (it.propertyType != SerializedPropertyType.Integer) continue;
                if (!NOMBRES_DE_INDICE.Contains(it.name.ToLowerInvariant())) continue;
                if (it.intValue == INDICE_CPU) continue;
                it.intValue = INDICE_CPU;
                cambio = true;
                n++;
            }
            if (cambio) so.ApplyModifiedProperties();
        }
        return n;
    }

    // Vida.Start() solo lee la vida del preset si maxLife <= 0. El trompo Base la trae
    // fija en 100, y entonces la salud del preset no valdría nada.
    private static void PonerVidaMaximaEnCero(GameObject copia)
    {
        Vida vida = copia.GetComponentInChildren<Vida>(true);
        if (vida == null) return;
        var so = new SerializedObject(vida);
        SerializedProperty maxLife = so.FindProperty("maxLife");
        if (maxLife == null) return;
        maxLife.floatValue = 0f;
        so.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------------
    //  Interfaz espejada
    // ------------------------------------------------------------------------
    // El trompo Base trae su panel de energía y velocidad a la izquierda (es el del
    // Jugador 1). El cuerpo viejo de la CPU lo tenía a la derecha: se copian sus anclas,
    // su pivote y su posición.
    private static Transform BuscarImagenDeLaInterfaz(Transform personaje)
    {
        if (personaje == null) return null;
        Transform canvas = personaje.Find("PlayerCanvas");
        if (canvas == null) return null;

        foreach (Transform hijo in canvas)
        {
            if (hijo.name.Trim() != "BattleUI") continue; // un hijo se llama "BattleUI " (con espacio)
            Transform imagen = hijo.Find("Image");
            if (imagen != null) return imagen;
        }
        return null;
    }

    private static bool EspejarInterfaz(Transform imagenVieja, Transform copia)
    {
        if (imagenVieja == null) return false;
        Transform imagenNueva = BuscarImagenDeLaInterfaz(copia);
        if (imagenNueva == null) return false;

        RectTransform desde = imagenVieja as RectTransform;
        RectTransform hacia = imagenNueva as RectTransform;
        if (desde == null || hacia == null) return false;

        hacia.anchorMin = desde.anchorMin;
        hacia.anchorMax = desde.anchorMax;
        hacia.pivot = desde.pivot;
        hacia.anchoredPosition = desde.anchoredPosition;
        hacia.sizeDelta = desde.sizeDelta;
        hacia.localScale = desde.localScale;
        return true;
    }

    // ------------------------------------------------------------------------
    //  Preset
    // ------------------------------------------------------------------------
    private static void ConfigurarPreset(GameObject copia, Preset p, Sprite sprite)
    {
        PresetCPU preset = Undo.AddComponent<PresetCPU>(copia);
        var so = new SerializedObject(preset);

        so.FindProperty("playerIndex").intValue = INDICE_CPU;
        so.FindProperty("layerSprite").objectReferenceValue = sprite;
        AsignarPrefabs(so.FindProperty("attackOptionsPrefabs"), p.ataques);
        AsignarPrefabs(so.FindProperty("specialOptionsPrefabs"), p.especiales);
        so.FindProperty("heaviness").floatValue = p.heaviness;
        so.FindProperty("health").intValue = p.health;
        so.FindProperty("maxWaitTime").floatValue = p.maxWaitTime;
        so.FindProperty("maxEnergy").intValue = p.maxEnergy;
        so.FindProperty("initEnergy").intValue = p.initEnergy;
        so.FindProperty("frictionStrength").floatValue = p.frictionStrength;

        so.ApplyModifiedProperties();
    }

    private static void AsignarPrefabs(SerializedProperty arreglo, string[] nombres)
    {
        arreglo.arraySize = nombres.Length;
        for (int i = 0; i < nombres.Length; i++)
        {
            GameObject prefab = CargarPrefabDeAtaque(nombres[i]);
            if (prefab == null) Debug.LogWarning($"CpuBaseBuilder: no encontré el prefab de ataque \"{nombres[i]}\" en {CARPETA_ATAQUES}");
            arreglo.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
        }
    }

    private static GameObject CargarPrefabDeAtaque(string nombre)
    {
        foreach (string sub in SUBCARPETAS)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CARPETA_ATAQUES + sub + nombre + ".prefab");
            if (prefab != null) return prefab;
        }
        return null;
    }

    // ------------------------------------------------------------------------
    //  Listas de la escena
    // ------------------------------------------------------------------------
    // Lo que la escena sabe de cada trompo: EndGame (quién perdió), MusicManager
    // (volumen de los sonidos) y CountDown (qué interfaz se prende al terminar la cuenta).
    private static void RegistrarEnLaEscena(Transform contenedor, Transform trompo, Transform baseJugador)
    {
        EndGame endGame = Object.FindFirstObjectByType<EndGame>(FindObjectsInactive.Include);
        if (endGame != null) AgregarAArray(endGame, "Players", trompo.gameObject);

        MusicManager musica = Object.FindFirstObjectByType<MusicManager>(FindObjectsInactive.Include);
        AudioSource audio = trompo.GetComponent<AudioSource>();
        if (musica != null && audio != null) AgregarAArray(musica, "soundSources", audio);

        // CountDown prende, al terminar la cuenta, la interfaz del Jugador 1: se prende la
        // equivalente de este trompo (misma ruta relativa dentro del cuerpo).
        CountDown cuenta = Object.FindFirstObjectByType<CountDown>(FindObjectsInactive.Include);
        if (cuenta != null) ReplicarEnCountDown(cuenta, contenedor, baseJugador);
    }

    private static void ReplicarEnCountDown(CountDown cuenta, Transform contenedor, Transform baseJugador)
    {
        var so = new SerializedObject(cuenta);
        SerializedProperty lista = so.FindProperty("objectsToActive");
        if (lista == null || !lista.isArray) return;

        var nuevos = new List<GameObject>();
        for (int i = 0; i < lista.arraySize; i++)
        {
            GameObject entrada = lista.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
            if (entrada == null || !entrada.transform.IsChildOf(baseJugador)) continue;

            Transform equivalente = contenedor.Find(RutaRelativa(baseJugador, entrada.transform));
            if (equivalente != null) nuevos.Add(equivalente.gameObject);
        }

        foreach (GameObject g in nuevos)
        {
            lista.arraySize++;
            lista.GetArrayElementAtIndex(lista.arraySize - 1).objectReferenceValue = g;
        }
        so.ApplyModifiedProperties();
    }

    private static string RutaRelativa(Transform raiz, Transform t)
    {
        if (t == raiz) return "";
        var partes = new List<string>();
        for (Transform x = t; x != null && x != raiz; x = x.parent) partes.Add(x.name);
        partes.Reverse();
        return string.Join("/", partes);
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

    // Al borrar los cuerpos viejos, sus entradas en las listas quedan vacías.
    private static void LimpiarListasNulas()
    {
        EndGame endGame = Object.FindFirstObjectByType<EndGame>(FindObjectsInactive.Include);
        if (endGame != null) QuitarNulos(endGame, "Players");

        MusicManager musica = Object.FindFirstObjectByType<MusicManager>(FindObjectsInactive.Include);
        if (musica != null) QuitarNulos(musica, "soundSources");

        CountDown cuenta = Object.FindFirstObjectByType<CountDown>(FindObjectsInactive.Include);
        if (cuenta != null)
        {
            QuitarNulos(cuenta, "objectsToActive");
            QuitarNulos(cuenta, "objectsToDestroy");
        }
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
    [MenuItem("Trompos IA/CPU/Validar CPU (trompo Base)")]
    public static void Validar()
    {
        if (!EscenaCorrecta(out Scene escena)) return;
        if (!BuscarBase(escena, out Transform cpu, out Transform baseJugador)) return;

        EndGame endGame = Object.FindFirstObjectByType<EndGame>(FindObjectsInactive.Include);
        var informe = new StringBuilder();
        bool todoBien = true;

        foreach (Transform personaje in cpu)
        {
            informe.AppendLine($"=== {RAIZ_CPU} > {personaje.name}");

            void Falla(string texto) { informe.AppendLine("   PROBLEMA: " + texto); todoBien = false; }

            if (personaje.GetComponent<AIBrain>() == null) Falla("falta el AIBrain");

            PresetCPU preset = personaje.GetComponent<PresetCPU>();
            if (preset == null) Falla("falta el PresetCPU");
            else informe.AppendLine("   " + ResumenDelPreset(preset));

            Transform trompo = personaje.Find("1");
            if (trompo == null) Falla("no hay un trompo llamado \"1\"");
            else
            {
                DefPlayerID id = trompo.GetComponent<DefPlayerID>();
                if (id == null) Falla("el trompo no tiene DefPlayerID");
                else if (LeerEntero(id, "playerID") != INDICE_CPU) Falla($"DefPlayerID.playerID vale {LeerEntero(id, "playerID")} (tiene que ser {INDICE_CPU})");

                if (endGame != null && !EstaEnLista(endGame, "Players", trompo.gameObject)) Falla("el trompo no está en EndGame.Players");
            }

            if (personaje.gameObject.activeSelf) Falla("el personaje arranca ENCENDIDO (tiene que arrancar apagado)");

            foreach (string cruce in BuscarCruces(personaje, cpu, baseJugador)) Falla("referencia cruzada: " + cruce);
        }

        informe.AppendLine(todoBien ? "\nTODO OK." : "\nHAY PROBLEMAS: mirá arriba.");
        Debug.Log("CpuBaseBuilder.Validar:\n" + informe);
        EditorUtility.DisplayDialog("Validar CPU", informe.ToString(), "OK");
    }

    private static string ResumenDelPreset(PresetCPU preset)
    {
        var so = new SerializedObject(preset);
        return $"preset: {Contar(so, "attackOptionsPrefabs")} ataques, {Contar(so, "specialOptionsPrefabs")} especiales, " +
               $"vida {so.FindProperty("health").intValue}, energía {so.FindProperty("initEnergy").intValue}/{so.FindProperty("maxEnergy").intValue}, " +
               $"sprite {(so.FindProperty("layerSprite").objectReferenceValue != null ? "sí" : "NO")}";
    }

    private static string Contar(SerializedObject so, string campo)
    {
        SerializedProperty arr = so.FindProperty(campo);
        int total = arr.arraySize, vacios = 0;
        for (int i = 0; i < total; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == null) vacios++;
        return vacios == 0 ? total.ToString() : $"{total} ({vacios} VACÍOS)";
    }

    private static int LeerEntero(Object o, string campo)
    {
        SerializedProperty p = new SerializedObject(o).FindProperty(campo);
        return p != null ? p.intValue : -1;
    }

    private static bool EstaEnLista(Object componente, string campo, Object valor)
    {
        SerializedProperty arr = new SerializedObject(componente).FindProperty(campo);
        if (arr == null || !arr.isArray) return false;
        for (int i = 0; i < arr.arraySize; i++)
            if (arr.GetArrayElementAtIndex(i).objectReferenceValue == valor) return true;
        return false;
    }

    // Referencias de los componentes del personaje hacia objetos de OTRO personaje (del
    // Jugador 1 o de otro hijo de la CPU). Las referencias a objetos compartidos de la
    // escena (RPGTurn, Estadio, etc.) son normales y no se reportan.
    private static List<string> BuscarCruces(Transform personaje, Transform cpu, Transform baseJugador)
    {
        Transform jugador1 = baseJugador.parent;
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

                bool deOtro = (jugador1 != null && t.IsChildOf(jugador1)) || (t.IsChildOf(cpu) && t != cpu);
                if (deOtro) cruces.Add($"{mb.GetType().Name}.{it.name} -> {RutaCompleta(t)}");
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

    private static bool BuscarBase(Scene escena, out Transform cpu, out Transform baseJugador)
    {
        cpu = baseJugador = null;
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            if (raiz.name == RAIZ_CPU) cpu = raiz.transform;
            else if (raiz.name == RAIZ_JUGADOR1) baseJugador = raiz.transform.Find(CUERPO_BASE);
        }

        if (cpu != null && baseJugador != null) return true;
        EditorUtility.DisplayDialog("Falta algo en la escena",
            $"No encontré en la escena \"{RAIZ_CPU}\" y/o \"{RAIZ_JUGADOR1}\" con el trompo \"{CUERPO_BASE}\" adentro.", "OK");
        return false;
    }
}
#endif
