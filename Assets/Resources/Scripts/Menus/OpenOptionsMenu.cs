using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpenOptionsMenu : MonoBehaviour
{
    [SerializeField] private string map = "ConfigMenu";
    public int width = 1280;
    public int height = 720;
    private Camera targetCamera;
    public Image display;
    private Texture2D snapshot;

    private bool isSceneChanged = false;

    private void Awake()
    {
        targetCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    public void ChangeScene()
    {
        // guardar una imagen de la escena actual para mostrarla en el menu de opciones
        // Capture();
        Scene thisScene = SceneManager.GetActiveScene();
        Debug.Log(thisScene.name);
        thisScene.GetRootGameObjects()[0].SetActive(false);

        // cargar la escena de opciones
        SceneManager.LoadScene(map, LoadSceneMode.Additive);
        isSceneChanged = true;

        // cambiar imagen de fondo del menu
        // display = GameObject.FindGameObjectWithTag("SnapshotImage").GetComponent<Image>();

        // Sprite sprite = Sprite.Create(
        //    snapshot,
        //    new Rect(0, 0, snapshot.width, snapshot.height),
        //    new Vector2(0.5f, 0.5f)
        // );

        // display.sprite = sprite;

        // desactivar el objeto raiz de la escena actual para que no se vea
    }

    // Update is called once per frame
    void Update()
    {
        if (isSceneChanged) return;

        if (Controles.Pausa()) // joystick + teclado (Esc) — ver Controles.cs
        {
            ChangeScene();
        }
    }

    public void Capture()
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt;

        snapshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        targetCamera.Render();

        RenderTexture.active = rt;
        snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        snapshot.Apply();

        targetCamera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(rt);
    }
}