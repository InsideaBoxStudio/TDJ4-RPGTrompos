using UnityEngine;
using TMPro;
using System.Collections;

public class CountDown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private GameObject[] objectsToDestroy; //objetos a eliminar despues de la cuenta regresiva
    [SerializeField] private GameObject[] objectsToActive; //objetos a ocultar despues de la cuenta regresiva
    public int countDownTime = 3;
    private AudioSource audioSource;
    public bool finished = false; // true cuando la cuenta regresiva inicial terminó (la IA espera esto)

    void Awake()
    {
        Time.timeScale = 0f;
        audioSource = GetComponent<AudioSource>();
    }
    
    void Start()
    {
        // Se vuelve a congelar acá porque el Awake no alcanza: TimeScaleController
        // pone timeScale = 1 en SU Awake, y el orden entre Awakes no está
        // garantizado. Si el suyo corría después, los trompos arrancaban sin
        // esperar la cuenta regresiva. Start corre cuando ya terminaron todos.
        Time.timeScale = 0f;
        StartCoroutine(CountDownCoroutine());
    }

    IEnumerator CountDownCoroutine()
    {
        while (countDownTime > 0)
        {
            countDownText.text = countDownTime.ToString();
            yield return new WaitForSecondsRealtime(1f);
            countDownTime--;
            audioSource.Play();

            if (countDownTime == 0)
            {
                countDownText.text = "GO!";
            }
        }

        //esperar un segundo antes de ocultar el texto
        yield return new WaitForSecondsRealtime(1f);

        //ocultar texto
        countDownText.text = "";
        //destruir objetos
        foreach (GameObject obj in objectsToDestroy)
        {
            Destroy(obj);
        }

        foreach (GameObject obj in objectsToActive)
        {
            obj.SetActive(true);
        }

        //reanudar tiempo
        Time.timeScale = 1f;
        finished = true; // ahora sí la pelea empieza (la IA puede actuar)
    }
}