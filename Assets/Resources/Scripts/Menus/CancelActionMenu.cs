using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CancelActionMenu : MonoBehaviour
{
    public InputActionReference cancelAction;
    public GameObject botonDestino;

    private void OnEnable()
    {
        cancelAction.action.Enable();
    }

    void Update()
    {
        if (cancelAction.action.WasPressedThisFrame())
        {
            EventSystem.current.SetSelectedGameObject(botonDestino);
        }
    }
}
