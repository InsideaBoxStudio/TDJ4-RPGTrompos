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
    [SerializeField] public int vidaActual;
    [SerializeField] public float timeDuration = 0.2f;

    public AudioSource audioSource;
    public float hitStopDuration = 0.05f;

    private float potenciaDeTiro = 0.5f;
    private float originalTimeScale = 1;
    private bool lastTurn = false;
    private float lastAddWaitTime = 2f;
    [SerializeField] private SpriteRenderer Beyblade;

    void Start()
    {
        if (maxLife <= 0)
        {
            maxLife = PlayerSettings.maxHealth[int.Parse(gameObject.name)];
        }

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
        vidaActual = (int)maxLife;
    }

    void Update()
    {
        if (transform.GetComponent<CheckPlayerTurn>().isTurnActive)
        {
            lastTurn = true;
        }
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
            maxLife = maxLife * potenciaDeTiro;
            vidaActual = (int)maxLife;
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

        if (DamageCount > 0) // si quita vida cambiar a rojo
        {
            damageNum.color = Color.red;
        }
        else if (DamageCount < 0) // si da vida cambiar a verde
        {
            damageNum.color = Color.green;
        }
        else // si no quita ni da vida, dejar transparente
        {
            damageNum.color = new Color(1f, 1f, 1f, 0f);
        }

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

        // -----------------------
        // Sumar tiempo de espera por daño
        // -----------------------

        if (lastTurn)
        {
            lastAddWaitTime = 2f;
            lastTurn = false;
        }
        else
        {
            lastAddWaitTime = lastAddWaitTime + 1f;
        }
        transform.GetComponent<CheckPlayerTurn>().PlayerChoseAnAction(DamageCount / lastAddWaitTime, 1000f, true);
    }

    private void ReturnColor()
    {
        SpriteRenderer Beyblade = transform.gameObject.GetComponentInChildren<SpriteRenderer>();
        Beyblade.color = Color.white;
    }

    private IEnumerator HitStop()
    {
        if (Time.timeScale <= 0)
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

    // Cura al trompo sin pasar de la vida máxima. Devuelve true si curó algo (false
    // si ya estaba lleno o si ya perdió: una cura no revive).
    //
    // Antes los objetos de curar llamaban a Damage() con un valor negativo. Funcionaba,
    // pero cada cura disparaba el destello rojo de golpe, el sonido de golpe, la pausa
    // de impacto, y además podía pasar la vida máxima (la barra mostraba más de 100 %).
    // Acá solo se mueve la barra y se muestra el "+N" en verde.
    public bool Heal(int amount)
    {
        if (amount <= 0 || vidaActual <= 0) return false;

        int tope = (int)maxLife;
        int curado = Mathf.Min(amount, tope - vidaActual);
        if (curado <= 0) return false;

        vidaActual += curado;
        lifeBar.GetComponent<CircularBar>().healthPercent = vidaActual / maxLife * 100;

        TMP_Text numero = lifeBar.GetComponentInChildren<TMP_Text>();
        numero.color = Color.green;
        numero.text = "+" + curado;
        CancelInvoke(nameof(ReturnText));
        Invoke(nameof(ReturnText), 1f);
        return true;
    }

    // Llamado por la IA (AIBrain) para cargarse vida llena sin usar la barra de timing.
    public void SetFullLife()
    {
        vidaActual = (int)maxLife;
    }

    // Vida máxima actual (la usa el indicador de vida para mostrar "actual / máx").
    public float VidaMaxima => maxLife;
}