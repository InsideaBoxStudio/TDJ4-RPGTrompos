using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ChangeMap : MonoBehaviour
{
    [SerializeField] private GameObject[] activeGameObject;
    [SerializeField] private string map = "elegir mapa";
    [SerializeField] private bool isCancelButton = false;
    public InputActionReference cancelAction;
    [SerializeField] private bool noSelected = false;
    [SerializeField] private float wait = 0.2f;

    private bool isSceneChanged = false;
    public void ChangeScene()
    {
        Invoke("WaitScene", wait);
        if (noSelected) return;

        for(int i = 0; i < activeGameObject.Length; i++)
        {
            activeGameObject[i].SetActive(true);
        }
    }

    void OnEnable()
    {
        if (cancelAction == null) return;
        cancelAction.action.Enable();
    }

    void OnDisable()
    {
        if (cancelAction == null) return;
        cancelAction.action.Disable();
    }

    private void Start()
    {
        Time.timeScale = 1;
    }

    void Update()
    {
        if (
            (CharacterData.characterIndex1 != " " &&
            CharacterData.characterIndex2 != " " &&
            isSceneChanged)
            || (noSelected && isSceneChanged) ||
            (isCancelButton && cancelAction.action.WasPressedThisFrame() && EventSystem.current.currentSelectedGameObject == gameObject))
        {
            SceneManager.LoadScene(map);
        }
    }

    private void WaitScene()
    {
        isSceneChanged = true;
    }
}
