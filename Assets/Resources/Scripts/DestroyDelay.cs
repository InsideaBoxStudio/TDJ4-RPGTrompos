using System;
using UnityEngine;

public class DestroyDelay : MonoBehaviour
{
    [SerializeField] private float destroyTime = 1f;
    private void Awake()
    {
        Invoke(nameof(Destroy), destroyTime);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
