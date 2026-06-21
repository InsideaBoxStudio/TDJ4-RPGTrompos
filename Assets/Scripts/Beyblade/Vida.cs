using UnityEngine;
using TMPro;

public class Vida : MonoBehaviour
{
    [SerializeField] private float maxLife = 100;
    [SerializeField] private Color DamageColor = Color.red;
    [SerializeField] private GameObject lifeBar;
    [SerializeField] private bool isPracticeMode = false;
    [SerializeField] public float vidaActual;
    [SerializeField] public float timeDuration = 0.2f;

    public AudioSource audioSource;

    private float potenciaDeTiro = 0.5f;
    private float initialScaleX;
    private float initialScaleX2;

    void Start()
    {
        /*
        initialScaleX = lifeBar.transform.localScale.x;
        initialScaleX2 = lifeBar.transform.GetChild(0).localScale.x;
        */
        if (!isPracticeMode) return;
        vidaActual = maxLife;

        audioSource = GetComponent<AudioSource>();
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

        // Efecto de sonido
        audioSource.Play();

        // -----------------------
        // Efecto Visual
        // -----------------------

        // cambiar color momentaneamente
        SpriteRenderer Beyblade = transform.gameObject.GetComponentInChildren<SpriteRenderer>();
        Beyblade.color = DamageColor;
        Invoke("ReturnColor", timeDuration);

        // lanzar particulas
        transform.GetComponent<ParticleSystem>().Play();

        // numero de daño recibido
        TMP_Text damageNum = lifeBar.GetComponentInChildren<TMP_Text>();
        damageNum.text = DamageCount.ToString();
        Invoke("ReturnText", 1f);

        // hacer temblar la barra de vida
        lifeBar.GetComponent<Shake>().StartShake(0.2f, 0.05f);

        // -----------------------
        // Sacar Vida
        // -----------------------

        vidaActual -= DamageCount;

        if (vidaActual <= 0)
        {
            vidaActual = 0;
            Time.timeScale = 0f;
        }

        float porcentajeVida = vidaActual / maxLife * 100;
        lifeBar.GetComponent<CircularBar>().healthPercent = porcentajeVida;

        Debug.Log(porcentajeVida);
        /*
        lifeBar.transform.localScale = new Vector3(
            initialScaleX * porcentajeVida, // reduccion proporcional al daño
            lifeBar.transform.localScale.y,
            lifeBar.transform.localScale.z
        );

        damageNum.transform.localScale = new Vector3(
            initialScaleX2 / porcentajeVida, // reduccion proporcional al daño
            damageNum.transform.localScale.y,
            damageNum.transform.localScale.z
        );
        */
    }

    private void ReturnColor()
    {
        SpriteRenderer Beyblade = transform.gameObject.GetComponentInChildren<SpriteRenderer>();
        Beyblade.color = Color.white;
    }

    private void ReturnText()
    {
        TMP_Text damageNum = lifeBar.GetComponentInChildren<TMP_Text>();
        damageNum.text = " ";
    }

    // Llamado por la IA (AIBrain) para cargarse vida llena sin usar la barra de timing.
    public void SetFullLife()
    {
        vidaActual = maxLife;
    }

    // Vida máxima actual (la usa el indicador de vida para mostrar "actual / máx").
    public float VidaMaxima => maxLife;
}