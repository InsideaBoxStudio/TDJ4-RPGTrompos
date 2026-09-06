using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIBackNavigation : MonoBehaviour
{
    [SerializeField] private InputActionReference previousAction;
    [SerializeField] private Selectable objectToSelect;

    private void OnEnable()
    {
        if (previousAction == null)
        {
            Debug.LogError("❌ previousAction no está asignado.");
            return;
        }

        Debug.Log("🟢 UIBackNavigation habilitado");
        Debug.Log("Escuchando acción: " + previousAction.action.name);

        previousAction.action.performed += OnPrevious;
        previousAction.action.Enable();
    }

    private void OnDisable()
    {
        if (previousAction == null)
            return;

        previousAction.action.performed -= OnPrevious;
        previousAction.action.Disable();
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        Debug.Log("🔙 SE PRESIONÓ CÍRCULO / VOLVER");

        if (objectToSelect == null)
        {
            Debug.LogError("❌ objectToSelect no está asignado.");
            return;
        }

        if (EventSystem.current == null)
        {
            Debug.LogError("❌ No hay EventSystem en la escena.");
            return;
        }

        EventSystem.current.SetSelectedGameObject(objectToSelect.gameObject);
    }
}