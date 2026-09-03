using System;
using UnityEngine;

public class RotateBeyblade : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 360;

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, rotateSpeed) * Time.deltaTime);
    }
}
