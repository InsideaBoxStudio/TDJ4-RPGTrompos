using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ConectedControls : MonoBehaviour
{
    [SerializeField] private Image image; // referencia a la imagen del canvas
    [SerializeField] private int connectedDeviceId = 0;
    [SerializeField] private Color ConnectedColor = new Color(0, 1, 1, 1); // cian
    [SerializeField] private Color DisconnectedColor = new Color(1, 1, 1, 0.25f); // semitransparente


    // Update is called once per frame
    void Update()
    {
        if (Gamepad.all.Count > connectedDeviceId)
        {
            image.color = ConnectedColor;
        }
        else
        {
            image.color = DisconnectedColor;
        }
    }
}
