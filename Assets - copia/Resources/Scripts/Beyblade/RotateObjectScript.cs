using UnityEngine;

public class RotateObjectScript : MonoBehaviour
{
    private GameObject cameraObject;

    void Start()
    {
        cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        transform.rotation = cameraObject.transform.rotation;
    }
}