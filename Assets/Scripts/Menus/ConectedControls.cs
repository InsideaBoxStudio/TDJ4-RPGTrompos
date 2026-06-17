using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ConectedControls : MonoBehaviour
{
    [SerializeField] private Image image; // referencia a la imagen del canvas
    [SerializeField] private int connectedDeviceId = 0;

    // Update is called once per frame
    void Update()
    {
        if (Gamepad.all.Count > connectedDeviceId)
        {
            image.color = Color.white;
        }
        else
        {
            image.color = new Color (1, 1, 1, 0.25f); // semitransparente
        }
    }
}
