using UnityEngine;

public class RotateObjectScript : MonoBehaviour
{
    [SerializeField] private bool invert;
    [SerializeField] private int addZ = 0;
    private GameObject cameraObject;

    void Start()
    {
        cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        if (!invert)
        {
            transform.rotation = cameraObject.transform.rotation;
        }
        else
        {
            transform.rotation = Quaternion.Euler(-0f, -0f, -cameraObject.transform.rotation.z + addZ);
        }
    }
}