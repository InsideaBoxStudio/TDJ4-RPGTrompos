using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircularBarState : MonoBehaviour
{
    public LineRenderer line;
    public float radius = 1.5f;
    public int segments = 100;

    [Header("Color")]
    public Color startColor = Color.blue;   // estado negativo (ej: congelado)
    public Color endColor = Color.red;      // estado positivo (ej: quemado)
    public float alpha = 0.5f;

    [Range(-100, 100)]
    public float stateValue = 0; // negativo = freeze, positivo = burn

    public float burnTickInterval = 1f; // cada cuánto se activa
    private float burnTimer = 0f;

    private float angle = 0;

    void Awake()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        line.loop = false;
    }

    void Update()
    {
        DrawCircle();
        UpdateColor();

        if (stateValue == 0)
        {
            Destroy(gameObject);
        }
        else
        {
            DecayState();

            HandleBurnTick();
        }
    }

    void DrawCircle()
    {
        float absValue = Mathf.Abs(stateValue);

        int visibleSegments = Mathf.Max(
            1,
            Mathf.RoundToInt((absValue / 100f) * segments)
        );

        line.positionCount = visibleSegments + 1;

        bool clockwise = stateValue >= 0;

        for (int i = 0; i <= visibleSegments; i++)
        {
            float t = (float)i / segments;

            angle = clockwise
                ? t * Mathf.PI * 2f
                : -t * Mathf.PI * 2f;

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    void UpdateColor()
    {
        float t = Mathf.InverseLerp(0f, 100f, Mathf.Abs(stateValue));

        // negativo → startColor, positivo → endColor
        Color baseColor = stateValue >= 0 ? endColor : startColor;

        Color currentColor = Color.Lerp(Color.clear, baseColor, t);
        currentColor.a = alpha;

        line.startColor = currentColor;
        line.endColor = currentColor;
    }

    // NUEVA FUNCIÓN: sumar o restar estado
    public void AddState(float value)
    {
        stateValue = Mathf.Clamp(stateValue + value, -100f, 100f);
    }

    // opcional: reset rápido
    public void ClearState()
    {
        stateValue = 0;
    }

    void HandleBurnTick()
    {
        if (stateValue >= 0)
        {
            burnTimer = 0f; // reset si no está quemado
            return;
        }

        burnTimer += Time.deltaTime;

        if (burnTimer >= burnTickInterval)
        {
            burnTimer -= burnTickInterval; // evita drift

            OnBurnTick();
        }
    }

    void OnBurnTick()
    {
        gameObject.GetComponentInParent<Vida>().Damage(1, transform.position);
    }

    void DecayState()
    {
        float decaySpeed = 1f; // cuánto baja por segundo

        if (stateValue > 0)
        {
            stateValue -= decaySpeed * Time.deltaTime;
            if (stateValue < 0) stateValue = 0;
        }
        else if (stateValue < 0)
        {
            stateValue += decaySpeed * Time.deltaTime;
            if (stateValue > 0) stateValue = 0;
        }
    }
}