using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircularBar : MonoBehaviour
{
    public LineRenderer line;
    public float radius = 1.5f;
    public int segments = 100;
    public bool clockwise = true;
    private float angle = 0;

    [Header("Color")]
    public Color startColor = Color.white;
    public Color endColor = Color.red;
    public float alpha = 0.5f;

    [Range(0, 100)]
    public float healthPercent = 100;

    void Start()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        line.loop = false;

        DrawCircle();
    }

    void Update()
    {
        DrawCircle();
        UpdateColor();
    }

    void DrawCircle()
    {
        int visibleSegments = Mathf.Max(
            1,
            Mathf.RoundToInt((healthPercent / 100f) * segments)
        );

        line.positionCount = visibleSegments + 1;

        for (int i = 0; i <= visibleSegments; i++)
        {
            if (clockwise)
            {
                angle = (float)i / segments * Mathf.PI * 2f;
            }
            else
            {
                angle = -(float)i / segments * Mathf.PI * 2f;
            }

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0));
        }
    }
    void UpdateColor()
    {
        float t = 1f - (healthPercent / 100f);

        Color currentColor = Color.Lerp(startColor, endColor, t);
        currentColor.a = alpha;

        line.startColor = currentColor;
        line.endColor = currentColor;
    }

    public void SetHealth(float percent)
    {
        healthPercent = Mathf.Clamp(percent, 0, 100);
    }
}
