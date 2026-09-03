using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuSceneManager : MonoBehaviour
{
    public static MenuSceneManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string configMenuScene = "ConfigMenu";

    private GameObject[] initMenuObjects;
    private bool isConfigOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OpenConfigMenu(string initMenuScene)
    {
        if (isConfigOpen) return;

        Scene initScene = SceneManager.GetSceneByName(initMenuScene);

        if (!initScene.IsValid() || !initScene.isLoaded)
        {
            Debug.LogWarning($"La escena {initMenuScene} no está cargada.");
            return;
        }

        initMenuObjects = initScene.GetRootGameObjects();

        SceneManager.LoadScene(configMenuScene, LoadSceneMode.Additive);

        PauseInitMenu();

        isConfigOpen = true;
    }

    public void CloseConfigMenu()
    {
        if (!isConfigOpen) return;

        ResumeInitMenu();

        Scene configScene = SceneManager.GetSceneByName(configMenuScene);

        if (configScene.IsValid() && configScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(configMenuScene);
        }

        isConfigOpen = false;
    }

    private void PauseInitMenu()
    {
        foreach (GameObject obj in initMenuObjects)
        {
            // Mantener activo el EventSystem
            if (obj.GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
                continue;

            obj.SetActive(false);
        }
    }

    private void ResumeInitMenu()
    {
        foreach (GameObject obj in initMenuObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}