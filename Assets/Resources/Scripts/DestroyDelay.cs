using System;
using UnityEngine;

public class DestroyDelay : MonoBehaviour
{
    [SerializeField] private int destroyTime = 1;
    private void Awake()
    {
        Invoke(nameof(Destroy), destroyTime);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
