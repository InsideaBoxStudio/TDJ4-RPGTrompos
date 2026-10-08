using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISelector : MonoBehaviour
{
    public void SeleccionarBoton(Button boton)
    {
        EventSystem.current.SetSelectedGameObject(boton.gameObject);
    }
}