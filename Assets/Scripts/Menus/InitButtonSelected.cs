using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InitButtonSelected : MonoBehaviour
{
    private Button boton;

    void Start()
    {
        boton = GetComponent<Button>();

        if (boton != null)
            EventSystem.current.SetSelectedGameObject(boton.gameObject);
    }

    void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null || !selected.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(boton.gameObject);
        }
    }
}