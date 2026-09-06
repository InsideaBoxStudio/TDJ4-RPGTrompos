using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionTester : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            Debug.LogError("❌ No asignaste el Input Action Asset.");
            return;
        }

        foreach (var actionMap in inputActions.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                action.performed += OnActionPerformed;
            }

            actionMap.Enable();
        }

        Debug.Log("✅ INPUT ACTION TESTER ACTIVADO");
    }

    private void OnDisable()
    {
        if (inputActions == null)
            return;

        foreach (var actionMap in inputActions.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                action.performed -= OnActionPerformed;
            }

            actionMap.Disable();
        }
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        InputAction action = context.action;

        string actionMapName = action.actionMap.name;
        string actionName = action.name;
        string controlName = context.control.name;
        string controlPath = context.control.path;

        // Buscar el binding correspondiente
        string bindingPath = "No encontrado";

        int bindingIndex = action.GetBindingIndexForControl(context.control);

        if (bindingIndex >= 0)
        {
            bindingPath = action.bindings[bindingIndex].effectivePath;
        }

        Debug.Log(
            "\n" +
            "====================================\n" +
            "🎮 INPUT DETECTADO\n" +
            "====================================\n" +
            "Action Map : " + actionMapName + "\n" +
            "Action     : " + actionName + "\n" +
            "Binding    : " + bindingPath + "\n" +
            "Control    : " + controlName + "\n" +
            "Path       : " + controlPath + "\n" +
            "===================================="
        );
    }
}