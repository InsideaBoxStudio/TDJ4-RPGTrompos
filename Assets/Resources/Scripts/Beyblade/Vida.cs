using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

public class Vida : MonoBehaviour
{
    [SerializeField] private float maxLife = 100;
    [SerializeField] private Color DamageColor = Color.red;
    [SerializeField] private GameObject lifeBar;
    [SerializeField] private GameObject JuiceEffects;
    [SerializeField] private bool isPracticeMode = false;
    [SerializeField] public float vidaActual;
    [SerializeField] public float timeDuration = 0.2f;

    public AudioSource audioSource;
    public float hitStopDuration = 0.05f;

    private float potenciaDeTiro = 0.5f;
    private float originalTimeScale = 1;
    [SerializeField] private SpriteRenderer Beyblade;

    void Start()
    {
        if (Beyblade != null) {
            foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
            {
                if (child.CompareTag("PlayerSprite"))
                {
                    Beyblade = child.GetComponent<SpriteRenderer>();
                    break;
                }
            }
        }

        audioSource = GetComponent<AudioSource>();

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

    public void Damage(int DamageCount, Vector3 damagePosition)
    {

        // Efecto de sonido
        audioSource.Play();

        // -----------------------
        // Efecto Visual
        // -----------------------

        // cambiar color momentaneamente
        Beyblade.color = DamageColor;
        Invoke("ReturnColor", timeDuration);

        // lanzar particulas
        GameObject juice = Instantiate(JuiceEffects);
        juice.transform.position = damagePosition;

        // numero de daño recibido
        TMP_Text damageNum = lifeBar.GetComponentInChildren<TMP_Text>();
        damageNum.text = DamageCount.ToString();
        Invoke("ReturnText", 1f);

        StartCoroutine(HitStop());

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
    }

    private void ReturnColor()
    {
        SpriteRenderer Beyblade = transform.gameObject.GetComponentInChildren<SpriteRenderer>();
        Beyblade.color = Color.white;
    }

    private IEnumerator HitStop()
    {
        if (Time.timeScale >= 0)
        {
            originalTimeScale = 1;
        }

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(0.1f);

        Time.timeScale = originalTimeScale;
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