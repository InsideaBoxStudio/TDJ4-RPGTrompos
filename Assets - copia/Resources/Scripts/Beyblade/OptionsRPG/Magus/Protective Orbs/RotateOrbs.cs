using UnityEngine;

public class RotateOrbs : MonoBehaviour
{
    [SerializeField] float speedRotation = 120f;

    void Update()
    {
        transform.Rotate(0f, 0f, speedRotation * Time.deltaTime);

        if (transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
