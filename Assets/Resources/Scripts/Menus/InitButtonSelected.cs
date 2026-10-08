using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class InitButtonSelected : MonoBehaviour
{
    [SerializeField] private MultiplayerEventSystem multiplayerEventSystem;
    private Button boton;

    void Start()
    {
        boton = GetComponent<Button>();

        if (boton == null)
            return;

        if (multiplayerEventSystem != null)
        {
            multiplayerEventSystem.SetSelectedGameObject(boton.gameObject);
        }
        else if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(boton.gameObject);
        }
    }

    void Update()
    {
        if (boton == null)
            return;

        GameObject selected = null;

        // Si tiene MultiplayerEventSystem, usa ese
        if (multiplayerEventSystem != null)
        {
            selected = multiplayerEventSystem.currentSelectedGameObject;

            if (selected == null || !selected.activeInHierarchy)
            {
                multiplayerEventSystem.SetSelectedGameObject(boton.gameObject);
            }
        }
        // Si no tiene MultiplayerEventSystem, usa el EventSystem normal
        else if (EventSystem.current != null)
        {
            selected = EventSystem.current.currentSelectedGameObject;

            if (selected == null || !selected.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(boton.gameObject);
            }
        }
    }
}