using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InitButtonSelected : MonoBehaviour
{
    void Start()
    {
        Button boton = GetComponent<Button>();

        if (boton == null) return;
        EventSystem.current.SetSelectedGameObject(boton.gameObject);
    }
}
