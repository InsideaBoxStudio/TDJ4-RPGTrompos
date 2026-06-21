using UnityEngine;

public class CircleRenderer : MonoBehaviour
{
    public LineRenderer line;
    public float radius = 2f;
    public int segments = 100;

    void Start()
    {
        line.positionCount = segments + 1;
        line.loop = true;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}