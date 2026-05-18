using UnityEngine;

public class Vida : MonoBehaviour
{
    [SerializeField] private float maxLife = 100;
    [SerializeField] private GameObject lifeBar;
    [SerializeField] private bool isPracticeMode = false;
    [SerializeField] public float vidaActual;
    private float potenciaDeTiro = 2;
    private float initialScaleX;

    void Start()
    {
        initialScaleX = lifeBar.transform.localScale.x;
        if (!isPracticeMode) return;
        vidaActual = maxLife;
    }

    public void WaitForInfo(float distanceFromCenter)
    {
        potenciaDeTiro = distanceFromCenter;

        if (potenciaDeTiro == 2 && !isPracticeMode)
        {
            // No se presionó el botón
            vidaActual = 0;
        }
        else
        {
            maxLife = maxLife * (1 - potenciaDeTiro);
            vidaActual = maxLife;
        }
    }

    public void Damage(int DamageCount)
    {
        vidaActual -= DamageCount;

        if (vidaActual <= 0)
        {
            vidaActual = 0;
            Time.timeScale = 0f;
        }

        float porcentajeVida = vidaActual / maxLife;
        lifeBar.transform.localScale = new Vector3(
            initialScaleX * porcentajeVida, // reduccion proporcional al daño
            lifeBar.transform.localScale.y,
            lifeBar.transform.localScale.z);
    }
}